using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lumen.Api.Graphics
{
    public abstract class Canvas : ILedCanvas
    {
        public virtual string Name
        {
            get { return GetType().Name; }
        }

        protected LedColor[] pixels { get; set; }
     
        public uint PixelCount
        {
            get { return Width * Height; }
        }

        public uint Width { get; protected set; }
        public uint Height { get; protected set; }


        protected Canvas(uint width, uint height = 1)
        {
            Width = width;
            Height = height;
            pixels = InitializePixels<LedColor>((int)PixelCount);
        }

        /// <summary>
        /// Set the canvas up and initialize the pixel array given the specified width and height
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>

        public virtual void Initialize(uint width, uint height = 1)
        {
            Width = width;
            Height = height;
            pixels = InitializePixels<LedColor>((int)PixelCount);
        }

        protected static T[] InitializePixels<T>(int length) where T : new()
        {
            T[] array = new T[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = new T();
            }
            return array;
        }

        public virtual LedColor[] GetPixels()
        {
            return pixels;
        }

        public void FillSolid(LedColor color)
        {
            for (uint x = 0; x < Width; x++)
            for (uint y = 0; y < Height; y++)
                DrawPixel(x, y, color);
        }

        public void SetPixel(uint x, LedColor color)
        {
            pixels[x] = color;
        }

        public void BlendPixel(uint x, LedColor color)
        {
            LedColor c1 = GetPixel(x);
            var c2 = c1 + color;
         //   Console.WriteLine(c2.ToString());
            SetPixel(x, c1 + color);
        }
        public abstract LedColor GetPixel(uint x, uint y = 0);

        public abstract void DrawPixel(uint x, LedColor color);

        public abstract void DrawPixel(uint x, uint y, LedColor color);
        public virtual void DrawPixels(double fPos, double count, LedColor color)
        {
            double availFirstPixel = 1 - (fPos - (uint)(fPos));
            double amtFirstPixel = Math.Min(availFirstPixel, count);
            count = Math.Min(count, PixelCount - fPos);
            if (fPos >= 0 && fPos < PixelCount)
                BlendPixel((uint)fPos, color.FadeToBlackBy(1.0 - amtFirstPixel));

            fPos += amtFirstPixel;
            //fPos %= DotCount;
            count -= amtFirstPixel;

            while (count >= 1.0)
            {
                if (fPos >= 0 && fPos < PixelCount)
                {
                    BlendPixel((uint)fPos, color);
                    count -= 1.0;
                }
                fPos += 1.0;
            }

            if (count > 0.0)
            {
                if (fPos >= 0 && fPos < PixelCount)
                    BlendPixel((uint)fPos, color.FadeToBlackBy(1.0 - count));
            }
        }
    }
}
