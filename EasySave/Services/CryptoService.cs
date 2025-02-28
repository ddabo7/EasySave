using System.Diagnostics;

namespace EasySave.Services
{
    public class CryptoService
    {
        private readonly string _cryptoSoftPath;
        private readonly string[] _extensionsToEncrypt;

        public CryptoService(string cryptoSoftPath, string[] extensionsToEncrypt)
        {
            _cryptoSoftPath = cryptoSoftPath;
            _extensionsToEncrypt = extensionsToEncrypt;
        }

        public async Task<long> EncryptFileAsync(string sourceFile, string destinationFile)
        {
            if (!ShouldEncryptFile(sourceFile))
                return 0;

            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _cryptoSoftPath,
                        Arguments = $"\"{sourceFile}\" \"{destinationFile}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                await process.WaitForExitAsync();
                stopwatch.Stop();

                if (process.ExitCode != 0)
                    return -1;

                return stopwatch.ElapsedMilliseconds;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        private bool ShouldEncryptFile(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            return _extensionsToEncrypt.Contains(extension);
        }
    }
}
