using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace Lumen.Utils
{
    public static class Utility
    {
        public static T DeserializeFromJson<T>(string json)
        {
            if (string.IsNullOrEmpty(json)) return default;
            

            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                ContractResolver = new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() }

            };

            try
            {
                return JsonConvert.DeserializeObject<T>(json, settings);
            }
            catch (JsonException ex)
            {
                Log.Error(ex, $"Unable to parse JSON from string {json}.");
            }

            return default;

        }

        public static T DeserializeFromJson<T>(string json, Type type)
        {
            if (string.IsNullOrEmpty(json)) return default;

            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                ContractResolver = new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() }

            };

            try
            {
                return (T)JsonConvert.DeserializeObject(json, type, settings);
            }
            catch (JsonException ex)
            {
                Log.Error(ex, $"Unable to parse JSON from string {json}.");
            }

            return default;
        }


        public static string SerializeToJson(object obj)
        {
            if (obj == null) return string.Empty;

            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                ContractResolver = new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() }

            };

            try
            {
                return JsonConvert.SerializeObject(obj, settings);
            }
            catch (JsonException ex)
            {
                Log.Error(ex, $"Unable to serialize object {obj} to JSON.");
            }

            return string.Empty;
        }
    }
}
