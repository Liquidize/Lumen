using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Lumen.Api.Graphics
{
    public class ColorPalette
    {

        [JsonProperty("colors")]
        protected LedColor[] Colors { get; set; }

        [JsonProperty("blend")]
        public bool Blend { get; set; } = true;

        [JsonIgnore]
        public int OriginalSize
        {
            get
            {
                return Colors.Length;
            }
        }

        public ColorPalette(LedColor[] colors, bool blend = true)
        {
            Colors = colors;
            Blend = blend;
        }



        [JsonIgnore]
        public virtual LedColor this[double d]
        {
            get
            {
                while (d < 0)
                {
                    d += 1.0;
                }

                d -= ((long)d);

                double fracPerColor = 1.0 / Colors.Length;
                double indexD = d / fracPerColor;
                int index = (int)(d / fracPerColor) % Colors.Length;
                double fraction = indexD - index;

                LedColor color1 = Colors[index];
                if (!Blend) return color1;

                // Get next color and blend them

                LedColor color2 = Colors[(index + 1) % Colors.Length];
                return color1.Blend(color2, 1 - fraction);
            }
        }

    }
}
