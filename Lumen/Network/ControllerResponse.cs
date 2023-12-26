using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lumen.Network
{
    public struct ControllerResponse
    {
        public uint Size;
        public uint FlashVersion;
        public double CurrentClock;
        public double OldestPacket;
        public double NewestPacket;
        public double Brightness;
        public double WifiSignal;
        public uint BufferSize;
        public uint BufferPos;
        public uint FpsDrawing;
        public uint Watts;

        public void Reset()
        {
            Size = 0;
            FlashVersion = 0;
            CurrentClock = 0;
            OldestPacket = 0;
            NewestPacket = 0;
            Brightness = 0;
            WifiSignal = 0;
            BufferSize = 0;
            BufferPos = 0;
            FpsDrawing = 0;
            Watts = 0;
        }

    }
}
