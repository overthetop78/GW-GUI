using GWGUI.Emulation.Apple.Emulators.Common.Constants;
using GWGUI.Emulation.Apple.Emulators.Common.Contracts;
using GWGUI.Emulation.Apple.Emulators.Common.Exceptions;
using GWGUI.Emulation.Apple.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Interop;
using System.IO;
using System.Globalization;
using System.Text.Json;

namespace GWGUI.Emulation.Apple.Emulators.Common.Functions;

internal static class SessionMediaFunctions
{
    internal static SessionMedia Prepare(
        MediaConfiguration configuration,
        string sessionDirectory,
        IReadOnlySet<string> supportedExtensions)
    {
        var mediaDirectory = Path.Combine(sessionDirectory, SessionMediaConstants.SessionDirectoryName);
        var prepared = FindPrepared(configuration, mediaDirectory);
        if (prepared is not null) return prepared;
        var sourcePaths = ReadSourcePaths(configuration.Path);
        foreach (var sourcePath in sourcePaths)
            ContentFunctions.Validate(sourcePath, supportedExtensions);



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
            SetReadOnly(runtimePath, configuration.IsReadOnly);
            runtimePaths.Add(runtimePath);
        }

        var runtimeContentPath = runtimePaths[SessionMediaConstants.FirstMediaIndex];
        if (IsPlaylist(configuration.Path))
        {
            runtimeContentPath = Path.Combine(runtimeDirectory, SessionMediaConstants.RuntimePlaylistFileName);
            var nextPath = SessionMediaConstants.FirstMediaIndex;
            File.WriteAllLines(runtimeContentPath, File.ReadLines(configuration.Path).Select(line =>
            {
                var trimmed = line.Trim();
                if (trimmed.Length == 0 || trimmed.StartsWith(SessionMediaConstants.PlaylistCommentPrefix,
                        StringComparison.Ordinal)) return line;
                var separator = line.IndexOf(SessionMediaConstants.PlaylistLabelSeparator);
                var label = separator < 0 ? string.Empty : line[separator..];
                return Path.GetFileName(runtimePaths[nextPath++]) + label;
            }));
        }

        var result = new SessionMedia(configuration, runtimeContentPath, sourcePaths, runtimePaths, !configuration.IsReadOnly);
        File.WriteAllText(Path.Combine(runtimeDirectory, SessionMediaConstants.SessionManifestFileName),
            JsonSerializer.Serialize(result));
        return result;
    }

    private static SessionMedia? FindPrepared(MediaConfiguration configuration, string directory)
    {
        if (!Directory.Exists(directory)) return null;
        var root = Path.GetFullPath(directory) + Path.DirectorySeparatorChar;
        foreach (var path in Directory.EnumerateFiles(directory, SessionMediaConstants.SessionManifestFileName,
                     SearchOption.AllDirectories).OrderByDescending(File.GetLastWriteTimeUtc))
        {
            var prepared = JsonSerializer.Deserialize<SessionMedia>(File.ReadAllText(path));
            if (prepared is null || prepared.Configuration.Slot != configuration.Slot
                || prepared.Configuration.IsReadOnly != configuration.IsReadOnly
                || !string.Equals(Path.GetFullPath(prepared.Configuration.Path), Path.GetFullPath(configuration.Path),
                    StringComparison.OrdinalIgnoreCase)) continue;
            if (!prepared.RuntimePaths.Append(prepared.RuntimePath).All(file =>
                    Path.GetFullPath(file).StartsWith(root, StringComparison.OrdinalIgnoreCase) && File.Exists(file)))
                continue;
            return prepared with { Configuration = configuration };
        }
        return null;
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
            .Select(line => line.Split(SessionMediaConstants.PlaylistLabelSeparator)[0])
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

    private static void SetReadOnly(string path, bool readOnly)
    {
        var attributes = File.GetAttributes(path);
        File.SetAttributes(path, readOnly ? attributes | FileAttributes.ReadOnly : attributes & ~FileAttributes.ReadOnly);
    }
}
