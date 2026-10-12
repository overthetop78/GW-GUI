using System.IO;
using GWGUI.Emulation.Snk.Emulators.Mame2000.Constants;

namespace GWGUI.Emulation.Snk.Emulators.Mame2000.Functions;

internal static class MediaRuntimeFunctions
{
    private static readonly IReadOnlyDictionary<string, string> ParentDrivers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["sengokh"] = "sengoku",
        ["burningh"] = "burningf",
        ["2020bbh"] = "2020bb",
        ["maglordh"] = "maglord",
        ["puzzldpr"] = "puzzledp",
        ["breakrev"] = "breakers"
    };
    internal static IReadOnlyList<EmulationMediaDevice> Devices(MachineModel model) => model == MachineModel.NeoGeoMvs ? [new(EmulationMediaSlot.Cartridge0, EmulationMediaType.Cartridge, RuntimeOptionConstants.CartridgeExtensions, RequiresMachineRecreation: true)] : [];

    internal static IReadOnlyList<string> InitialContentPaths(MachineConfiguration configuration)
    {
        var path = configuration.InitialContent is { Length: > 0 } initial ? initial : configuration.Media.SingleOrDefault(media => media.Slot == EmulationMediaSlot.Cartridge0 && media.IsInserted)?.Path;
        return path is null ? [] : [ContentFunctions.Validate(path, RuntimeOptionConstants.CartridgeExtensions.Select(extension => extension.TrimStart(MediaConstants.ExtensionPrefix)).ToHashSet(StringComparer.OrdinalIgnoreCase))];
    }

    internal static LoadedContent? Prepare(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions)
    {
        if (configuration.Model != MachineModel.NeoGeoMvs) throw new ArgumentException(nameof(configuration));
        var paths = InitialContentPaths(configuration);
        if (paths.Count == 0) return null;
        var source = paths[0];
        if (Path.GetFileName(source).Equals(RuntimeOptionConstants.SystemArchive, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(configuration));
        var sourceDirectory = Path.GetDirectoryName(source)!;
        var directory = new SessionContentDirectory(sessionDirectory, sourceDirectory);
        LoadedContent? content = null;
        var completed = false;
        try
        {
            var stagedSource = directory.AddFile(source);
            var driver = Path.GetFileNameWithoutExtension(source);
            while (ParentDrivers.TryGetValue(driver, out var parent))
            {
                var parentArchive = Path.Combine(sourceDirectory, parent + Path.GetExtension(source));
                if (File.Exists(parentArchive)) directory.AddFile(parentArchive);
                driver = parent;
            }
            directory.AddFile(Path.Combine(sessionDirectory, CoreDirectoryConstants.SystemDirectoryName, RuntimeOptionConstants.SystemArchive), RuntimeOptionConstants.SystemArchive);
            content = ContentFunctions.Create(stagedSource, RuntimeOptionConstants.NeedsFullPath, supportedExtensions, useWindowsAnsiPath: true);
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
