namespace EasySave.Models;

public class BackupLogEntry
{
    public string Name { get; set; }
    public string SourcePath { get; set; }
    public string TargetPath { get; set; }
    public long FileSize { get; set; }
    public DateTime FileTransferTime { get; set; }
    public long EncryptionTime { get; set; }
    public string Status { get; set; }

    public BackupLogEntry()
    {
        Name = string.Empty;
        SourcePath = string.Empty;
        TargetPath = string.Empty;
        Status = string.Empty;
    }
}
