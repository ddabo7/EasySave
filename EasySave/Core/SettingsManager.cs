using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasySave.Core
{
    public class Settings
    {
        public string Language { get; set; } = "fr";
        public string LogFormat { get; set; } = "";
        
        [JsonConverter(typeof(StringArrayConverter))]
        public string[] BusinessSoftware { get; set; } = Array.Empty<string>();
        
        [JsonConverter(typeof(StringArrayConverter))]
        public string[] PriorityExtensions { get; set; } = Array.Empty<string>();
        
        [JsonConverter(typeof(StringArrayConverter))]
        public string[] EncryptExtensions { get; set; } = Array.Empty<string>();
        
        public int MaxParallelJobs { get; set; } = 5;
        public long LargeFileThreshold { get; set; } = 1024 * 1024; // 1 Mo
        public int NetworkLoadThreshold { get; set; } = 80;
        public bool MonitorNetworkLoad { get; set; } = true;
    }

    public class StringArrayConverter : JsonConverter<string[]>
    {
        public override string[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                var list = new List<string>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                        break;
                    if (reader.TokenType == JsonTokenType.String)
                        list.Add(reader.GetString());
                }
                return list.ToArray();
            }
            return Array.Empty<string>();
        }

        public override void Write(Utf8JsonWriter writer, string[] value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var item in value)
            {
                writer.WriteStringValue(item);
            }
            writer.WriteEndArray();
        }
    }

    public static class SettingsManager
    {
        private static readonly string SettingsPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "settings.json"
        );

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public static Settings LoadSettings()
        {
            if (!File.Exists(SettingsPath))
            {
                var defaultSettings = new Settings();
                SaveSettings(defaultSettings);
                return defaultSettings;
            }

            var json = File.ReadAllText(SettingsPath);
            try
            {
                return JsonSerializer.Deserialize<Settings>(json, _jsonOptions) ?? new Settings();
            }
            catch
            {
                return new Settings();
            }
        }

        public static void SaveSettings(Settings settings)
        {
            var json = JsonSerializer.Serialize(settings, _jsonOptions);
            File.WriteAllText(SettingsPath, json);
        }
    }
}
