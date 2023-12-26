using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lumen.Api.Effects;

namespace Lumen.Registries
{
    public interface IEffectRegistry
    {
        public LedEffect CreateEffectInstance(string effectName);
    }

    public class EffectRegistry : IEffectRegistry
    {
        private readonly Dictionary<string, Func<LedEffect>> EffectFactories = new Dictionary<string, Func<LedEffect>>();

        public void RegisterEffect(string effectName, Func<LedEffect> factory)
        {
            EffectFactories.TryAdd(effectName, factory);
        }

        public void LoadEffects()
        {
            var effectTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                .Where(t => typeof(LedEffect).IsAssignableFrom(t) && !t.IsAbstract);
            foreach (var effectType in effectTypes)
            {
                var instance = Activator.CreateInstance(effectType) as LedEffect;
                if (instance.Name != null)
                    RegisterEffect(instance.Name, () => Activator.CreateInstance(effectType) as LedEffect);
            }
        }

        public LedEffect CreateEffectInstance(string effectName)
        {
            if (EffectFactories.TryGetValue(effectName, out var effect))
            {
                return effect.Invoke();
            }
            else
            {
                return null;
            }
        }

    }
}
