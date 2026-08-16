using System.Globalization;

namespace GWGUI.Emulation.Atari;

internal static class AtariSessionMediaFunctions
{
    internal static AtariSessionMedia Prepare(
        AtariMediaConfiguration configuration,
        string sessionDirectory,
        IReadOnlySet<string> supportedExtensions)
    {
        var sourcePaths = ReadSourcePaths(configuration.Path);
        foreach (var sourcePath in sourcePaths)
            Cores.AtariContentFunctions.Validate(sourcePath, supportedExtensions);

        if (configuration.IsReadOnly)
            return new AtariSessionMedia(configuration, configuration.Path, sourcePaths, sourcePaths, false);

        var runtimeDirectory = Path.Combine(sessionDirectory, AtariSessionMediaConstants.SessionDirectoryName,
            string.Format(CultureInfo.InvariantCulture, AtariSessionMediaConstants.SessionInstanceNameFormat,
                configuration.Slot, Guid.NewGuid().ToString(AtariSessionMediaConstants.UniqueNameFormat)));
        Directory.CreateDirectory(runtimeDirectory);
        var runtimePaths = new List<string>(sourcePaths.Count);
        for (var index = AtariSessionMediaConstants.FirstMediaIndex; index < sourcePaths.Count; index++)
        {
            var sourcePath = sourcePaths[index];
            var runtimeName = string.Format(CultureInfo.InvariantCulture,
                AtariSessionMediaConstants.RuntimeFileNameFormat,
                index + AtariSessionMediaConstants.RuntimeFileNumberOffset,
                Path.GetFileName(sourcePath));
            var runtimePath = Path.Combine(runtimeDirectory, runtimeName);
            File.Copy(sourcePath, runtimePath, overwrite: true);
            MakeWritable(runtimePath);
            runtimePaths.Add(runtimePath);
        }

        var runtimeContentPath = runtimePaths[AtariSessionMediaConstants.FirstMediaIndex];
        if (IsPlaylist(configuration.Path))
        {
            runtimeContentPath = Path.Combine(runtimeDirectory, AtariSessionMediaConstants.RuntimePlaylistFileName);
            File.WriteAllLines(runtimeContentPath, runtimePaths.Select(path => Path.GetFileName(path)!));
        }

        return new AtariSessionMedia(configuration, runtimeContentPath, sourcePaths, runtimePaths, true);
    }

    internal static void Save(AtariSessionMedia media)
    {
        if (!media.RequiresExplicitSave)
            throw new InvalidOperationException(AtariSessionMediaErrors.ExplicitSaveRequired);
        for (var index = AtariSessionMediaConstants.FirstMediaIndex; index < media.SourcePaths.Count; index++)
            File.Copy(media.RuntimePaths[index], media.SourcePaths[index], overwrite: true);
    }

    internal static IReadOnlyList<string> ReadSourcePaths(string contentPath)
    {
        var absoluteContentPath = Path.GetFullPath(contentPath);
        if (!IsPlaylist(absoluteContentPath)) return [absoluteContentPath];
        var playlistDirectory = Path.GetDirectoryName(absoluteContentPath)!;
        var paths = File.ReadLines(absoluteContentPath)
            .Select(line => line.Trim())
            .Where(line => line.Length > AtariSessionMediaConstants.FirstMediaIndex &&
                           !line.StartsWith(AtariSessionMediaConstants.PlaylistCommentPrefix,
                               StringComparison.Ordinal))
            .Select(line => Path.GetFullPath(Path.Combine(playlistDirectory, line)))
            .ToArray();
        if (paths.Length == AtariSessionMediaConstants.FirstMediaIndex)
            throw new InvalidDataException(AtariSessionMediaErrors.PlaylistEmpty);
        var missing = paths.FirstOrDefault(path => !File.Exists(path));
        if (missing is not null)
            throw new FileNotFoundException(AtariSessionMediaErrors.PlaylistEntryMissing, missing);
        return paths;
    }

    private static bool IsPlaylist(string path) => string.Equals(
        Path.GetExtension(path), AtariSessionMediaConstants.PlaylistExtension,
        StringComparison.OrdinalIgnoreCase);

    private static void MakeWritable(string path)
    {
        var attributes = File.GetAttributes(path);
        if ((attributes & FileAttributes.ReadOnly) != FileAttributes.ReadOnly) return;
        File.SetAttributes(path, attributes & ~FileAttributes.ReadOnly);
    }
}
