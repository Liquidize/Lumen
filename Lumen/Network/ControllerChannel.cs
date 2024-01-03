using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Lumen.Api.Graphics;
using Lumen.Interop;
using Lumen.Server;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Serilog;

namespace Lumen.Network
{
    public class ControllerChannel
    {
        static ConcurrentDictionary<string, ControllerSocket> _hostSockets = new ConcurrentDictionary<string, ControllerSocket>();

        [JsonProperty("host")]
        public string Host { get; protected set; }

        [JsonProperty("name")]
        public string Name { get; protected set; }
        [JsonProperty("useCompression")]
        public bool UseCompression { get; protected set; } = false;

        [JsonProperty("channel")]
        public byte Channel { get; protected set; } = 0;

        [JsonProperty("swapRedGreen")]
        public bool SwapRedGreen { get; protected set; } = false;

        [JsonProperty("batchSize")]
        public int BatchSize { get; protected set; } = 1;


        [JsonProperty("framesPerBuffer")]
        public uint FramesPerBuffer { get; protected set; } = 21;

        [JsonIgnore]
        public const double PercentBufferUse = 0.70;


        [JsonProperty("reversed")]
        public bool Reversed { get; protected set; } = false;

        [JsonIgnore]
        private const double BatchTimeout = 1.0;
        private ConcurrentQueue<byte[]> _dataQueue = new ConcurrentQueue<byte[]>();

        private Thread _thread;

        [JsonIgnore]
        public int QueueDepth {  get {  return _dataQueue.Count; } }


        [JsonIgnore]
        public const int MaxQueueDepth = 99;

        [JsonProperty("offset")]
        public uint Offset { get; protected set; } = 0;

        [JsonProperty("width")]
        public uint Width { get; protected set; }

        [JsonProperty("height")]
        public uint Height { get; protected set; } = 1;

        private Location _location;
        private DateTime _lastBatchTime = DateTime.UtcNow;
        private DateTime _lastSendTime = DateTime.UtcNow;

        [JsonIgnore] public ControllerResponse Response;

        [JsonIgnore]
        public ControllerSocket? ControllerSocket
        {
            get { return ControllerSocketForHost(Host); }
        }

        internal bool HasSocket
        {
            get
            {
                return ControllerSocket != null && ControllerSocket.Socket != null;
            }
        }

        internal bool IsReadyForData
        {
            get
            {
                ControllerSocket controllerSocket = GetControllerSocket();
                if (null == controllerSocket ||
                    controllerSocket.Socket == null ||
                    !controllerSocket.Socket.Connected ||
                    QueueDepth > MaxQueueDepth)
                    return false;
                return true;
            }
        }

        private const UInt16 WIFI_COMMAND_PIXELDATA = 0;
        private const UInt16 WIFI_COMMAND_VU = 1;
        private const UInt16 WIFI_COMMAND_CLOCK = 2;
        private const UInt16 WIFI_COMMAND_PIXELDATA64 = 3;

        public ControllerChannel(string hostName, string name, uint width, uint height = 1, uint offset = 0, bool useCompression = false, byte channel = 0, bool swapRedGreen = false, int batchSize = 1)
        {
            Host = hostName;
            Name = name;
            Width = width;
            Height = height;
            Offset = offset;
            UseCompression = useCompression;
            Channel = channel;
            SwapRedGreen = swapRedGreen;
            BatchSize = batchSize;

        }

        public static ControllerSocket? ControllerSocketForHost(string host)
        {
            if (_hostSockets.ContainsKey(host))
            {
                _hostSockets.TryGetValue(host, out ControllerSocket controller);
                return controller;
            }
            return null;
        }

        public void StartThread()
        {
            _thread = new Thread(ThreadConnectionLoop);
            _thread.Name = $"{Host} Worker Thread";
            _thread.IsBackground = true;
            _thread.Priority = ThreadPriority.BelowNormal;
            _thread.Start();
        }

        public bool CompressAndEnqueueData(LedColor[] pixels, DateTime startTime)
        {
            if (_hostSockets.ContainsKey(Host) == false && (DateTime.UtcNow - _lastSendTime).TotalSeconds < 2)
            {
                Log.Information($"Too early to retry for host {Host}");
                return false;
            }

            _lastSendTime = DateTime.UtcNow;

            ControllerSocket? socket = ControllerSocketForHost(Host);
            if (socket == null)
            {
                Log.Information("Socket is null on compression and queue");
                _hostSockets[Host] = socket;
            }

            if (QueueDepth > MaxQueueDepth)
            {
                Log.Information($"Queue full discarding frame for {Host}");
                return false;
            }

            if (SwapRedGreen)
            {
                foreach (var led in pixels)
                {
                    var temp = led.R;
                    led.R = led.G;
                    led.G = temp;
                }
            }

            byte[] raw = GetDataFrame(pixels, startTime);
            byte[] compressed = UseCompression ? CompressData(raw) : raw;
            if (compressed.Length >= raw.Length)
            {
                compressed = raw;
            }

            _dataQueue.Enqueue(compressed);
            return true;

        }



