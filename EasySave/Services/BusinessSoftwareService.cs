using System.Diagnostics;

namespace EasySave.Services
{
    public class BusinessSoftwareService
    {
        private readonly System.Collections.Generic.List<string> _businessSoftware;
        private bool _lastState = false;

        public event Action<bool>? BusinessSoftwareStateChanged;

        public BusinessSoftwareService(System.Collections.Generic.IEnumerable<string> businessSoftware)
        {
            _businessSoftware = new System.Collections.Generic.List<string>(businessSoftware ?? System.Array.Empty<string>());
        }

        public bool IsBusinessSoftwareRunning()
        {
            try
            {
                var processes = Process.GetProcesses();
                bool isRunning = processes.Any(p => 
                {
                    try
                    {
                        return _businessSoftware.Any(sw => 
                            p.ProcessName.Contains(sw, System.StringComparison.OrdinalIgnoreCase));
                    }
                    catch
                    {
                        return false;
                    }
                });

                if (isRunning != _lastState)
                {
                    _lastState = isRunning;
                    BusinessSoftwareStateChanged?.Invoke(isRunning);
                    if (isRunning)
                    {
                        System.Console.WriteLine("Logiciel métier détecté. Les sauvegardes sont suspendues.");
                    }
                    else
                    {
                        System.Console.WriteLine("Aucun logiciel métier détecté. Les sauvegardes peuvent reprendre.");
                    }
                }

                return isRunning;
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Erreur lors de la vérification des logiciels métier : {ex.Message}");
                return false;
            }
        }

        public void UpdateBusinessSoftwareList(System.Collections.Generic.IEnumerable<string> software)
        {
            _businessSoftware.Clear();
            _businessSoftware.AddRange(software);
            
            // Toujours garder Excel dans la liste
            var excelNames = new[] { "excel", "EXCEL", "Microsoft Excel", "com.microsoft.Excel", "Excel" };
            foreach (var name in excelNames)
            {
                if (!_businessSoftware.Contains(name, System.StringComparer.OrdinalIgnoreCase))
                {
                    _businessSoftware.Add(name);
                }
            }
        }
    }
}
