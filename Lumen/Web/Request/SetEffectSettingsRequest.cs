using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Lumen.Api.Effects;
using Lumen.Api.Graphics;
using Lumen.Interop;
using Lumen.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lumen.Web.Request
{

    /// <summary>
    /// Custom JSON converter for <see cref="SetEffectSettingsRequest"/> that allows for the settings to be deserialized into the correct type
    /// </summary>
    public class SetEffectSettingsRequestConverter : JsonConverter<SetEffectSettingsRequest>
    {
        public override void WriteJson(JsonWriter writer, SetEffectSettingsRequest? value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override SetEffectSettingsRequest? ReadJson(JsonReader reader, Type objectType, SetEffectSettingsRequest? existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            JObject jsonObject = JObject.Load(reader);

            // Extract values from the JSON object
            string location = jsonObject["location"]?.Value<string>() ?? string.Empty;
            string id = jsonObject["id"]?.Value<string>() ?? string.Empty;


            var locationObj = Lumen.LocationRegistry.GetLocation(location);
            if (locationObj == null)
                throw new JsonSerializationException($"Location '{location}' does not exist.");

            var effect = locationObj.GetEffect(id);

            if (effect == null)
                throw new JsonSerializationException($"Effect '{id}' does not exist.");


            var settingsType = Lumen.EffectRegistry.GetSettingsType(effect.GetType());
            if (settingsType == null)
                throw new JsonSerializationException($"Effect '{id}' type {effect.GetType().Name} does not have a settings type.");


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
                            $"Unable to create default instance of effect settings type {settingsType.Name}. See inner exception for details.", ex);
                    }
                }
                else
                {
                    settings = jsonObject["settings"]?.ToObject(settingsType, serializer) as EffectSettings;
                }

                if (settings == null)
                    throw new JsonSerializationException(
                        $"Could not deserialize given settings to type {settingsType?.Name ?? "null"}.");

            

            return new SetEffectSettingsRequest(location, id, settings);
        }
    }


    /// <summary>
    /// Request data for setting effect settings via the API
    /// </summary>
    /// <param name="Location">Name of the location to make request too</param>
    /// <param name="Id">Unique ID of the effect to set the settings for</param>
    /// <param name="MergeDefaults">Whether to merge the default settings in for any missed keys, defaults to false. If false then merge in the current settings for any missed keys. If true merge in effect defaults.</param>
    /// <param name="Settings">JSON key-value object as a dictionary containing the new settings</param>
    [JsonConverter(typeof(SetEffectSettingsRequestConverter))]
    public record SetEffectSettingsRequest([Required(AllowEmptyStrings = false)] string Location, [Required(AllowEmptyStrings = false)] string Id, EffectSettings Settings)
    {
        public SetEffectSettingsRequest() : this(string.Empty, string.Empty, null)
        {

        }
    }

}
