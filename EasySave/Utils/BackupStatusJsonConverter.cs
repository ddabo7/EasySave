using System.Text.Json;
using System.Text.Json.Serialization;
using EasySave.Models;

namespace EasySave.Utils;

public class BackupStatusJsonConverter : JsonConverter<BackupStatus>
{
    public override BackupStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                string value = reader.GetString()!;
                if (Enum.TryParse<BackupStatus>(value, true, out var result))
                {
                    return result;
                }
                
                // Gérer les anciens statuts
                switch (value.ToLower())
                {
                    case "ready":
                        return BackupStatus.Pending;
                    case "running":
                        return BackupStatus.Running;
                    case "completed":
                        return BackupStatus.Completed;
                    case "stopped":
                        return BackupStatus.Stopped;
                    case "failed":
                        return BackupStatus.Failed;
                    case "cancelled":
                        return BackupStatus.Cancelled;
                    default:
                        return BackupStatus.Pending;
                }

            case JsonTokenType.Number:
                int numericValue = reader.GetInt32();
                // Convertir les anciennes valeurs numériques en statuts
                switch (numericValue)
                {
                    case 0:
                        return BackupStatus.Pending;
                    case 1:
                        return BackupStatus.Running;
                    case 2:
                        return BackupStatus.Completed;
                    case 3:
                        return BackupStatus.Stopped;
                    case 4:
                        return BackupStatus.Failed;
                    default:
                        return BackupStatus.Pending;
                }

            default:
                throw new JsonException($"Unexpected token {reader.TokenType} when parsing BackupStatus");
        }
    }

    public override void Write(Utf8JsonWriter writer, BackupStatus value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
