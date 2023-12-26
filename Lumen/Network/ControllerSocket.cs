using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace Lumen.Network
{
    public class ControllerSocket
    {
        private Socket _socket;
        private IPAddress _address;
        private IPEndPoint _endPoint;
        private DateTime _lastDataTime;
        private uint _bytesSentSinceFrame = 0;

        public string HostName { get; private set; }

        public bool IsDead { get; protected set; } = false;

        public string FirmwareVersion { get; private set; }

        public Socket Socket { get { return _socket; } }

        public uint BytesPerSecond
        {
            get
            {
                double deltaSeconds = (DateTime.UtcNow - _lastDataTime).TotalSeconds;
                if (deltaSeconds < 0.001) return 0;
                return (uint)(_bytesSentSinceFrame / deltaSeconds);
            }
        }

        public bool IsConnected
        {
            get
            {
                return _socket != null && _socket.Connected;
            }
        }


        public ControllerSocket(string host)
        {
            HostName = host;

            var entry = Dns.GetHostByName(host);
            _address = entry.AddressList[0];
            _endPoint = new IPEndPoint(_address, 49152);
        }

        public bool EnsureConnected()
        {
            if (IsDead) return false;
            if (_socket != null && _socket.Connected) return true;
            if (_endPoint == null) return false;

            try
            {
                if (DateTime.UtcNow - _lastDataTime < TimeSpan.FromSeconds(1))
                {
                    Log.Warning("Bailing connection as too early!");
                    return true;
                }

                _lastDataTime = DateTime.UtcNow;
                _socket = new Socket(_address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _socket.Connect(_endPoint);
                _bytesSentSinceFrame = 0;
                Log.Information($"Connected to {_endPoint}.");
                return true;
            }
            catch (SocketException ex)
            {
                Log.Error(ex, "Unable to connect to controller.");
                IsDead = true;
                return false;
            }
        }

        unsafe public uint SendData(byte[] data, ref ControllerResponse response)
        {
            uint result = (uint)_socket.Send(data);
            if (result != data.Length)
            {
                IsDead = true;
                return result;
            }

            TimeSpan timeSinceLastSent = DateTime.UtcNow - _lastDataTime;
            if (timeSinceLastSent > TimeSpan.FromSeconds(10.0))
            {
                _lastDataTime = DateTime.UtcNow;
                _bytesSentSinceFrame = 0;
            }
            else
            {
                _bytesSentSinceFrame += result;
            }

            int cbToRead = sizeof(ControllerResponse);
            byte[] buffer = new byte[cbToRead];

            while (_socket.Available >= cbToRead)
            {
                var readBytes = _socket.Receive(buffer, cbToRead, SocketFlags.None);
                if (readBytes >= sizeof(ControllerResponse) && buffer[0] >= sizeof(ControllerResponse))
                {
                    GCHandle pinnedArray = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    IntPtr pointer = pinnedArray.AddrOfPinnedObject();
                    response = Marshal.PtrToStructure<ControllerResponse>(pointer);
                    pinnedArray.Free();
                }

                FirmwareVersion = "v" + response.FlashVersion;
            }

            return result;

        }



    }
}
