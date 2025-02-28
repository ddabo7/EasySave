using EasySave.Models;
using EasySave.Core;
using EasySave.Logging;
using System.Collections.Generic;
using System.Text.Json;

namespace EasySave.UI;

public class ConsoleUI
{
    private string currentLanguage;
    private readonly Dictionary<string, Dictionary<string, string>> translations;

    public ConsoleUI()
    {
        currentLanguage = "fr";
        translations = new Dictionary<string, Dictionary<string, string>>
        {
            ["fr"] = new Dictionary<string, string>
            {
                ["MenuTitle"] = "Menu Principal - EasySave",
                ["MenuOptions"] = "Options disponibles :",
                ["BackupOption"] = "B. Menu de sauvegarde",
                ["LogsOption"] = "V. Voir les logs",
                ["LanguageOption"] = "L. Changer la langue",
                ["QuitOption"] = "Q. Quitter l'application",
                ["SelectOption"] = "Sélectionnez une option :",
                ["BackupMenuTitle"] = "Menu de Gestion des Sauvegardes",
                ["BackupMenuOption1"] = "1. Programmer une nouvelle sauvegarde",
                ["BackupMenuOption2"] = "2. Exécuter des sauvegardes",
                ["BackupMenuOption3"] = "3. Supprimer une sauvegarde",
                ["BackupMenuReturn"] = "R. Retour au menu principal",
                ["BackupListTitle"] = "Sauvegardes programmées ({0}/5) :",
                ["ConfigureBackupTitle"] = "Configuration d'une nouvelle sauvegarde",
                ["CustomizeBackupName"] = "Voulez-vous personnaliser le nom de la sauvegarde ?",
                ["EnterBackupName"] = "Entrez le nom de la sauvegarde",
                ["SourcePath"] = "Chemin source",
                ["TargetPath"] = "Chemin cible",
                ["MaxBackupsReached"] = "\nLimite atteinte : Vous ne pouvez pas programmer plus de 5 sauvegardes.",
                ["DeleteOrExecute"] = "Supprimez une sauvegarde existante ou exécutez les sauvegardes actuelles.",
                ["NoBackupsScheduled"] = "\nAucune sauvegarde n'est programmée.",
                ["PleaseScheduleFirst"] = "Veuillez d'abord programmer au moins une sauvegarde.",
                ["PressAnyKey"] = "\nAppuyez sur une touche pour continuer...",
                ["InvalidOption"] = "\nOption invalide. Veuillez choisir une option valide.",
                ["SelectBackupType"] = "\nType de sauvegarde :\n1. Complète\n2. Différentielle\nChoisissez (1 ou 2) :",
                ["SelectBackup"] = "\nSélectionnez les sauvegardes à exécuter :\n- Pour une plage : utilisez le format N-M (exemple: 1-3)\n- Pour une sélection multiple : utilisez le format N;M (exemple: 1;3)\nThe backup name will be automatically generated with date and time.",
                ["InvalidSelection"] = "Sélection invalide. Veuillez réessayer.",
                ["ExecutingBackup"] = "Exécution de la sauvegarde {0}...",
                ["BackupComplete"] = "✓ Sauvegarde {0} terminée avec succès",
                ["BackupError"] = "❌ Erreur lors de la sauvegarde {0}: {1}",
                ["BackupSummary"] = "\nRésumé des sauvegardes :",
                ["BackupSuccess"] = "Succès : {0}",
                ["BackupFailure"] = "Échecs : {0}",
                ["ChangeLanguage"] = "Changer la langue (fr/en) :",
                ["LogsTitle"] = "Logs de sauvegarde",
                ["SelectDate"] = "Sélectionnez une date (1-{0}) :",
                ["NoLogs"] = "Aucun log disponible.",
                ["LogEntry"] = "• {0} - {1}\n  Source : {2}\n  Cible : {3}\n  Taille : {4} octets\n  Durée : {5}ms\n",
                ["DeleteBackup"] = "Entrez le numéro de la sauvegarde à supprimer :",
                ["BackupDeleted"] = "Sauvegarde supprimée avec succès",
                ["InvalidBackupNumber"] = "Numéro de sauvegarde invalide"
            },
            ["en"] = new Dictionary<string, string>
            {
                ["MenuTitle"] = "Main Menu - EasySave",
                ["MenuOptions"] = "Available options:",
                ["BackupOption"] = "B. Backup menu",
                ["LogsOption"] = "V. View logs",
                ["LanguageOption"] = "L. Change language",
                ["QuitOption"] = "Q. Quit application",
                ["SelectOption"] = "Select an option:",
                ["BackupMenuTitle"] = "Backup Management Menu",
                ["BackupMenuOption1"] = "1. Schedule new backup",
                ["BackupMenuOption2"] = "2. Execute backups",
                ["BackupMenuOption3"] = "3. Delete a backup",
                ["BackupMenuReturn"] = "R. Return to main menu",
                ["BackupListTitle"] = "Scheduled backups ({0}/5):",
                ["ConfigureBackupTitle"] = "Configure new backup",
                ["CustomizeBackupName"] = "Do you want to customize the backup name?",
                ["EnterBackupName"] = "Enter backup name",
                ["SourcePath"] = "Source path",
                ["TargetPath"] = "Target path",
                ["MaxBackupsReached"] = "\nLimit reached: You cannot schedule more than 5 backups.",
                ["DeleteOrExecute"] = "Delete an existing backup or execute current backups.",
                ["NoBackupsScheduled"] = "\nNo backups are scheduled.",
                ["PleaseScheduleFirst"] = "Please schedule at least one backup first.",
                ["PressAnyKey"] = "\nPress any key to continue...",
                ["InvalidOption"] = "\nInvalid option. Please choose a valid option.",
                ["SelectBackupType"] = "\nBackup type:\n1. Full\n2. Differential\nChoose (1 or 2):",
                ["SelectBackup"] = "\nSelect backups to execute:\n- For a range: use N-M format (example: 1-3)\n- For multiple selection: use N;M format (example: 1;3)\nThe backup name will be automatically generated with date and time.",
                ["InvalidSelection"] = "Invalid selection. Please try again.",
                ["ExecutingBackup"] = "Executing backup {0}...",
                ["BackupComplete"] = "✓ Backup {0} completed successfully",
                ["BackupError"] = "❌ Error during backup {0}: {1}",
                ["BackupSummary"] = "\nBackup summary:",
                ["BackupSuccess"] = "Success: {0}",
                ["BackupFailure"] = "Failures: {0}",
                ["ChangeLanguage"] = "Change language (fr/en):",
                ["LogsTitle"] = "Backup logs",
                ["SelectDate"] = "Select a date (1-{0}):",
                ["NoLogs"] = "No logs available.",
                ["LogEntry"] = "• {0} - {1}\n  Source: {2}\n  Target: {3}\n  Size: {4} bytes\n  Duration: {5}ms\n",
                ["DeleteBackup"] = "Enter the backup number to delete:",
                ["BackupDeleted"] = "Backup deleted successfully",
                ["InvalidBackupNumber"] = "Invalid backup number"
            }
        };
    }

