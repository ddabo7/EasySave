using System.Text.Json;
using System.IO;
using EasySave.Models;

namespace EasySave.Core;

/// <summary>
/// Gère l'état des sauvegardes.
/// </summary>
public class StateManager
{
    private readonly string stateFilePath;
    private readonly JsonSerializerOptions jsonOptions;
    private readonly Dictionary<string, JobState> states;

    public IReadOnlyDictionary<string, JobState> States => states;

    public StateManager()
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var projectRoot = Directory.GetParent(baseDirectory)?.Parent?.Parent?.Parent?.FullName;
        if (projectRoot == null)
            throw new InvalidOperationException("Could not determine project root directory");

        var stateDirectory = Path.Combine(projectRoot, "Logs", "State");
        stateFilePath = Path.Combine(stateDirectory, "state.json");
        
        jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
        
        Directory.CreateDirectory(stateDirectory);
        states = LoadStates();
    }

    private Dictionary<string, JobState> LoadStates()
    {
        try
        {
            if (!File.Exists(stateFilePath))
            {
                SaveEmptyState();
                return new Dictionary<string, JobState>();
            }

            var jsonContent = File.ReadAllText(stateFilePath);
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                SaveEmptyState();
                return new Dictionary<string, JobState>();
            }

            var loadedStates = JsonSerializer.Deserialize<Dictionary<string, JobState>>(jsonContent, jsonOptions);
            return loadedStates ?? new Dictionary<string, JobState>();
        }
        catch (JsonException)
        {
            SaveEmptyState();
            return new Dictionary<string, JobState>();
        }
    }

    private void SaveEmptyState()
    {
        File.WriteAllText(stateFilePath, JsonSerializer.Serialize(
            new Dictionary<string, JobState>(), jsonOptions));
    }

    /// <summary>
    /// Sauvegarde l'état d'un travail.
    /// </summary>
    public void SaveState(JobState state)
    {
        if (state == null) return;

        states[state.JobName] = state;
        var jsonString = JsonSerializer.Serialize(states, jsonOptions);
        File.WriteAllText(stateFilePath, jsonString);
    }

    /// <summary>
    /// Récupère l'état d'un travail.
    /// </summary>
    public JobState? GetState(string jobName)
    {
        return states.TryGetValue(jobName, out var state) ? state : null;
    }

    /// <summary>
    /// Récupère tous les états.
    /// </summary>
    public IReadOnlyDictionary<string, JobState> GetAllStates()
    {
        return states;
    }
}
