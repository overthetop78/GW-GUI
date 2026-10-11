using System.IO;
using GWGUI.Emulation.Snk.Emulators.FbAlpha2012.Constants;

namespace GWGUI.Emulation.Snk.Emulators.FbAlpha2012.Functions;

internal static class MediaRuntimeFunctions
{
    internal static IReadOnlyList<EmulationMediaDevice> Devices => [new(EmulationMediaSlot.Cartridge0, EmulationMediaType.Cartridge, RuntimeOptionConstants.CartridgeExtensions, RequiresMachineRecreation: true)];
    internal static IReadOnlyList<string> InitialContentPaths(MachineConfiguration configuration)
    {
        var path = configuration.InitialContent is { Length: > 0 } initial ? initial : configuration.Media.SingleOrDefault(media => media.Slot == EmulationMediaSlot.Cartridge0 && media.IsInserted)?.Path;
        return path is null ? [] : [ContentFunctions.Validate(path, RuntimeOptionConstants.CartridgeExtensions.Select(extension => extension.TrimStart(MediaConstants.ExtensionPrefix)).ToHashSet(StringComparer.OrdinalIgnoreCase))];
    }

    internal static LoadedContent? Prepare(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions)
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
                if (!RuntimeOptionConstants.CartridgeExtensions.Contains(Path.GetExtension(archive), StringComparer.OrdinalIgnoreCase) || Path.GetFileName(archive).Equals(RuntimeOptionConstants.SystemArchive, StringComparison.OrdinalIgnoreCase)) continue;
                directory.AddFile(archive);
            }
            directory.AddFile(Path.Combine(sessionDirectory, CoreDirectoryConstants.SystemDirectoryName, RuntimeOptionConstants.SystemArchive), RuntimeOptionConstants.SystemArchive);
            content = ContentFunctions.Create(stagedPath, RuntimeOptionConstants.NeedsFullPath, supportedExtensions);
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