    public void DisplayMenu()
    {
        Console.Clear();
        Console.WriteLine(GetTranslation("MenuTitle"));
        Console.WriteLine(new string('-', 50));
        Console.WriteLine(GetTranslation("MenuOptions"));
        Console.WriteLine(GetTranslation("BackupOption"));
        Console.WriteLine(GetTranslation("LogsOption"));
        Console.WriteLine(GetTranslation("LanguageOption"));
        Console.WriteLine(GetTranslation("QuitOption"));
        Console.WriteLine(new string('-', 50));
        Console.Write(GetTranslation("SelectOption") + " ");
    }

    public void DisplayLogs(List<DateTime> availableDates, List<EasySave.Logging.TransferInfo> logs)
    {
        Console.Clear();
        Console.WriteLine(GetTranslation("LogsTitle"));
        Console.WriteLine(new string('-', 50));

        if (!availableDates.Any())
        {
            Console.WriteLine(GetTranslation("NoLogs"));
            return;
        }

        // Afficher les logs en format JSON
        var options = new JsonSerializerOptions { WriteIndented = true };
        var jsonLogs = JsonSerializer.Serialize(logs, options);
        Console.WriteLine(jsonLogs);
    }

    public void SetLanguage(string language)
    {
        if (translations.ContainsKey(language))
        {
            currentLanguage = language;
        }
    }

    public string GetTranslation(string key)
    {
        return translations[currentLanguage].GetValueOrDefault(key, key);
    }

    public void DisplayBackupComplete(string name)
    {
        Console.WriteLine(string.Format(GetTranslation("BackupComplete"), name));
    }

    public void DisplayBackupError(string name, string error)
    {
        Console.WriteLine(string.Format(GetTranslation("BackupError"), name, error));
    }

    public void DisplayInvalidSelection()
    {
        Console.WriteLine(GetTranslation("InvalidSelection"));
    }

    public void DisplayBackupSummary(int successCount, int failureCount)
    {
        Console.WriteLine(GetTranslation("BackupSummary"));
        Console.WriteLine(string.Format(GetTranslation("BackupSuccess"), successCount));
        Console.WriteLine(string.Format(GetTranslation("BackupFailure"), failureCount));
    }

    public EasySave.Models.BackupType GetBackupType()
    {
        while (true)
        {
            Console.Write(GetTranslation("SelectBackupType") + " ");
            var choice = Console.ReadLine()?.Trim();

            if (choice == "1")
            {
                return EasySave.Models.BackupType.FULL;
            }
            if (choice == "2")
            {
                return EasySave.Models.BackupType.DIFFERENTIAL;
            }

            DisplayInvalidSelection();
        }
    }

    public string? GetBackupSelection()
    {
        Console.WriteLine(GetTranslation("SelectBackup"));
        return Console.ReadLine();
    }
}
