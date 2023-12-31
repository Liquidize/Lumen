using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lumen.Api.Effects;
using Lumen.Api.Graphics;

namespace Lumen.Registries
{
    public interface IEffectRegistry
    {
        public LedEffect CreateEffectInstance(string effectName, ILedCanvas canvas, Dictionary<string, object> settings);
    }

    public class EffectRegistry : IEffectRegistry
    {
        private readonly Dictionary<string, Func<ILedCanvas, Dictionary<string, object>, LedEffect>> effectFactories = new Dictionary<string, Func<ILedCanvas, Dictionary<string, object>, LedEffect>>();

        public void RegisterEffect(string effectName, Func<ILedCanvas, Dictionary<string, object>, LedEffect> factory)
        {
            effectFactories.TryAdd(effectName, factory);
        }

        public void LoadEffects()
        {
            var effectTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                .Where(t => typeof(LedEffect).IsAssignableFrom(t) && !t.IsAbstract);
            foreach (var effectType in effectTypes)
            {
                    RegisterEffect(effectType.Name, (canvas, settings) => Activator.CreateInstance(effectType, canvas, settings) as LedEffect);
            }
        }

        public LedEffect CreateEffectInstance(string effectName, ILedCanvas canvas, Dictionary<string, object> settings)
        {
            if (effectFactories.TryGetValue(effectName, out var factory))
            {
                return factory.Invoke(canvas, settings);
            }
            else
            {
                return null;
            }
        }
    }
}
