using EasySave.Models;

namespace EasySave.Services;

public interface ILogService
{
    void LogEntry(LogEntry entry);
    List<LogEntry> LoadLogs();
    void ClearLogs();
}
