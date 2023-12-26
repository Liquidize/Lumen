using Lumen.Api.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lumen.Api.Effects
{
    public class SimpleColorFillEffect : LedEffect
    {
    
        protected LedColor Color;
        protected uint EveryNth = 1;

        /// <summary>
        ///  Default constructor used when loading
        /// </summary>
        public SimpleColorFillEffect() : base()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="color"></param>
        /// <param name="everyNth"></param>
        public SimpleColorFillEffect(LedColor color, uint everyNth = 10)
        {
            Color = color;
            EveryNth = everyNth;
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
            for (uint i = 0; i < canvas.PixelCount; i += EveryNth)
                canvas.DrawPixel(i, Color);
        }

        /// <summary>
        /// Gets a dictionary containing the default parameters
        /// </summary>
        /// <returns></returns>
        protected override Dictionary<string, object> GetEffectDefaults()
        {
            return new Dictionary<string, object>()
            {
                { "lifetime", 0 },
                { "color", ColorLibrary.DarkRed },
                { "everyNth", 1 }
            };
        }

        /// <summary>
        /// Sets the parameters of this instance of the effect from the provided dictionary.
        /// </summary>
        /// <param name="effectParams">Key-value pair of parameters, if none is passed Defaults are obtained</param>
        public override void SetEffectParameters(Dictionary<string, object> effectParams)
        {

            base.SetEffectParameters(effectParams);

            if (effectParams.TryGetValue("lifetime", out object lifetime))
            {
                Lifetime = Convert.ToInt32(lifetime);
            }

            if (effectParams.TryGetValue("color", out object color))
            {
                Color = (LedColor)color;
            }

            if (effectParams.TryGetValue("everyNth", out object everyNth))
            {
                EveryNth = Convert.ToUInt32(everyNth);
            }

        }
    }
}