        private byte[] CompressData(byte[] raw)
        {
            const int HEADER_TAG = 0x44415645;
            byte[] compressed = FastLedInterop.Compress(raw);
            byte[] frame = FastLedInterop.CombineByteArrays(FastLedInterop.DWORDToBytes((uint)HEADER_TAG),
                FastLedInterop.DWORDToBytes((uint)compressed.Length),
                FastLedInterop.DWORDToBytes((uint)raw.Length),
                FastLedInterop.DWORDToBytes(0x12345678),
                compressed);
            return frame;
        }

        [JsonIgnore]
        public double TimeOffset
        {
            get
            {
                if (_location == null) return 1.0;

                if (_location.FramesPerSecond == 0)                // No speed indication yet, can't guess at offset, assume 1 second for now
                    return 1.0;

                double offset = (FramesPerBuffer * PercentBufferUse) / _location.FramesPerSecond;
                return offset;
            }
        }

        protected byte[] GetDataFrame(LedColor[] pixels, DateTime startTime)
        {

            double epoch = (startTime.Ticks - DateTime.UnixEpoch.Ticks + (TimeOffset * TimeSpan.TicksPerSecond)) / (double)TimeSpan.TicksPerSecond;

            ulong seconds = (ulong)epoch;                                       // Whole part of time number (left of the decimal point)
            ulong uSeconds = (ulong)((epoch - (int)epoch) * 1000000);           // Fractional part of time (right of the decimal point)

            var data = FastLedInterop.GetColorBytesAtOffset(pixels, Offset, Width * Height, Reversed, SwapRedGreen);
            return FastLedInterop.CombineByteArrays(FastLedInterop.WORDToBytes(WIFI_COMMAND_PIXELDATA64),      // Offset, always zero for us
                FastLedInterop.WORDToBytes((UInt16)Channel),               // LED channel on ESP32
                FastLedInterop.DWORDToBytes((UInt32)data.Length / 3),      // Number of LEDs
                FastLedInterop.ULONGToBytes(seconds),                      // Timestamp seconds (64 bit)
                FastLedInterop.ULONGToBytes(uSeconds),                     // Timestmap microseconds (64 bit)
                data);                                                 // Color Data
        }


        private void ThreadConnectionLoop()
        {
            for (;;)
            {
                ControllerSocket socket = _hostSockets.GetOrAdd(Host, (hostname) =>
                {
                    return new ControllerSocket(hostname);
                });

                //Log.Information("Socket host is " + Host);

                if (QueueDepth >= MaxQueueDepth)
                {
                    _dataQueue.Clear();
                    Log.Warning($"Discaring data for jammed socket {Host}");
                    _hostSockets.TryRemove(Host, out ControllerSocket oldSocket);
                    continue;
                }

                if (!socket.EnsureConnected())
                {
                    _hostSockets.TryRemove(Host, out ControllerSocket oldSocket);
                    continue;
                }

                if (ShouldSendBatch)
                {
                    _lastBatchTime = DateTime.UtcNow;

                    byte[] msg =
                        FastLedInterop.CombineByteArrays(_dataQueue.DequeueChunk(_dataQueue.Count()).ToArray());
                    if (msg.Length > 0)
                    {
                        try
                        {
                            uint bytesSent = 0;
                            if (!socket.IsDead)
                            {
                                bytesSent = socket.SendData(msg, ref Response);
                            }

                            if (bytesSent != msg.Length)
                            {
                                Log.Warning($"Could not write all bytes so closing the socket for {Host}.");
                                _hostSockets.TryRemove(Host, out ControllerSocket oldSocket);
                            }

                        }
                        catch (SocketException ex)
                        {
                            Log.Error(ex, $"Exception occured when writing to socket for {Host}");
                            _hostSockets.TryRemove(Host, out ControllerSocket oldSocket);
                        }
                    }

                }
                Thread.Sleep(10);

            }
        }

        [JsonIgnore]
        bool ShouldSendBatch
        {
            get
            {
                if (_location is null)
                    return false;

                if (_dataQueue.Count() > _location.FramesPerSecond)
                {
                    // If a full second has accumulated
                    return true;
                }

                if (_dataQueue.Any())
                    if ((DateTime.UtcNow - _lastBatchTime).TotalSeconds > BatchTimeout)
                        return true;

                if (_dataQueue.Count() >= BatchSize)
                {
                    return true;
                }

                return false;
            }
        }



        public ControllerSocket? GetControllerSocket()
        {
            if (_hostSockets.ContainsKey(Host))
            {
                _hostSockets.TryGetValue(Host, out ControllerSocket socket);
                return socket;
            }

            return null;
        }

        internal void SetLocation(Location location)
        {
            _location = location;
        }
    }
}
