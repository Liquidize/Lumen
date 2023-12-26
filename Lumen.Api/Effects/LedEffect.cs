using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lumen.Api.Graphics;
using Newtonsoft.Json;

namespace Lumen.Api.Effects
{
    public abstract class LedEffect
    {
        [JsonIgnore]
        public virtual string Name
        {
            get { return GetType().Name; }
        }

        

        protected LedEffect()
        {
            SetEffectParameters(GetEffectDefaults());
        }

        protected LedEffect(Dictionary<string, object> effectParams)
        {
            SetEffectParameters(effectParams);
        }

        public virtual int Lifetime { get; protected set; } = 5;

        [JsonIgnore]
        public DateTime StartTime { get; protected set; } = DateTime.UtcNow;

        private bool _endRequested = false;


        /// <summary>
        /// Optional logic update function for abstraction, called directly before Render
        /// </summary>
        /// <param name="deltaTime">Total milliseconds between the last frame render and now</param>
        public virtual void Update(double deltaTime)
        {

        }

        /// <summary>
        /// Gets the end time for this effect
        /// </summary>
        /// <returns></returns>
        public DateTime GetEndTime()
        {
            return StartTime.AddSeconds(Lifetime);
        }


        /// <summary>
        /// Sets the start time of this effect
        /// </summary>
        /// <param name="time"></param>
        public void SetStartTime(DateTime time)
        {
            StartTime = time;
        }

        /// <summary>
        /// Checks whether this effects lifetime is over.
        /// </summary>
        /// <returns></returns>
        public bool IsLifetimeOver()
        {
            return (Lifetime != 0 && DateTime.UtcNow > GetEndTime()) || _endRequested;
        }

        /// <summary>
        /// Renders a single frame of the effect.
        /// </summary>
        /// <param name="canvas">The canvas to draw on</param>
        /// <param name="deltaTime">Total milliseconds between last frame draw and now, potentially useful for smoothing effects out</param>
        protected abstract void Render(ILedCanvas canvas, double deltaTime);

        /// <summary>
        /// Request that this effect end, forcing its lifetime to be over regardless of how many seconds were left.
        /// </summary>
        public void RequestEnd() => _endRequested = true;

        /// <summary>
        /// Draws a single frame
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="deltaTime"></param>
        public void DrawFrame(ILedCanvas canvas, double deltaTime)
        {
            Render(canvas, deltaTime);
        }


        /// <summary>
        /// Gets a dictionary containing the default parameters for this effect.
        /// </summary>
        /// <returns></returns>
        protected abstract Dictionary<string, object> GetEffectDefaults();

        /// <summary>
        /// Sets the effect parameters based off the given dictionary of values. If no dictionary is passed or it is empty get the defaults.
        /// If a dictionary with values is passed, we merge in the missing values from the defaults.
        /// </summary>
        /// <param name="effectParams"></param>
        public virtual void SetEffectParameters(Dictionary<string, object> effectParams)
        {
            if (effectParams == null || effectParams.Count == 0)
            {
                effectParams = GetEffectDefaults();
            }
            else
            {
                // Merge effectParams with defaults, prioritizing effectParams
                var mergedParams = new Dictionary<string, object>(GetEffectDefaults());

                foreach (var kvp in effectParams)
                {
                    if (!mergedParams.ContainsKey(kvp.Key))
                    {
                        // Add key-value pair from effectParams if it doesn't exist in defaults
                        mergedParams.Add(kvp.Key, kvp.Value);
                    }
                    else
                    {
                        // Update existing key-value pair with value from effectParams
                        mergedParams[kvp.Key] = kvp.Value;
                    }
                }

                effectParams = mergedParams;
            }


            if (effectParams.TryGetValue("lifetime", out object lifetime))
            {
                Lifetime = Convert.ToInt32(lifetime);
            }
        }
    }
}
