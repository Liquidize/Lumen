using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lumen.Api.Graphics
{
    /// <summary>
    /// A forced 1D implementation of Canvas
    /// </summary>
    public class Canvas1D : Canvas
    {
        public override string Name
        {
            get { return "Canvas1D"; }
        }

        // We use this constructor to initialize it in Lumen dynamically
        public Canvas1D() : base(0, 1)
        {

        }


        public override void DrawPixel(uint x, LedColor color)
        {
           if (x < 0 || x >= Width) return;
            pixels[x] = color;
        }

        public override void DrawPixel(uint x, uint y, LedColor color)
        {
           DrawPixel(x, color);
        }



        public LedColor GetPixel(uint x)
        {
            return GetPixel(x, 0);
        }

        public override LedColor GetPixel(uint x, uint y = 0)
        {
            if (x < 0 || x >= Width) return ColorLibrary.Black;
            
            return pixels[x];
        }
    }
}
