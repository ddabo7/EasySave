namespace EasySave.Models
{
    public class FileDetail
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
    }
}