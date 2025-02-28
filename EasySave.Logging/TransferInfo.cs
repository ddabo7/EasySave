using System.Text.Json.Serialization;

namespace EasySave.Logging;

/// <summary>
/// Représente les informations de transfert d'un fichier.
/// </summary>
public class TransferInfo
{
    [JsonPropertyName("timestamp")]
    public DateTime TimeStamp { get; set; } = DateTime.Now;

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("sourceFile")]
    public string SourceFile { get; set; } = "";

    [JsonPropertyName("targetFile")]
    public string TargetFile { get; set; } = "";

    [JsonPropertyName("fileSize")]
    public long FileSize { get; set; }

    [JsonPropertyName("transferTime")]
    public long TransferTime { get; set; }  // en millisecondes

    [JsonPropertyName("cryptTime")]
    public long CryptTime { get; set; }  // en millisecondes

    [JsonPropertyName("backupType")]
    public string BackupType { get; set; } = "";

    [JsonPropertyName("status")]
    public string Status { get; set; } = "Success";

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("sourceFileHash")]
    public string? SourceFileHash { get; set; }

    [JsonPropertyName("targetFileHash")]
    public string? TargetFileHash { get; set; }
}
