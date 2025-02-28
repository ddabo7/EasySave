/// <summary>
/// Parse les commandes utilisateur pour l'exécution des sauvegardes.
/// </summary>
public static class CommandLineParser
{
    /// <summary>
    /// Parse une chaîne de sélection de travaux (ex: "1-3" ou "1;3").
    /// </summary>
    /// <param name="input">La chaîne à parser</param>
    /// <returns>Liste des indices sélectionnés</returns>
    public static IEnumerable<int> ParseInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Enumerable.Empty<int>();

        // Split by semicolon first to handle multiple parts
        var parts = input.Split(';');
        var result = new List<int>();

        foreach (var part in parts)
        {
            // Handle range format (e.g., "1-3")
            if (part.Contains('-'))
            {
                var rangeParts = part.Split('-');
                if (rangeParts.Length == 2 && 
                    int.TryParse(rangeParts[0], out int start) && 
                    int.TryParse(rangeParts[1], out int end))
                {
                    result.AddRange(Enumerable.Range(start, end - start + 1));
                }
            }
            // Handle single number
            else if (int.TryParse(part, out int number))
            {
                result.Add(number);
            }
        }

        return result.OrderBy(x => x).Distinct();
    }
}
