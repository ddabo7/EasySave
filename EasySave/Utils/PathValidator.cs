/// <summary>
/// Valide les chemins source et destination.
/// </summary>
public static class PathValidator
{
    /// <summary>
    /// Convertit un chemin avec ~ en chemin absolu.
    /// </summary>
    private static string ExpandPath(string path)
    {
        if (path.StartsWith("~"))
        {
            string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return path.Replace("~", homeDirectory);
        }
        return path;
    }

    /// <summary>
    /// Vérifie si un chemin source est valide.
    /// </summary>
    public static bool ValidateSourcePath(string path)
    {
        try
        {
            var expandedPath = ExpandPath(path);
            
            // Vérifier si le chemin est valide
            if (string.IsNullOrWhiteSpace(expandedPath))
                return false;

            // Vérifier si le chemin existe
            if (!Directory.Exists(expandedPath))
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Vérifie si un chemin de destination est valide ou peut être créé.
    /// </summary>
    public static bool ValidateDestinationPath(string path)
    {
        try
        {
            var expandedPath = ExpandPath(path);
            
            // Vérifier si le chemin est valide
            if (string.IsNullOrWhiteSpace(expandedPath))
                return false;

            // Créer le répertoire s'il n'existe pas
            if (!Directory.Exists(expandedPath))
            {
                Directory.CreateDirectory(expandedPath);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Vérifie si un chemin UNC est valide.
    /// </summary>
    public static bool ValidateUNCPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        // Vérifier si c'est un chemin UNC
        if (path.StartsWith(@"\\"))
        {
            try
            {
                var uri = new Uri(path);
                return uri.IsUnc;
            }
            catch
            {
                return false;
            }
        }

        return true; // Chemin local
    }

    /// <summary>
    /// Convertit un chemin en format UNC si nécessaire.
    /// </summary>
    public static string ToUncPath(string path)
    {
        var expandedPath = ExpandPath(path);
        
        if (expandedPath.StartsWith(@"\\"))
            return expandedPath;  // Déjà au format UNC

        return Path.GetFullPath(expandedPath);
    }
}
