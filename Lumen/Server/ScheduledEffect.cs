using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lumen.Api.Effects;
using Lumen.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lumen.Server
{

    /// <summary>
    /// Converter for ScheduledEffects from JSON to handle custom effect settings.
    /// </summary>
    public class ScheduledEffectConverter : JsonConverter<ScheduledEffect>
    {
        public override void WriteJson(JsonWriter writer, ScheduledEffect? value, JsonSerializer serializer)
        {
            var jsonObject = new JObject(
                new JProperty("effectName", value.EffectName),
                new JProperty("daysOfWeek", new JArray(value.DaysOfWeek)),
                new JProperty("startHour", value.StartHour),
                new JProperty("startMinute", value.StartMinute),
                new JProperty("endHour", value.EndHour),
                new JProperty("endMinute", value.EndMinute),
                new JProperty("settings", JObject.FromObject(value.Settings, serializer)),
                new JProperty("id", value.Id)
            );
            jsonObject.WriteTo(writer);
        }

        public override ScheduledEffect? ReadJson(JsonReader reader, Type objectType, ScheduledEffect? existingValue, bool hasExistingValue,
     JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            JObject jsonObject;
            try
            {
                jsonObject = JObject.Load(reader);
            }
            catch (JsonException ex)
            {
                throw new JsonSerializationException("Error reading ScheduledEffect from JSON.", ex);
            }

            string effectName = jsonObject["effectName"]?.Value<string>();
            if (effectName == null)
                throw new JsonSerializationException("Missing 'effectName' property in JSON.");

            string[] daysOfWeek;
            try
            {
                daysOfWeek = jsonObject["daysOfWeek"]?.ToObject<string[]>() ?? throw new JsonSerializationException("Missing 'daysOfWeek' property in JSON.");
            }
            catch (JsonException ex)
            {
                throw new JsonSerializationException("Error reading 'daysOfWeek' property from JSON.", ex);
            }

            uint startHour = jsonObject["startHour"]?.Value<uint>() ?? 0;
            uint startMinute = jsonObject["startMinute"]?.Value<uint>() ?? 0;
            uint endHour = jsonObject["endHour"]?.Value<uint>() ?? 24;
            uint endMinute = jsonObject["endMinute"]?.Value<uint>() ?? 60;
            string id = jsonObject["id"]?.Value<string>() ?? Guid.NewGuid().ToString("N").Substring(0, 8);

            var settingsType = Lumen.EffectRegistry.GetSettingsType(effectName);
            if (settingsType == null)
                throw new JsonSerializationException($"Unable to find settings type for effect '{effectName}'.");

            EffectSettings settings;
            try
            {
                settings = (EffectSettings)jsonObject["settings"]?.ToObject(settingsType) ?? (EffectSettings)Activator.CreateInstance(settingsType) ?? throw new Exception($"Unable to generate default settings for type {settingsType.Name}");
            }
            catch (JsonException ex)
            {
                throw new JsonSerializationException("Error reading 'settings' property from JSON.", ex);
            }

            return new ScheduledEffect(daysOfWeek, effectName, settings, id, startHour, endHour, startMinute, endMinute);
        }

    }

    /// <summary>
    /// An effect that is pre-defined in the Location's JSON, and scheduled to run at set times.
    /// </summary>
    [JsonConverter(typeof(ScheduledEffectConverter))]
    public class ScheduledEffect
    {
        public const DayOfWeek WeekEnds = DayOfWeek.Saturday | DayOfWeek.Sunday;
        public const DayOfWeek WeekDays = DayOfWeek.Monday | DayOfWeek.Tuesday | DayOfWeek.Wednesday | DayOfWeek.Thursday | DayOfWeek.Friday;
        public const DayOfWeek AllDays = WeekDays | WeekEnds;

        [JsonProperty("effectName")]
        public string EffectName { get; protected set; }
        [JsonProperty("daysOfWeek")]
        public string[] DaysOfWeek { get; protected set; }
        [JsonProperty("startHour")]
        public uint StartHour { get; protected set; }
        [JsonProperty("startMinute")]
        public uint StartMinute { get; protected set; }
        [JsonProperty("endHour")]
        public uint EndHour { get; protected set; }
        [JsonProperty("endMinute")]
        public uint EndMinute { get; protected set; }

        /// <summary>
        /// Settings of the scheduled effect
        /// </summary>
        [JsonProperty("settings")]
        public EffectSettings Settings { get; protected set; }

        [JsonProperty("id")]
        public string Id { get; protected set; } = Guid.NewGuid().ToString("N").Substring(0, 8);

        public ScheduledEffect(string[] daysOfWeek, string effectName, EffectSettings settings, string id, uint startHour, uint endHour,
            uint startMinute = 0, uint endMinute = 60)
        {
            EffectName = effectName;
            DaysOfWeek = daysOfWeek;
            StartHour = startHour;
            StartMinute = startMinute;
            EndHour = endHour;
            EndMinute = endMinute;

            if (string.IsNullOrEmpty(id))
                Id = Guid.NewGuid().ToString("N").Substring(0, 8);
            else
                Id = id;

            Settings = settings;

        }

        [JsonIgnore]
        public bool IsEffectScheduledToRunNow
        {
            get
            {
                if (DaysOfWeek.Any(d => (d.Equals(DateTime.Now.DayOfWeek.ToString(), StringComparison.OrdinalIgnoreCase) || d.Equals("all", StringComparison.OrdinalIgnoreCase))))
                    if (DateTime.Now.Hour > StartHour || DateTime.Now.Hour == StartHour && DateTime.Now.Minute >= StartMinute)
                        if (DateTime.Now.Hour < EndHour || DateTime.Now.Hour == EndHour && DateTime.Now.Minute <= EndMinute)
                            return true;

                return false;
            }
        }
    }
}
