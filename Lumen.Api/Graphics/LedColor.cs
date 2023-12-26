using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lumen.Api.Graphics
{
    /// <summary>
    /// A 24-bit RGB Color
    /// </summary>
    [Serializable] 
    public class LedColor
    {
        /// <summary>
        /// Red color value stored as 8 bits.
        /// </summary>
        [JsonProperty("r")]
        public byte R { get; set; }
        /// <summary>
        /// Green color value stored as 8 bits.
        /// </summary>
        [JsonProperty("g")]
        public byte G { get; set; }
        /// <summary>
        /// Blue color value stored as 8 bits.
        /// </summary>
        [JsonProperty("b")]
        public byte B { get; set; }


       
        private static Random _random = new Random();

        public LedColor()
        {
            R = 150;
            G = 60;
            B = 0;
        }

        public LedColor(byte r, byte g, byte b)
        {
            R =r; G = g; B = b;
        }

        public LedColor(string hex)
        {
            var color = FromHex(hex);
            R = color.R; G = color.G; B = color.B;
        }

        public LedColor(uint color)
        {

            R = (byte)(color >> 16);
            G = (byte)(color >> 8);
            B = (byte)color;
        }

        public LedColor(LedColor other)
        {
            R = other.R; G = other.G; B = other.B;
        }

        public override string ToString()
        {
            return $"rgb({R},{G},{B})";
        }

        /// <summary>
        /// Parses a Led Color object from a Hex string.
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static LedColor FromHex(string hex)
        {
            if (hex.StartsWith("#"))
            {
                hex = hex.Substring(1);
            }

            if (hex.Length != 6)
            {
                throw new ArgumentException("Invalid hex color string. It should be in the format RRGGBB.",
                    nameof(hex));
            }

            byte red = Convert.ToByte(hex.Substring(0, 2), 16);
            byte green = Convert.ToByte(hex.Substring(2, 2), 16);
            byte blue = Convert.ToByte(hex.Substring(4, 2), 16);

            return new LedColor(red, green, blue);

        }

        /// <summary>
        /// Attempt to parse a hex string out into an Led Color
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryParseHex(string hex, out LedColor result)
        {
            result = null;
            
            if (hex.StartsWith("#"))
            {
                hex = hex.Substring(1);
            }

            if (hex.Length != 6)
            {
                return false;
            }

            if (!byte.TryParse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, null, out byte red) || 
                !byte.TryParse(hex.Substring(2,2), System.Globalization.NumberStyles.HexNumber, null, out byte green) ||
                !byte.TryParse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber, null, out byte blue))
            {
                return false;
            }

            result = new LedColor(red, green, blue);
            return true;

        }

        /// <summary>
        /// Creates an Led Color object from Hsv values
        /// </summary>
        /// <param name="hue"></param>
        /// <param name="saturation"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static LedColor HSVToRGB(double hue, double saturation = 1.0, double value = 1.0)
        {
            hue %= 360;

            if (saturation <= 0.0) // Handle the case where saturation is <= 0
            {
                int v = Convert.ToInt32(value * 255);
                return new LedColor((byte)v, (byte)v, (byte)v); // Return grayscale color
            }

            double hh = hue / 60.0;
            int i = (int)Math.Floor(hh);
            double f = hh - i;

            value *= 255;
            int vInt = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            switch (i)
            {
                case 0:
                    return new LedColor((byte)vInt, (byte)t, (byte)p);

                case 1:
                    return new LedColor((byte)q, (byte)vInt, (byte)p);

                case 2:
                    return new LedColor((byte)p, (byte)vInt, (byte)t);

                case 3:
                    return new LedColor((byte)p, (byte)q, (byte)vInt);

                case 4:
                    return new LedColor((byte)t, (byte)p, (byte)vInt);

                default: // case 5:
                    return new LedColor((byte)vInt, (byte)p, (byte)q);
            }
        }

        public void RGBToHSV(out double hue, out double sat, out double val)
        {
            Color color = Color.FromArgb(R, G, B);
            hue = color.GetHue();
            sat = color.GetSaturation();
            val = color.GetBrightness();
        }

        public static LedColor RandomColor()
        {
            return HSVToRGB(_random.Next(0, 360));
        }

        public uint ToInt()
        {
            return (uint) (R << 16) + (uint) (G << 8) + (uint) B;
        }


        public Color GetColor()
        {
            return Color.FromArgb(R, G, B);
        }

        public LedColor SetColor(byte red, byte green, byte blue)
        {
            R = red; G = green; B = blue; return this;
        }

        [System.Text.Json.Serialization.JsonIgnore]
        public double Hue
        {
            get
            {
                return Color.FromArgb(R, G, B).GetHue();
            }
            set
            {
                var color = Color.FromArgb(R, G, B);
                double sat = color.GetSaturation();
                double brightness = color.GetBrightness();
                var hsvColor = HSVToRGB(value * 360, sat, brightness);
                R = hsvColor.R;
                G = hsvColor.G;
                B = hsvColor.B;
            }
        }

        public LedColor ScaleColorsDownBy(double amount)
        {
            R = (byte)Math.Max(R * amount, 0);
            G = (byte)Math.Max(G * amount, 0);
            B = (byte)Math.Max(B * amount, 0);
            return this;
        }

        public LedColor ScaleColorsUpBy(double amount)
        {
            R = (byte)Math.Min(R * amount, 255);
            G = (byte)Math.Min(G * amount, 255);
            B = (byte)Math.Min(B * amount, 255);
            return this;
        }

        public LedColor FadeToBlackBy(double amount)
        {
            LedColor copy = new LedColor(this);
            double amountToFade = Math.Clamp(amount, 0.0f, 1.0f);
            copy.ScaleColorsDownBy(1.0f - amountToFade);
            return copy;
        }

        public LedColor Blend(LedColor other, double amount = 0.5f)
        {
            double amountToBlend = Math.Clamp(amount, 0.0f, 1.0f);
            byte r = (byte)(R * amountToBlend + other.R * (1.0 - amountToBlend));
            byte g = (byte)(G * amountToBlend + other.G * (1.0 - amountToBlend));
            byte b = (byte)(R * amountToBlend + other.B * (1.0 - amountToBlend));
            return new LedColor(r, g, b);
        }

        public static LedColor operator +(LedColor left, LedColor right)
        {
            byte r = (byte)Math.Clamp(left.R + right.R, 0, 255);
            byte g = (byte)Math.Clamp(left.G + right.G, 0, 255);
            byte b = (byte)Math.Clamp(left.B + right.B, 0, 255);
            return new LedColor(r, g, b);
        }

        public static LedColor operator -(LedColor left, LedColor right)
        {
            byte r = (byte)Math.Clamp(left.R - right.R, 0, 255);
            byte g = (byte)Math.Clamp(left.G - right.G, 0, 255);
            byte b = (byte)Math.Clamp(left.B - right.B, 0, 255);
            return new LedColor(r, g, b);
        }

        public static LedColor operator *(LedColor left, double right)
        {
            byte r = (byte)Math.Clamp(left.R * right, 0, 255);
            byte g = (byte)Math.Clamp(left.G * right, 0, 255);
            byte b = (byte)Math.Clamp(left.B * right, 0, 255);
            return new LedColor(r, g, b);
        }

    }
}
