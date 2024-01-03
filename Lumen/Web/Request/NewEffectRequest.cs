using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Lumen.Api.Effects;
using Lumen.Api.Graphics;
using Lumen.Web.Request;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lumen.Web.Request
{
    /// <summary>
    /// Custom JSON converter for <see cref="NewEffectRequest"/> objects if settings is not provided, a default instance of the settings type will be created.
    /// </summary>
    public class NewEffectRequestJsonConverter : JsonConverter<NewEffectRequest>
    {
        public override void WriteJson(JsonWriter writer, NewEffectRequest? value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override NewEffectRequest? ReadJson(JsonReader reader, Type objectType, NewEffectRequest? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            JObject jsonObject = JObject.Load(reader);

            // Extract values from the JSON object
            string location = jsonObject["location"]?.Value<string>() ?? string.Empty;
            string id = jsonObject["id"]?.Value<string>() ?? new Guid().ToString("N").Substring(0, 8);
            string effectName = jsonObject["effect"]?.Value<string>() ?? string.Empty;



            var locationObj = Lumen.LocationRegistry.GetLocation(location);
            if (locationObj == null)
                throw new JsonSerializationException($"Location '{location}' does not exist.");

            var effect = Lumen.EffectRegistry.GetEffectType(effectName);

            if (effect == null)
                throw new JsonSerializationException($"Effect '{effectName}' does not exist.");


            var settingsType = Lumen.EffectRegistry.GetSettingsType(effect);
            if (settingsType == null)
                throw new JsonSerializationException($"Effect type {effect.Name} does not have a settings type.");


            EffectSettings? settings;

            if (jsonObject.ContainsKey("settings") != true)
            {
                try
                {
                    settings = Activator.CreateInstance(settingsType) as EffectSettings;
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        $"Unable to create default instance of effect settings type {settingsType.Name}. See inner exception for details.",
                        ex);
                }
            }
            else
            {
                settings = jsonObject["settings"]?.ToObject(settingsType, serializer) as EffectSettings;
            }

            if (settings == null)
                throw new JsonSerializationException(
                    $"Could not deserialize given settings to type {settingsType?.Name ?? "null"}.");



            return new NewEffectRequest(location, id, effectName, settings);
        }
    }



    /// <summary>
    /// A request to create a new effect on a location. If settings is not provided, a default instance of the settings type will be created.
    /// </summary>
    /// <param name="Location">Name of the location to create the effect on</param>
    /// <param name="Id">Optional custom ID to give the effect, if one is not provided it is generated randomly</param>
    /// <param name="Effect">Name of the effect to create, generally the type name</param>
    /// <param name="Settings">Optional settings to apply to the effect on creation, if none is given the defaults are used</param>
    [JsonConverter(typeof(NewEffectRequestJsonConverter))]
    public record NewEffectRequest([Required] string Location, string Id, [Required] string Effect, EffectSettings Settings)
    {
        public NewEffectRequest() : this(string.Empty, Guid.NewGuid().ToString(), string.Empty, null)
        {

        }
    }
}
