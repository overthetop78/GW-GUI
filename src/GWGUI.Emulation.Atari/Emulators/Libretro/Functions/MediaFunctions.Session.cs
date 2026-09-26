using System.Globalization;

namespace GWGUI.Emulation.Atari.Emulators.Libretro.Functions;

internal static class SessionMediaFunctions
{
    internal static SessionMedia Prepare(
        MediaConfiguration configuration,
        string sessionDirectory,
        IReadOnlySet<string> supportedExtensions)
    {
        var sourcePaths = ReadSourcePaths(configuration.Path);
        foreach (var sourcePath in sourcePaths)
            ContentFunctions.Validate(sourcePath, supportedExtensions);

        if (configuration.IsReadOnly)
            return new SessionMedia(configuration, configuration.Path, sourcePaths, sourcePaths, false);

        var runtimeDirectory = Path.Combine(sessionDirectory, SessionMediaConstants.SessionDirectoryName,
            string.Format(CultureInfo.InvariantCulture, SessionMediaConstants.SessionInstanceNameFormat,
                configuration.Slot, Guid.NewGuid().ToString(SessionMediaConstants.UniqueNameFormat)));
        Directory.CreateDirectory(runtimeDirectory);
        var runtimePaths = new List<string>(sourcePaths.Count);
        for (var index = SessionMediaConstants.FirstMediaIndex; index < sourcePaths.Count; index++)
        {
            var sourcePath = sourcePaths[index];
            var runtimeName = string.Format(CultureInfo.InvariantCulture,
                SessionMediaConstants.RuntimeFileNameFormat,
                index + SessionMediaConstants.RuntimeFileNumberOffset,
                Path.GetFileName(sourcePath));
            var runtimePath = Path.Combine(runtimeDirectory, runtimeName);
            File.Copy(sourcePath, runtimePath, overwrite: true);
            MakeWritable(runtimePath);
            runtimePaths.Add(runtimePath);
        }

        var runtimeContentPath = runtimePaths[SessionMediaConstants.FirstMediaIndex];
        if (IsPlaylist(configuration.Path))
        {
            runtimeContentPath = Path.Combine(runtimeDirectory, SessionMediaConstants.RuntimePlaylistFileName);
            File.WriteAllLines(runtimeContentPath, runtimePaths.Select(path => Path.GetFileName(path)!));
        }

        return new SessionMedia(configuration, runtimeContentPath, sourcePaths, runtimePaths, true);
    }

    internal static void Save(SessionMedia media)
    {
        if (!media.RequiresExplicitSave)
            throw new InvalidOperationException(SessionMediaErrors.ExplicitSaveRequired);
        for (var index = SessionMediaConstants.FirstMediaIndex; index < media.SourcePaths.Count; index++)
            File.Copy(media.RuntimePaths[index], media.SourcePaths[index], overwrite: true);
    }

    internal static IReadOnlyList<string> ReadSourcePaths(string contentPath)
    {
        var absoluteContentPath = Path.GetFullPath(contentPath);
        if (!IsPlaylist(absoluteContentPath)) return [absoluteContentPath];
        var playlistDirectory = Path.GetDirectoryName(absoluteContentPath)!;
        var paths = File.ReadLines(absoluteContentPath)
            .Select(line => line.Trim())
            .Where(line => line.Length > SessionMediaConstants.FirstMediaIndex &&
                           !line.StartsWith(SessionMediaConstants.PlaylistCommentPrefix,
                               StringComparison.Ordinal))
            .Select(line => Path.GetFullPath(Path.Combine(playlistDirectory, line)))
            .ToArray();
        if (paths.Length == SessionMediaConstants.FirstMediaIndex)
            throw new InvalidDataException(SessionMediaErrors.PlaylistEmpty);
        var missing = paths.FirstOrDefault(path => !File.Exists(path));
        if (missing is not null)
            throw new FileNotFoundException(SessionMediaErrors.PlaylistEntryMissing, missing);
        return paths;
    }

    private static bool IsPlaylist(string path) => string.Equals(
        Path.GetExtension(path), SessionMediaConstants.PlaylistExtension,
        StringComparison.OrdinalIgnoreCase);

    private static void MakeWritable(string path)
    {
        var attributes = File.GetAttributes(path);
        if ((attributes & FileAttributes.ReadOnly) != FileAttributes.ReadOnly) return;
        File.SetAttributes(path, attributes & ~FileAttributes.ReadOnly);
    }
}
