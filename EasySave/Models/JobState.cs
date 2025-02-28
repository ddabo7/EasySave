using System.Text.Json.Serialization;
using EasySave.Utils;

namespace EasySave.Models;

/// <summary>
/// Représente l'état d'un travail de sauvegarde.
/// </summary>
public class JobState
{
    [JsonPropertyName("name")]
    public string JobName { get; set; } = string.Empty;

    [JsonPropertyName("totalFiles")]
    public long TotalFiles { get; set; }

    [JsonPropertyName("totalSize")]
    public long TotalSize { get; set; }

    [JsonPropertyName("filesProcessed")]
    public long FilesProcessed { get; set; }

    [JsonPropertyName("sizeProcessed")]
    public long SizeProcessed { get; set; }

    [JsonPropertyName("status")]
    [JsonConverter(typeof(BackupStatusJsonConverter))]
    public BackupStatus Status { get; set; }

    [JsonPropertyName("progress")]
    public int Progress { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("lastUpdate")]
    public DateTime LastUpdate { get; set; }
}
