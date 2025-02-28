namespace EasySave.Models;

public enum BackupStatus
{
    Pending,
    Running,
    Paused,
    Completed,
    Stopped,
    Failed,
    Cancelled
}

public static class BackupStatusExtensions
{
    public static BackupStatus[] AllValues => new[]
    {
        BackupStatus.Pending,
        BackupStatus.Running,
        BackupStatus.Paused,
        BackupStatus.Completed,
        BackupStatus.Stopped,
        BackupStatus.Failed,
        BackupStatus.Cancelled
    };
}
