using System.Collections.Generic;
using System.Linq;

namespace EasySave.Models
{
    public enum BackupType
    {
        FULL,
        DIFFERENTIAL
    }

    public static class BackupTypeExtensions
    {
        public static IEnumerable<BackupType> AllValues => 
            Enum.GetValues(typeof(BackupType)).Cast<BackupType>();
    }
}