using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lumen.Api.Effects;
using Lumen.Api.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Lumen.Registries
{
    public interface IEffectRegistry
    {
        public LedEffect CreateEffectInstance(string effectName, ILedCanvas canvas, EffectSettings settings);
        public Type GetEffectType(string effectName);
        public Type GetSettingsType(string effectName);
    }

    public class EffectRegistry : IEffectRegistry
    {
        private readonly Dictionary<string, Type> _effectTypes = new Dictionary<string, Type>();

        public Type GetEffectType(string effectName)
        {
            return _effectTypes[effectName] ?? null;
        }

        public void LoadEffects()
        {
            var effectTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                .Where(type => type.IsAbstract != true 
                               && type.IsSubclassOf(typeof(LedEffect)) 
                               && type.BaseType.GetGenericArguments().Length > 0);

            foreach (var type in effectTypes)
            {
                _effectTypes.Add(type.Name, type);
            }
        }

        /// <summary>
        /// Creates a new instance of an effect from the given effect name if it exists in the registry.
        /// 
        /// </summary>
        /// <param name="effectName">Name of the effect to create</param>
        /// <param name="canvas">Location's Canvas object to use for drawing</param>
        /// <param name="settings">JObject of the settings TODO: Find way to make passing just an EffectSettings type work...</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public LedEffect CreateEffectInstance(string effectName, ILedCanvas canvas, EffectSettings settings)
        {
            try
            {
                if (!_effectTypes.TryGetValue(effectName, out var type))
                {
                    throw new KeyNotFoundException($"Effect '{effectName}' not found in registry.");
                }

                var constructors = type.GetConstructors();
                var constructor = constructors.FirstOrDefault(c =>
                {
                    var parameters = c.GetParameters();
                    return parameters.Length == 2 &&
                           parameters[0].ParameterType == typeof(ILedCanvas) &&
                           parameters[1].ParameterType.BaseType == typeof(EffectSettings);
                });

                if (constructor == null)
                {
                    throw new InvalidOperationException($"Effect '{effectName}' does not have a valid constructor.");
                }

                var settingsParameter = constructor.GetParameters()
                    .FirstOrDefault(x => x.ParameterType.BaseType == typeof(EffectSettings));
                if (settingsParameter == null)
                {
                    throw new InvalidOperationException($"Effect '{effectName}' does not have a valid settings type.");
                }

                var settingsType = settingsParameter.ParameterType;
                if (settingsType != null)
                {
                  
                    return (LedEffect)constructor.Invoke(new object[] { canvas, settings });
                }
                else
                {
                    throw new InvalidOperationException($"Effect '{effectName}' does not have a valid settings type.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"An error occurred while creating the effect '{effectName}'. See inner exception for details.",
                    ex);
            }
        }


        public Type? GetSettingsType(string effectName)
        {
            var effect = GetEffectType(effectName);
            if (effect == null)
                return null;

            var constructors = effect.GetConstructors();
            var parameters = constructors.FirstOrDefault(c =>
                c.GetParameters().FirstOrDefault(p => p.ParameterType.BaseType == typeof(EffectSettings)) != null);
            var settingsType = parameters?.GetParameters().FirstOrDefault(p => p.ParameterType.BaseType == typeof(EffectSettings))?.ParameterType;
            if (settingsType == null)
                return null;
            return settingsType;
        }

        public Type? GetSettingsType(Type effectType)
        {
            var constructors = effectType.GetConstructors();
            var parameters = constructors.FirstOrDefault(c =>
                               c.GetParameters().FirstOrDefault(p => p.ParameterType.BaseType == typeof(EffectSettings)) != null);
            var settingsType = parameters?.GetParameters().FirstOrDefault(p => p.ParameterType.BaseType == typeof(EffectSettings))?.ParameterType;
            if (settingsType == null)
                return null;
            return settingsType;
        }
    }
}
