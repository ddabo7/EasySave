namespace EasySave.Models;

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string JobName { get; set; } = string.Empty;
    public string FileSource { get; set; } = string.Empty;
    public string FileTarget { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public TimeSpan TransferTime { get; set; }
    public BackupStatus Status { get; set; }
}
