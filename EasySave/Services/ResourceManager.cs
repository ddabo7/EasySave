using Avalonia;
using System.Text.Json;
using System.Reflection;
using Avalonia.Threading;

namespace EasySave.Services;

public class ResourceManager
{
    private static ResourceManager? _instance;
    private static readonly object _lock = new();
    private Dictionary<string, string> _currentStrings = new();
    private string _currentLanguage = "en";

    public static ResourceManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new ResourceManager();
                }
            }
            return _instance;
        }
    }

    public event EventHandler? LanguageChanged;

    private ResourceManager()
    {
        LoadLanguage("en");
    }

    public void SetLanguage(string language)
    {
        if (language == _currentLanguage) return;

        if (LoadLanguage(language))
        {
            _currentLanguage = language;
            
            // Mettre à jour les ressources sur le thread UI
            Dispatcher.UIThread.Post(() =>
            {
                if (Application.Current != null)
                {
                    // Mettre à jour les ressources dynamiques
                    foreach (var key in _currentStrings.Keys)
                    {
                        Application.Current.Resources[key] = _currentStrings[key];
                    }
                }
                
                // Notifier du changement de langue
                LanguageChanged?.Invoke(this, EventArgs.Empty);
            });
        }
    }

    public string GetString(string key)
    {
        return _currentStrings.TryGetValue(key, out var value) ? value : key;
    }

    private bool LoadLanguage(string language)
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"EasySave.Resources.Localization.{language}.json";
            
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) return false;

            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            var strings = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            
            if (strings != null)
            {
                _currentStrings = strings;
                return true;
            }
        }
        catch
        {
            // Ignorer silencieusement les erreurs
        }

        return false;
    }
}
