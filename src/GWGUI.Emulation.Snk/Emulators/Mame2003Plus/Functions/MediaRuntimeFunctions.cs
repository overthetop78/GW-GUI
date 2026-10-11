using System.IO;
using GWGUI.Emulation.Snk.Emulators.Mame2003Plus.Constants;

namespace GWGUI.Emulation.Snk.Emulators.Mame2003Plus.Functions;

internal static class MediaRuntimeFunctions
{
    private static readonly IReadOnlyDictionary<string, string> ParentDrivers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["ridheroh"] = "ridhero", ["kotmh"] = "kotm", ["sengokh"] = "sengoku", ["burningh"] = "burningf", ["2020bbh"] = "2020bb", ["fatfursa"] = "fatfursp", ["kof95a"] = "kof95", ["kof96h"] = "kof96", ["kof97a"] = "kof97", ["kof97pls"] = "kof97", ["kog"] = "kof97", ["lastblda"] = "lastblad", ["rbff2a"] = "rbff2", ["mslug2t"] = "mslug2", ["kof98k"] = "kof98", ["kof98n"] = "kof98", ["kof99a"] = "kof99", ["kof99e"] = "kof99", ["kof99n"] = "kof99", ["kof99p"] = "kof99", ["garouo"] = "garou", ["garoup"] = "garou", ["mslug3n"] = "mslug3", ["kof2000n"] = "kof2000", ["maglordh"] = "maglord", ["wh1h"] = "wh1", ["kof2001h"] = "kof2001", ["cthd2003"] = "kof2001", ["ct2k3sp"] = "kof2001", ["kf2k2pls"] = "kof2002", ["kf2k2pla"] = "kof2002", ["kf2k2mp"] = "kof2002", ["kf2k2mp2"] = "kof2002", ["kof2k4se"] = "kof2002", ["kof10th"] = "kof2002", ["kf10thep"] = "kof2002", ["kf2k5uni"] = "kof2002", ["shocktra"] = "shocktro", ["svcboot"] = "svc", ["svcplus"] = "svc", ["svcplusa"] = "svc", ["svcsplus"] = "svc", ["kof2003d"] = "kof2003", ["kf2k3bl"] = "kof2003", ["kf2k3bla"] = "kof2003", ["kf2k3pl"] = "kof2003", ["kf2k3upl"] = "kof2003", ["pbobblna"] = "pbobblen", ["fightfva"] = "fightfev", ["puzzldpr"] = "puzzledp", ["breakrev"] = "breakers", ["doubled1a"] = "doubled1", ["doubled1b"] = "doubled1", ["doubled1c"] = "doubled1", ["doubled1d"] = "doubled1", ["doubled1e"] = "doubled1", ["bangbedp"] = "bangbead" };
    internal static IReadOnlyList<EmulationMediaDevice> Devices(MachineModel model) => model is MachineModel.NeoGeoMvs or MachineModel.NeoGeoAes ? [new(EmulationMediaSlot.Cartridge0, EmulationMediaType.Cartridge, RuntimeOptionConstants.CartridgeExtensions, RequiresMachineRecreation: true)] : [];

    internal static IReadOnlyList<string> InitialContentPaths(MachineConfiguration configuration)
    {
        var path = configuration.InitialContent is { Length: > 0 } initial ? initial : configuration.Media.SingleOrDefault(media => media.Slot == EmulationMediaSlot.Cartridge0 && media.IsInserted)?.Path;
        return path is null ? [] : [ContentFunctions.Validate(path, RuntimeOptionConstants.CartridgeExtensions.Select(extension => extension.TrimStart(MediaConstants.ExtensionPrefix)).ToHashSet(StringComparer.OrdinalIgnoreCase))];
    }

    internal static LoadedContent? Prepare(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions)
    {
        if (configuration.Model is not (MachineModel.NeoGeoMvs or MachineModel.NeoGeoAes)) throw new ArgumentException(nameof(configuration));
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
