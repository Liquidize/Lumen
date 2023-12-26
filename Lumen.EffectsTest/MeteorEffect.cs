using Lumen.Api.Effects;
using Lumen.Api.Graphics;
using Microsoft.VisualBasic.CompilerServices;

namespace Lumen.EffectsTest
{
    public class MeteorEffect : LedEffect
    {
        public override string Name
        {
            get { return "MeteorEffect"; }
        }


        protected int MeteorCount;
        protected int MeteorSize;
        protected double MeteorTrailDecay;
        protected double MeteorSpeedMin;
        protected double MeteorSpeedMax;
        protected Meteor[] Meteors;
        protected double HueVal = 0;
        protected bool Bounce;

        protected bool FirstDraw = true;


        static Random _random  = new Random();

        static double RandomDouble(double min, double max)
        {
            return _random.NextDouble() * (max - min) + min;
        }

        static byte RandomByte()
        {
            return (byte)_random.Next(0, 256);
        }


        protected override void Render(ILedCanvas canvas, double deltaTime)
        {
            if (FirstDraw)
            {
                canvas.FillSolid(ColorLibrary.Black);
                FirstDraw = false;
            }

            for (uint j = 0; j < canvas.PixelCount; j++)
            {
                if ((MeteorTrailDecay == 0) || (RandomByte() > 64))
                {
                    LedColor c = canvas.GetPixel(j, 0);
                   // c.FadeToBlackBy(MeteorTrailDecay);
                  var c2=  c *= MeteorTrailDecay;
                    canvas.DrawPixel(j, 0, c2);
                }
            }

            for (int i = 0; i < MeteorCount; i++)
                if (null != Meteors[i])
                    Meteors[i].Draw(canvas);

        }

        protected override Dictionary<string, object> GetEffectDefaults()
        {
          var dic = new Dictionary<string, object>();
          dic.Add("lifetime", 0);
          dic.Add("meteorCount", 4);
          dic.Add("meteorSize", 4);
          dic.Add("trailDecay", 0.8);
          dic.Add("minSpeed", 2);
          dic.Add("maxSpeed", 2);
          dic.Add("bounce", true); 
          return dic;
        }

        public override void SetEffectParameters(Dictionary<string, object> effectParams)
        {
            if (effectParams == null) effectParams = GetEffectDefaults();


            if (effectParams.TryGetValue("lifetime", out object lifetime))
            {
                Lifetime = Convert.ToInt32(lifetime);
            }

            if (effectParams.TryGetValue("meteorCount", out object meteorCount))
            {
                MeteorCount = Convert.ToInt32(meteorCount);
                MeteorCount += 1;
                MeteorCount /= 2;
                MeteorCount *= 2;
            }

            if (effectParams.TryGetValue("meteorSize", out object meteorSize))
            {
                MeteorSize = Convert.ToInt32(meteorSize);
            }

            if (effectParams.TryGetValue("trailDecay", out object trailDecay))
            {
                MeteorTrailDecay = Convert.ToDouble(trailDecay);
            }

            if (effectParams.TryGetValue("minSpeed", out object speedMin))
            {
                MeteorSpeedMin = Convert.ToDouble(speedMin);
            }

            if (effectParams.TryGetValue("maxSpeed", out object speedMax))
            {
                MeteorSpeedMax = Convert.ToDouble(speedMax);
            }

            if (effectParams.TryGetValue("bounce", out object bounce))
            {
                Bounce = Convert.ToBoolean(bounce);
            }

            Meteors = new Meteor[MeteorCount];

            int halfCount = MeteorCount / 2;

            for (int i = 0; i < halfCount; i++)
            {
                HueVal += 20;
                HueVal %= 360;
                Meteors[i] = new Meteor(HueVal, i * 316/ halfCount, true, Bounce,
                    RandomDouble(MeteorSpeedMin, MeteorSpeedMax), MeteorSize);

                int j = halfCount + i;
                Meteors[j] = new Meteor(HueVal, i * 316 / halfCount, false, Bounce,
                    RandomDouble(MeteorSpeedMin, MeteorSpeedMax), MeteorSize);

            }

        }
    }
}
