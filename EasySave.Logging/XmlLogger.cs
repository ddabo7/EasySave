using System.Xml.Linq;

namespace EasySave.Logging;

/// <summary>
/// Handles logging of file transfers in XML format.
/// </summary>
public class XmlLogManager : ILogManager
{
    private readonly string logDirectory;
    private readonly object lockObject = new();

    public XmlLogManager(string logPath)
    {
        logDirectory = logPath;
        Directory.CreateDirectory(logDirectory);
    }

    /// <summary>
    /// Logs a transfer information in the log file.
    /// </summary>
    public void LogTransfer(TransferInfo transferInfo)
    {
        if (transferInfo == null) throw new ArgumentNullException(nameof(transferInfo));
        if (transferInfo.TimeStamp == default) transferInfo.TimeStamp = DateTime.Now;

        var logFile = Path.Combine(logDirectory, $"{transferInfo.TimeStamp:yyyy-MM-dd}.xml");

        lock (lockObject)
        {
            try
            {
                XDocument doc;
                if (File.Exists(logFile))
                {
                    doc = XDocument.Load(logFile);
                }
                else
                {
                    doc = new XDocument(
                        new XDeclaration("1.0", "utf-8", "yes"),
                        new XElement("Logs")
                    );
                }

                var root = doc.Root;
                if (root != null)
                {
                    var logEntry = new XElement("TransferLog",
                        new XElement("TimeStamp", transferInfo.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss")),
                        new XElement("Name", transferInfo.Name),
                        new XElement("SourceFile", transferInfo.SourceFile),
                        new XElement("TargetFile", transferInfo.TargetFile),
                        new XElement("FileSize", transferInfo.FileSize),
                        new XElement("TransferTime", transferInfo.TransferTime),
                        new XElement("CryptTime", transferInfo.CryptTime),
                        new XElement("BackupType", transferInfo.BackupType)
                    );

                    root.Add(logEntry);
                    doc.Save(logFile);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to XML log file: {ex.Message} at path: {logFile}");
                throw;
            }
        }
    }

    /// <summary>
    /// Gets the list of dates for which logs are available.
    /// </summary>
    public List<DateTime> GetAvailableLogDates()
    {
        var dates = new List<DateTime>();
        if (Directory.Exists(logDirectory))
        {
            var files = Directory.GetFiles(logDirectory, "*.xml");
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
                        var doc = XDocument.Load(file);
                        if (doc.Root?.Elements("TransferLog").Any() == true)
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
        return dates.OrderByDescending(d => d).ToList();
    }

    /// <summary>
    /// Reads logs for a specific date.
    /// </summary>
    public List<TransferInfo> ReadLogs(DateTime date)
    {
        var logs = new List<TransferInfo>();
        var logFile = Path.Combine(logDirectory, $"{date:yyyy-MM-dd}.xml");

        if (!File.Exists(logFile)) return logs;

        try
        {
            var doc = XDocument.Load(logFile);
            var entries = doc.Root?.Elements("TransferLog");
            if (entries != null)
            {
                foreach (var entry in entries)
                {
                    var log = new TransferInfo
                    {
                        TimeStamp = DateTime.Parse(entry.Element("TimeStamp")?.Value ?? DateTime.MinValue.ToString()),
                        Name = entry.Element("Name")?.Value,
                        SourceFile = entry.Element("SourceFile")?.Value,
                        TargetFile = entry.Element("TargetFile")?.Value,
                        FileSize = long.Parse(entry.Element("FileSize")?.Value ?? "0"),
                        TransferTime = long.Parse(entry.Element("TransferTime")?.Value ?? "0"),
                        CryptTime = long.Parse(entry.Element("CryptTime")?.Value ?? "0"),
                        BackupType = entry.Element("BackupType")?.Value
                    };
                    logs.Add(log);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading XML log file: {ex.Message}");
            throw;
        }

        return logs;
    }

    /// <summary>
    /// Clears all logs.
    /// </summary>
    public void ClearLogs()
    {
        if (Directory.Exists(logDirectory))
        {
            var files = Directory.GetFiles(logDirectory, "*.xml");
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
