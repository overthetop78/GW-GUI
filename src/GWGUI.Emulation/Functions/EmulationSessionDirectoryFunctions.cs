using System.IO;

namespace GWGUI.Emulation.Functions;

/// <summary>Nettoyage commun des dossiers de session possédés par une machine d’émulation.</summary>
/// <remarks>À appeler après arrêt et destruction du cœur. Un dossier emprunté ou destiné à conserver des données utilisateur ne doit pas être confié à ces opérations.</remarks>
internal sealed class EmulationSessionDirectoryFunctions : IEmulationSessionDirectories
{
    /// <summary>Supprime récursivement le dossier de session avec les règles existantes de chemin et d’erreur.</summary>
    public void DeleteSessionDirectory(string sessionDirectory)
        => DeleteSessionDirectory(sessionDirectory, static path =>
        {
            if (Directory.Exists(path) && !string.IsNullOrWhiteSpace(Path.GetFileName(path)))
                Directory.Delete(path, recursive: true);
        });

    /// <summary>Supprime une session générée à nom GUID après retrait des attributs lecture seule de ses fichiers ordinaires.</summary>
    public void DeleteGeneratedSessionDirectory(string sessionDirectory)
        => DeleteSessionDirectory(sessionDirectory, static path =>
        {
            if (!Directory.Exists(path) || !Guid.TryParse(Path.GetFileName(path), out _)) return;
            foreach (var file in Directory.EnumerateFiles(path, "*", new EnumerationOptions { RecurseSubdirectories = true, AttributesToSkip = FileAttributes.ReparsePoint }))
                File.SetAttributes(file, File.GetAttributes(file) & ~FileAttributes.ReadOnly);
            Directory.Delete(path, recursive: true);
        });

    private static void DeleteSessionDirectory(string sessionDirectory, Action<string> deleteDirectory)
    {
        try
        {
            var path = Path.GetFullPath(sessionDirectory);
            deleteDirectory(path);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
