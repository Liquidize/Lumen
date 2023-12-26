using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Lumen.Api.Graphics
{
    /// <summary>
    /// Library of known named colors, data obtained from https://htmlcolorcodes.com/color-names/
    /// </summary>
    public static class ColorLibrary
    {
        // Reds
        public static readonly LedColor IndianRed = new LedColor(205, 92, 92);
        public static readonly LedColor LightCoral = new LedColor(240, 128, 128);
        public static readonly LedColor Salmon = new LedColor(250, 128, 114);
        public static readonly LedColor DarkSalmon = new LedColor(233, 150, 122);
        public static readonly LedColor LightSalmon = new LedColor(255, 160, 122);
        public static readonly LedColor Crimson = new LedColor(220, 20, 60);
        public static readonly LedColor Red = new LedColor(255, 0, 0);
        public static readonly LedColor FireBrick = new LedColor(178, 34, 34);
        public static readonly LedColor DarkRed = new LedColor(139, 0, 0);

        // Pinks
        public static readonly LedColor Pink = new LedColor(255, 192, 203);
        public static readonly LedColor LightPink = new LedColor(255, 182, 193);
        public static readonly LedColor HotPink = new LedColor(255, 105, 180);
        public static readonly LedColor DeepPink = new LedColor(255, 20, 147);
        public static readonly LedColor MediumVioletRed = new LedColor(199, 21, 133);
        public static readonly LedColor PaleVioletRed = new LedColor(219, 112, 147);

        // Oranges
        public static readonly LedColor Coral = new LedColor(255, 127, 80);
        public static readonly LedColor Tomato = new LedColor(255, 99, 71);
        public static readonly LedColor OrangeRed = new LedColor(255, 69, 0);
        public static readonly LedColor DarkOrange = new LedColor(255, 140, 0);
        public static readonly LedColor Orange = new LedColor(255, 165, 0);

        // Yellows
        public static readonly LedColor Gold = new LedColor(255, 215, 0);
        public static readonly LedColor Yellow = new LedColor(255, 255, 0);

        // Purples
        public static readonly LedColor Lavender = new LedColor(230, 230, 250);
        public static readonly LedColor Thistle = new LedColor(216, 191, 216);
        public static readonly LedColor Plum = new LedColor(221, 160, 221);
        public static readonly LedColor Violet = new LedColor(238, 130, 238);
        public static readonly LedColor Orchid = new LedColor(218, 212, 214);
        public static readonly LedColor Magenta = new LedColor(255, 0, 255);
        public static readonly LedColor Indigo = new LedColor(75, 0, 130);

        // Greens
        public static readonly LedColor GreenYellow = new LedColor(173, 255, 47);
        public static readonly LedColor Chartreuse = new LedColor(127, 255, 0);
        public static readonly LedColor LawnGreen = new LedColor(124, 252, 0);
        public static readonly LedColor Lime = new LedColor(0, 255, 0);
        public static readonly LedColor LimeGreen = new LedColor(50, 205, 50);
        public static readonly LedColor PaleGreen = new LedColor(152, 251, 152);
        public static readonly LedColor LightGreen = new LedColor(144, 238, 144);
        public static readonly LedColor MediumSpringGreen = new LedColor(0, 250, 154);
        public static readonly LedColor SpringGreen = new LedColor(0, 255, 127);
        public static readonly LedColor MediumSeaGreen = new LedColor(60, 179, 113);
        public static readonly LedColor SeaGreen = new LedColor(46, 139, 87);
        public static readonly LedColor ForestGreen = new LedColor(34, 139, 34);
        public static readonly LedColor Green = new LedColor(0, 128, 0);
        public static readonly LedColor DarkGreen = new LedColor(0, 100, 0);

        // Blues
        public static readonly LedColor Cyan = new LedColor(0, 255, 255);
        public static readonly LedColor LightCyan = new LedColor(224, 255, 255);
        public static readonly LedColor PaleTurquoise = new LedColor(175, 238, 238);
        public static readonly LedColor Aquamarine = new LedColor(127, 255, 212);
        public static readonly LedColor Turquoise = new LedColor(64, 224, 208);
        public static readonly LedColor MediumTurquoise = new LedColor(72, 209, 204);
        public static readonly LedColor DarkTurquoise = new LedColor(0, 206, 209);
        public static readonly LedColor CadetBlue = new LedColor(95, 158, 160);
        public static readonly LedColor SteelBlue = new LedColor(70, 130, 180);
        public static readonly LedColor LightSteelBlue = new LedColor(176, 196, 222);
        public static readonly LedColor PowderBlue = new LedColor(176, 224, 230);
        public static readonly LedColor LightBlue = new LedColor(173, 216, 230);
        public static readonly LedColor SkyBlue = new LedColor(135, 206, 235);
        public static readonly LedColor DeepSkyBlue = new LedColor(0, 191, 255);
        public static readonly LedColor DodgerBlue = new LedColor(30, 144, 255);
        public static readonly LedColor CornflowerBlue = new LedColor(100, 149, 237);
        public static readonly LedColor MediumSlateBlue = new LedColor(123, 104, 238);
        public static readonly LedColor RoyalBlue = new LedColor(65, 105, 225);
        public static readonly LedColor Blue = new LedColor(0, 0, 255);
        public static readonly LedColor MediumBlue = new LedColor(0, 0, 205);
        public static readonly LedColor DarkBlue = new LedColor(0, 0, 139);
        public static readonly LedColor Navy = new LedColor(0, 0, 128);
        public static readonly LedColor MidnightBlue = new LedColor(25, 25, 112);

        // Whites / Grays / Black
        public static readonly LedColor White = new LedColor(255, 255, 255);
        public static readonly LedColor Gainsboro = new LedColor(220, 220, 220);
        public static readonly LedColor LightGray = new LedColor(211, 211, 211);
        public static readonly LedColor Silver = new LedColor(192, 192, 192);
        public static readonly LedColor DarkGray = new LedColor(169, 169, 169);
        public static readonly LedColor Gray = new LedColor(128, 128, 128);
        public static readonly LedColor DimGray = new LedColor(105, 105, 105);
        public static readonly LedColor LightSlateGray = new LedColor(119, 136, 153);
        public static readonly LedColor SlateGray = new LedColor(112, 128, 144);
        public static readonly LedColor DarkSlateGray = new LedColor(47, 79, 79);
        public static readonly LedColor Black = new LedColor(0, 0, 0);


        /// <summary>
        /// Gets a color by its name, if no color is found return black.
        /// </summary>
        /// <param name="colorName"></param>
        /// <returns></returns>
        public static LedColor GetColorByName(string colorName)
        {
            return GetColorByName(colorName, Black);
        }

        /// <summary>
        /// Gets a color by its name, if no color is found returns the given default
        /// </summary>
        /// <param name="colorName"></param>
        /// <returns></returns>
        public static LedColor GetColorByName(string colorName, LedColor defaultColor)
        {
            switch (colorName.ToLower())
            {
                // Reds
                case "indianred":
                    return IndianRed;
                case "lightcoral":
                    return LightCoral;
                case "salmon":
                    return Salmon;
                case "darksalmon":
                    return DarkSalmon;
                case "lightsalmon":
                    return LightSalmon;
                case "crimson":
                    return Crimson;
                case "red":
                    return Red;
                case "firebrick":
                    return FireBrick;
                case "darkred":
                    return DarkRed;

                // Pinks
                case "pink":
                    return Pink;
                case "lightpink":
                    return LightPink;
                case "hotpink":
                    return HotPink;
                case "deeppink":
                    return DeepPink;
                case "mediumvioletred":
                    return MediumVioletRed;
                case "palevioletred":
                    return PaleVioletRed;

                // Oranges
                case "coral":
                    return Coral;
                case "tomato":
                    return Tomato;
                case "orangered":
                    return OrangeRed;
                case "darkorange":
                    return DarkOrange;
                case "orange":
                    return Orange;

                // Yellows
                case "gold":
                    return Gold;
                case "yellow":
                    return Yellow;

                // Purples
                case "lavender":
                    return Lavender;
                case "thistle":
                    return Thistle;
                case "plum":
                    return Plum;
                case "violet":
                    return Violet;
                case "orchid":
                    return Orchid;
                case "magenta":
                    return Magenta;

                // Greens
                case "greenyellow":
                    return GreenYellow;
                case "chartreuse":
                    return Chartreuse;
                case "lawngreen":
                    return LawnGreen;
                case "lime":
                    return Lime;
                case "limegreen":
                    return LimeGreen;
                case "palegreen":
                    return PaleGreen;
                case "lightgreen":
                    return LightGreen;
                case "mediumspringgreen":
                    return MediumSpringGreen;
                case "springgreen":
                    return SpringGreen;
                case "mediumseagreen":
                    return MediumSeaGreen;
                case "seagreen":
                    return SeaGreen;
                case "forestgreen":
                    return ForestGreen;
                case "green":
                    return Green;
                case "darkgreen":
                    return DarkGreen;

                // Blues
                case "cyan":
                    return Cyan;
                case "lightcyan":
                    return LightCyan;
                case "paleturquoise":
                    return PaleTurquoise;
                case "aquamarine":
                    return Aquamarine;
                case "turquoise":
                    return Turquoise;
                case "mediumturquoise":
                    return MediumTurquoise;
                case "darkturquoise":
                    return DarkTurquoise;
                case "cadetblue":
                    return CadetBlue;
                case "steelblue":
                    return SteelBlue;
                case "lightsteelblue":
                    return LightSteelBlue;
                case "powderblue":
                    return PowderBlue;
                case "lightblue":
                    return LightBlue;
                case "skyblue":
                    return SkyBlue;
                case "deepskyblue":
                    return DeepSkyBlue;
                case "dodgerblue":
                    return DodgerBlue;
                case "cornflowerblue":
                    return CornflowerBlue;
                case "mediumslateblue":
                    return MediumSlateBlue;
                case "royalblue":
                    return RoyalBlue;
                case "blue":
                    return Blue;
                case "mediumblue":
                    return MediumBlue;
                case "darkblue":
                    return DarkBlue;
                case "navy":
                    return Navy;
                case "midnightblue":
                    return MidnightBlue;

                // Whites / Grays / Black
                case "white":
                    return White;
                case "gainsboro":
                    return Gainsboro;
                case "lightgray":
                    return LightGray;
                case "silver":
                    return Silver;
                case "darkgray":
                    return DarkGray;
                case "gray":
                    return Gray;
                case "dimgray":
                    return DimGray;
                case "lightslategray":
                    return LightSlateGray;
                case "slategray":
                    return SlateGray;
                case "darkslategray":
                    return DarkSlateGray;
                case "black":
                    return Black;
                default:
                    return defaultColor;
            }
        }

    }
}
