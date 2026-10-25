using System.IO;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Constants;

namespace GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions;

internal sealed class ArchiveMediaFunctions(IReadOnlyList<string> cartridgeExtensions, string systemArchive, bool needsFullPath)
{
    internal IReadOnlyList<string> InitialContentPaths(MachineConfiguration configuration)
    {
        var path = configuration.InitialContent is { Length: > 0 } initial ? initial : configuration.Media.SingleOrDefault(media => media.Slot == EmulationMediaSlot.Cartridge0 && media.IsInserted)?.Path;
        return path is null ? [] : [ContentFunctions.Validate(path, cartridgeExtensions.Select(extension => extension.TrimStart(MediaConstants.ExtensionPrefix)).ToHashSet(StringComparer.OrdinalIgnoreCase))];
    }


    internal LoadedContent? PrepareMame(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions, IReadOnlyDictionary<string, string> parentDrivers)
    {
        if (configuration.Model != MachineModel.NeoGeoMvs) throw new ArgumentException(nameof(configuration));
        var paths = InitialContentPaths(configuration);
        if (paths.Count == 0) return null;
        var source = paths[0];
        if (Path.GetFileName(source).Equals(systemArchive, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(configuration));
        var sourceDirectory = Path.GetDirectoryName(source)!;
        var directory = new SessionContentDirectory(sessionDirectory, sourceDirectory);
        LoadedContent? content = null;
        var completed = false;
        try
        {
            var stagedSource = directory.AddFile(source);
            var driver = Path.GetFileNameWithoutExtension(source);
            while (parentDrivers.TryGetValue(driver, out var parent))
            {
                var parentArchive = Path.Combine(sourceDirectory, parent + Path.GetExtension(source));
                if (File.Exists(parentArchive)) directory.AddFile(parentArchive);
                driver = parent;
            }
            directory.AddFile(Path.Combine(sessionDirectory, CoreDirectoryConstants.SystemDirectoryName, systemArchive), systemArchive);
            content = ContentFunctions.Create(stagedSource, needsFullPath, supportedExtensions, useWindowsAnsiPath: true);
            content.OwnContentDirectory(directory);
            completed = true;
            return content;
        }
        finally
        {
            if (!completed)
            {
                try { content?.Dispose(); }
                finally { directory.Dispose(); }
            }
        }
    }

    internal LoadedContent? PrepareFba(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions)
    {
        var paths = InitialContentPaths(configuration);
        if (paths.Count == 0) return null;
        var sourceDirectory = Path.GetDirectoryName(paths[0])!;
        var directory = new SessionContentDirectory(sessionDirectory, sourceDirectory);
        LoadedContent? content = null;
        var completed = false;
        try
        {
            var stagedPath = directory.AddFile(paths[0]);
            foreach (var archive in Directory.EnumerateFiles(sourceDirectory, SessionContentConstants.AllFilesPattern))
            {
                if (!cartridgeExtensions.Contains(Path.GetExtension(archive), StringComparer.OrdinalIgnoreCase) || Path.GetFileName(archive).Equals(systemArchive, StringComparison.OrdinalIgnoreCase)) continue;
                directory.AddFile(archive);
            }
            directory.AddFile(Path.Combine(sessionDirectory, CoreDirectoryConstants.SystemDirectoryName, systemArchive), systemArchive);
            content = ContentFunctions.Create(stagedPath, needsFullPath, supportedExtensions);
            content.OwnContentDirectory(directory);
            completed = true;
            return content;
        }
        finally
        {
            if (!completed)
            {
                try { content?.Dispose(); }
                finally { directory.Dispose(); }
            }
        }
    }
}
