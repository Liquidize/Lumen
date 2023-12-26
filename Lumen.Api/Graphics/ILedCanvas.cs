using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lumen.Api.Graphics
{
    public interface ILedCanvas
    {
        uint Width { get;  }
        uint Height { get;  }
        uint PixelCount { get;  }

        LedColor[] GetPixels();
        LedColor GetPixel(uint x, uint y = 0);

        void DrawPixel(uint x, LedColor color);

        void DrawPixel(uint x, uint y, LedColor color);

        void DrawPixels(double fPos, double count, LedColor color);

        void FillSolid(LedColor color);

    }
}
