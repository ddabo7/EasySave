namespace EasySave.Services;

public interface ILocalizationService
{
    string CurrentLanguage { get; }
    void SetLanguage(string language);
    string GetString(string key);
    event EventHandler LanguageChanged;
}
