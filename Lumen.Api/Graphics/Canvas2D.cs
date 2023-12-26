using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lumen.Api.Graphics
{
    /// <summary>
    /// A 2D implementation of Canvas
    /// </summary>
    public class Canvas2D : Canvas
    {
        public override string Name
        {
            get { return "Canvas2D"; }
        }

        public Canvas2D() : base(0, 0)
        {

        }

        public Canvas2D(uint width, uint height) : base(width, height)
        {

        }

        public override LedColor GetPixel(uint x, uint y = 0)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return ColorLibrary.Black;
            }

            return pixels[y * Width + x];
        }

        public override void DrawPixel(uint x, LedColor color)
        {
            DrawPixel(x, 0, color);
        }

        public override void DrawPixel(uint x, uint y = 0, LedColor color = null)
        {
            if (x >= Width || y >= Height)
            {
                return;
            }

            var index = y * Width + x;
            pixels[index] = color;
        }
    }
}
