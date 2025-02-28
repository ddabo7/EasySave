using System.Security.Cryptography;

namespace EasySave.Utils;

public static class FileHashCalculator
{
    public static string? CalculateHash(string filePath)
    {
        try
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filePath);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
        catch
        {
            return null;
        }
    }
}
