using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Lumen.Api.Graphics
{
    public class GaussianPalette : ColorPalette
    {
        [JsonProperty("smoothing")]
        protected double Smoothing = 0.0;

        [JsonIgnore]
        public double[] Factors = new[] { 0.06136, 0.24477, 0.38774, 0.24477, 0.06136 };

        public GaussianPalette(LedColor[] colors, double smoothing = 0.0, bool blend = true) : base(colors, blend)
        {
            Smoothing = smoothing;
        }

        public override LedColor this[double d]
        {
            get
            {
                double s = Smoothing / OriginalSize;

                double red = base[d - s * 2].R * Factors[0] +
                             base[d - s].R * Factors[1] +
                             base[d].R * Factors[2] +
                             base[d + s].R * Factors[3] +
                             base[d + s * 2].R * Factors[4];

                double green = base[d - s * 2].G * Factors[0] +
                               base[d - s].G * Factors[1] +
                               base[d].G * Factors[2] +
                               base[d + s].G * Factors[3] +
                               base[d + s * 2].G * Factors[4];

                double blue = base[d - s * 2].B * Factors[0] +
                              base[d - s].B * Factors[1] +
                              base[d].B * Factors[2] +
                              base[d + s].B * Factors[3] +
                              base[d + s * 2].B * Factors[4];

                return new LedColor((byte)red, (byte)green, (byte)blue);
            }
        }

    }
}
