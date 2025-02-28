using System.Text.Json;
using System.Xml.Serialization;
using EasySave.Models;
using System.IO;

namespace EasySave.Services
{
    public class LogService : ILogService
    {
        private readonly string _logDirectory;
        private readonly string _logFormat;
        private readonly string _currentLogFile;

        public LogService()
        {
            _logDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");
            _logFormat = "JSON"; // Par défaut, on utilise JSON
            _currentLogFile = Path.Combine(_logDirectory, $"log_{DateTime.Now:yyyy-MM-dd}.json");

            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        public void LogEntry(LogEntry entry)
        {
            var logs = LoadLogs();
            logs.Add(entry);
            SaveLogs(logs);
        }

        public List<LogEntry> LoadLogs()
        {
            if (!File.Exists(_currentLogFile))
            {
                return new List<LogEntry>();
            }

            try
            {
                var content = File.ReadAllText(_currentLogFile);
                if (_logFormat == "JSON")
                {
                    return JsonSerializer.Deserialize<List<LogEntry>>(content) ?? new List<LogEntry>();
                }
                else // XML
                {
                    using var stringReader = new StringReader(content);
                    var serializer = new XmlSerializer(typeof(List<LogEntry>));
                    return (List<LogEntry>?)serializer.Deserialize(stringReader) ?? new List<LogEntry>();
                }
            }
            catch
            {
                return new List<LogEntry>();
            }
        }

        public void ClearLogs()
        {
            if (File.Exists(_currentLogFile))
            {
                File.Delete(_currentLogFile);
            }
        }

        private void SaveLogs(List<LogEntry> logs)
        {
            try
            {
                if (_logFormat == "JSON")
                {
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    var json = JsonSerializer.Serialize(logs, options);
                    File.WriteAllText(_currentLogFile, json);
                }
                else // XML
                {
                    using var writer = new StreamWriter(_currentLogFile);
                    var serializer = new XmlSerializer(typeof(List<LogEntry>));
                    serializer.Serialize(writer, logs);
                }
            }
            catch (Exception ex)
            {
                // Log l'erreur ou la gère d'une autre manière
                Console.WriteLine($"Erreur lors de la sauvegarde des logs : {ex.Message}");
            }
        }
    }
}
