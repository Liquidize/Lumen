using Lumen.Api.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lumen.Api.Effects
{
    public class SimpleDotRunEffect : LedEffect
    {
        protected LedColor Color;
        protected uint DotSize = 1;

        protected uint Location = 0;
        protected double SpeedFactor = 1.0;


        /// <summary>
        ///  Default constructor used when loading
        /// </summary>
        public SimpleDotRunEffect() : base()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="color"></param>
        /// <param name="everyNth"></param>
        public SimpleDotRunEffect(LedColor color, uint everyNth = 10)
        {
            Color = color;
            DotSize = everyNth;
        }


        /// <summary>
        ///  Called once per frame optional
        /// </summary>
        /// <param name="deltaTime"></param>
        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            


        }

        /// <summary>
        /// Render a single frame
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="deltaTime"></param>
        protected override void Render(ILedCanvas canvas, double deltaTime)
        {
            canvas.FillSolid(ColorLibrary.Black);

            Location += 1;
           

            if (Location >= canvas.PixelCount)
            {
                Location = 0;
            }


            canvas.DrawPixel(Location, Color);



        }

        /// <summary>
        /// Gets a dictionary containing the default parameters
        /// </summary>
        /// <returns></returns>
        protected override Dictionary<string, object> GetEffectDefaults()
        {
            return new Dictionary<string, object>()
            {
                { "lifetime", 5 },
                { "color", ColorLibrary.DarkRed },
                { "dotSize", 1 }
            };
        }

        /// <summary>
        /// Sets the parameters of this instance of the effect from the provided dictionary.
        /// </summary>
        /// <param name="effectParams">Key-value pair of parameters, if none is passed Defaults are obtained</param>
        public override void SetEffectParameters(Dictionary<string, object> effectParams)
        {
            if (effectParams == null)
            {
                effectParams = GetEffectDefaults();
            }

            if (effectParams.TryGetValue("lifetime", out object lifetime))
            {
                Lifetime = Convert.ToInt32(lifetime);
            }

            if (effectParams.TryGetValue("color", out object color))
            {
                Color = (LedColor)color;
            }

            if (effectParams.TryGetValue("dotSize", out object dotSize))
            {
                DotSize = Convert.ToUInt32(dotSize);
            }

        }
    }
}
