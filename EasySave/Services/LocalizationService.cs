using System.Text.Json;
using Avalonia;

namespace EasySave.Services;

public class LocalizationService : ILocalizationService
{
    private static readonly string ResourcePath = "Resources/Localization";
    private Dictionary<string, string> _currentStrings = new();
    private string _currentLanguage = "en";
    private static LocalizationService? _instance;
    private static readonly object _lock = new();

    public static LocalizationService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new LocalizationService();
                }
            }
            return _instance;
        }
    }

    public string CurrentLanguage => _currentLanguage;

    public event EventHandler? LanguageChanged;

    private LocalizationService()
    {
        LoadLanguage("en");
    }

    public void SetLanguage(string language)
    {
        if (LoadLanguage(language))
        {
            _currentLanguage = language;
            // Notifier tous les observateurs du changement de langue
            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public string GetString(string key)
    {
        return _currentStrings.TryGetValue(key, out var value) ? value : key;
    }

    private bool LoadLanguage(string language)
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, ResourcePath, $"{language}.json");
        Console.WriteLine($"Tentative de chargement du fichier de langue : {filePath}");
        
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Erreur : Le fichier de langue {filePath} n'existe pas");
            return false;
        }

        try
        {
            var json = File.ReadAllText(filePath);
            var strings = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (strings != null)
            {
                _currentStrings = strings;
                Console.WriteLine($"Langue {language} chargée avec succès");
                return true;
            }
            Console.WriteLine("Erreur : La désérialisation a retourné null");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors du chargement de la langue : {ex.Message}");
        }

        return false;
    }
}
