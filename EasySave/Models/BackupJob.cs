using System.Text.Json.Serialization;
using EasySave.Utils;

namespace EasySave.Models
{
    public class BackupJob
    {
        public string Name { get; set; } = string.Empty;
        public string SourcePath { get; set; } = string.Empty;
        public string DestinationPath { get; set; } = string.Empty;
        public BackupType Type { get; set; } = BackupType.FULL;
        public List<FileDetail> Files { get; set; } = new List<FileDetail>();
        
        [JsonConverter(typeof(BackupStatusJsonConverter))]
        public BackupStatus Status { get; set; }
        
        // Progression de la sauvegarde en pourcentage (0-100)
        public int Progress { get; set; }
        
        // Ces propriétés sont utilisées pour le logging
        public string SourceFile => Path.GetFileName(SourcePath);
        public string TargetFile => Path.GetFileName(DestinationPath);
        
        public bool IsPriority { get; set; } = false;
        public int FileSize { get; set; } = 0;
        public int PriorityFileSize { get; set; } = 0;
        public int TotalFileSize { get; set; } = 0;
        
        // Méthode pour calculer la progression
        public void UpdateProgress(int processedSize)
        {
            if (TotalFileSize > 0)
            {
                Progress = (int)((processedSize * 100.0) / TotalFileSize);
            }
            else
            {
                Progress = 0;
            }
        }
    }
}
