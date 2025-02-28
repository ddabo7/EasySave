using System.Text.Json;

namespace EasySave.Logging;

public class JsonLogManager : ILogManager
{
    private readonly string logDirectory;
    private readonly object lockObject = new();

    public JsonLogManager(string logPath)
    {
        logDirectory = logPath;
        Directory.CreateDirectory(logDirectory);
    }

    public void LogTransfer(TransferInfo transferInfo)
    {
        if (transferInfo == null) throw new ArgumentNullException(nameof(transferInfo));
        if (transferInfo.TimeStamp == default) transferInfo.TimeStamp = DateTime.Now;

        var logFile = Path.Combine(logDirectory, $"{transferInfo.TimeStamp:yyyy-MM-dd}.json");

        lock (lockObject)
        {
            try
            {
                List<TransferInfo> logs;
                if (File.Exists(logFile))
                {
                    var json = File.ReadAllText(logFile);
                    logs = JsonSerializer.Deserialize<List<TransferInfo>>(json) ?? new List<TransferInfo>();
                }
                else
                {
                    logs = new List<TransferInfo>();
                }

                logs.Add(transferInfo);
                var options = new JsonSerializerOptions { WriteIndented = true };
                var updatedJson = JsonSerializer.Serialize(logs, options);
                File.WriteAllText(logFile, updatedJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to JSON log file: {ex.Message}");
                throw;
            }
        }
    }

    public List<DateTime> GetAvailableLogDates()
    {
        var dates = new List<DateTime>();
        if (Directory.Exists(logDirectory))
        {
            var files = Directory.GetFiles(logDirectory, "*.json");
            foreach (var file in files)
            {
                if (DateTime.TryParseExact(Path.GetFileNameWithoutExtension(file),
                    "yyyy-MM-dd",
                    null,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime date))
                {
                    try
                    {
                        var json = File.ReadAllText(file);
                        var logs = JsonSerializer.Deserialize<List<TransferInfo>>(json);
                        if (logs?.Any() == true)
                        {
                            dates.Add(date);
                        }
                    }
                    catch
                    {
                        // Ignorer les fichiers corrompus
                        continue;
                    }
                }
            }
        }
        return dates;
    }

    public List<TransferInfo> ReadLogs(DateTime date)
    {
        var logFile = Path.Combine(logDirectory, $"{date:yyyy-MM-dd}.json");
        if (!File.Exists(logFile))
            return new List<TransferInfo>();

        try
        {
            var json = File.ReadAllText(logFile);
            return JsonSerializer.Deserialize<List<TransferInfo>>(json) ?? new List<TransferInfo>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading JSON log file: {ex.Message}");
            return new List<TransferInfo>();
        }
    }

    public void ClearLogs()
    {
        if (Directory.Exists(logDirectory))
        {
            var files = Directory.GetFiles(logDirectory, "*.json");
            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting log file {file}: {ex.Message}");
                }
            }
        }
    }
}
