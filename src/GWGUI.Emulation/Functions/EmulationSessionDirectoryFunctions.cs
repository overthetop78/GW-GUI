using System.IO;

namespace GWGUI.Emulation.Functions;

internal static class EmulationSessionDirectoryFunctions
{
    internal static void DeleteSessionDirectory(string sessionDirectory)
        => DeleteSessionDirectory(sessionDirectory, static path =>
        {
            if (Directory.Exists(path) && !string.IsNullOrWhiteSpace(Path.GetFileName(path)))
                Directory.Delete(path, recursive: true);
        });

    internal static void DeleteGeneratedSessionDirectory(string sessionDirectory)
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
