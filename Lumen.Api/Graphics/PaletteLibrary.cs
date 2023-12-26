using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lumen.Api.Graphics
{
    /// <summary>
    /// Library of predefined color palettes
    /// </summary>
    public static  class PaletteLibrary
    {
        public static readonly ColorPalette Rainbow = new ColorPalette(new LedColor[]
        {
            ColorLibrary.Red,
            ColorLibrary.Orange,
            ColorLibrary.Yellow,
            ColorLibrary.Green,
            ColorLibrary.Blue,
            ColorLibrary.Indigo,
            ColorLibrary.Violet
        });


        public static readonly GaussianPalette SmoothRainbow = new GaussianPalette(new LedColor[]
        {
            ColorLibrary.Red,
            ColorLibrary.Orange,
            ColorLibrary.Yellow,
            ColorLibrary.Green,
            ColorLibrary.Blue,
            ColorLibrary.Indigo,
            ColorLibrary.Violet
        });

        public static readonly GaussianPalette SteppedRainbow = new GaussianPalette(new LedColor[]
        {
            ColorLibrary.Red,
            ColorLibrary.Orange,
            ColorLibrary.Yellow,
            ColorLibrary.Green,
            ColorLibrary.Blue,
            ColorLibrary.Indigo,
            ColorLibrary.Violet
        }, 0, false);

    }
}
