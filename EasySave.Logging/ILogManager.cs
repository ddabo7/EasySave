namespace EasySave.Logging;

/// <summary>
/// Interface for log managers.
/// </summary>
public interface ILogManager
{
    void LogTransfer(TransferInfo transferInfo);
    List<DateTime> GetAvailableLogDates();
    List<TransferInfo> ReadLogs(DateTime date);
    void ClearLogs();
}
