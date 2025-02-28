using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.IO;
using System.Threading;

namespace EasySave.Services;

public class CryptoSoftService
{
    private const string CRYPTOSOFT_PATH = "CryptoSoft.exe";
    private static readonly Mutex mutex = new Mutex(false, "Global\\CryptoSoftMutex");
    private readonly bool _useFallbackEncryption;
    private static volatile bool _isRunning = false;
    private static readonly object _lock = new object();

    public CryptoSoftService()
    {
        _useFallbackEncryption = !RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }

    public async Task EncryptFile(string sourcePath, string targetPath)
    {
        if (_useFallbackEncryption)
        {
            await FallbackEncryption(sourcePath, targetPath);
            return;
        }

        bool mutexAcquired = false;
        try
        {
            mutexAcquired = mutex.WaitOne(TimeSpan.FromSeconds(30));
            if (!mutexAcquired)
            {
                throw new TimeoutException("Unable to acquire CryptoSoft mutex. Another instance might be running.");
            }

            lock (_lock)
            {
                if (_isRunning)
                {
                    throw new InvalidOperationException("CryptoSoft is already running.");
                }
                _isRunning = true;
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = CRYPTOSOFT_PATH,
                    Arguments = $"\"{sourcePath}\" \"{targetPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            await Task.Run(() =>
            {
                process.Start();
                process.WaitForExit();
            });

            if (process.ExitCode != 0)
            {
                throw new Exception($"CryptoSoft failed with exit code {process.ExitCode}");
            }
        }
        catch (Exception)
        {
            // If CryptoSoft fails, fall back to built-in encryption
            await FallbackEncryption(sourcePath, targetPath);
        }
        finally
        {
            lock (_lock)
            {
                _isRunning = false;
            }
            if (mutexAcquired)
            {
                mutex.ReleaseMutex();
            }
        }
    }

    private async Task FallbackEncryption(string sourcePath, string targetPath)
    {
        // Implémentation de l'encryption de secours
        using var sourceStream = File.OpenRead(sourcePath);
        using var targetStream = File.Create(targetPath);
        
        byte[] buffer = new byte[4096];
        int bytesRead;
        
        while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            // XOR encryption simple comme fallback
            for (int i = 0; i < bytesRead; i++)
            {
                buffer[i] = (byte)(buffer[i] ^ 0xFF);
            }
            await targetStream.WriteAsync(buffer, 0, bytesRead);
        }
    }
}
