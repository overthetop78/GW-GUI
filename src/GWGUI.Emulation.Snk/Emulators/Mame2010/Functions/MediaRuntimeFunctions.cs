using GWGUI.Emulation.Snk.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions;
using System.IO;
using GWGUI.Emulation.Snk.Emulators.Mame2010.Constants;

namespace GWGUI.Emulation.Snk.Emulators.Mame2010.Functions;
internal static class MediaRuntimeFunctions
{
    private static readonly IReadOnlyDictionary<string, string> ParentDrivers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["bstarsh"] = "bstars",
        ["ridheroh"] = "ridhero",
        ["kotmh"] = "kotm",
        ["sengokh"] = "sengoku",
        ["burningfh"] = "burningf",
        ["gpilotsh"] = "gpilots",
        ["quizdaisk"] = "quizdais",
        ["2020bba"] = "2020bb",
        ["2020bbh"] = "2020bb",
        ["socbrawlh"] = "socbrawl",
        ["samshoh"] = "samsho",
        ["tophuntrh"] = "tophuntr",
        ["aof2a"] = "aof2",
        ["fatfursa"] = "fatfursp",
        ["samsho2k"] = "samsho2",
        ["kof95h"] = "kof95",
        ["samsho3h"] = "samsho3",
        ["fswords"] = "samsho3",
        ["rbff1a"] = "rbff1",
        ["aof3k"] = "aof3",
        ["kof96h"] = "kof96",
        ["samsho4k"] = "samsho4",
        ["rbffspeck"] = "rbffspec",
        ["kof97h"] = "kof97",
        ["kof97pls"] = "kof97",
        ["kog"] = "kof97",
        ["lastbladh"] = "lastblad",
        ["lastsold"] = "lastblad",
        ["rbff2h"] = "rbff2",
        ["rbff2k"] = "rbff2",
        ["kof98k"] = "kof98",
        ["kof98ka"] = "kof98",
        ["kof98h"] = "kof98",
        ["kof99a"] = "kof99",
        ["kof99e"] = "kof99",
        ["kof99k"] = "kof99",
        ["kof99p"] = "kof99",
        ["garouo"] = "garou",
        ["garoup"] = "garou",
        ["garoubl"] = "garou",
        ["mslug3h"] = "mslug3",
        ["mslug3b6"] = "mslug3",
        ["kof2000n"] = "kof2000",
        ["kof2001h"] = "kof2001",
        ["cthd2003"] = "kof2001",
        ["ct2k3sp"] = "kof2001",
        ["ct2k3sa"] = "kof2001",
        ["kof2002b"] = "kof2002",
        ["kf2k2pls"] = "kof2002",
        ["kf2k2pla"] = "kof2002",
        ["kf2k2mp"] = "kof2002",
        ["kf2k2mp2"] = "kof2002",
        ["kof10th"] = "kof2002",
        ["kf10thep"] = "kof2002",
        ["kf2k5uni"] = "kof2002",
        ["kof2k4se"] = "kof2002",
        ["mslug5h"] = "mslug5",
        ["ms5plus"] = "mslug5",
        ["svcpcba"] = "svcpcb",
        ["svcboot"] = "svc",
        ["svcplus"] = "svc",
        ["svcplusa"] = "svc",
        ["svcsplus"] = "svc",
        ["samsho5h"] = "samsho5",
        ["samsho5b"] = "samsho5",
        ["kof2003h"] = "kof2003",
        ["kf2k3bl"] = "kof2003",
        ["kf2k3bla"] = "kof2003",
        ["kf2k3pl"] = "kof2003",
        ["kf2k3upl"] = "kof2003",
        ["samsh5sph"] = "samsh5sp",
        ["samsh5spn"] = "samsh5sp",
        ["maglordh"] = "maglord",
        ["ncombath"] = "ncombat",
        ["wh1h"] = "wh1",
        ["wh1ha"] = "wh1",
        ["wh2jh"] = "wh2j",
        ["nitdbl"] = "nitd",
        ["quizkofk"] = "quizkof",
        ["shocktra"] = "shocktro",
        ["lans2004"] = "shocktr2",
        ["pbobblenb"] = "pbobblen",
        ["fightfeva"] = "fightfev",
        ["puzzldpr"] = "puzzledp",
        ["mslug4h"] = "mslug4",
        ["ms4plus"] = "mslug4",
        ["matrimbl"] = "matrim",
        ["vlinero"] = "vliner"
    };
    internal static IReadOnlyList<EmulationMediaDevice> Devices(MachineModel model) => model == MachineModel.NeoGeoMvs ? [new(EmulationMediaSlot.Cartridge0, EmulationMediaType.Cartridge, RuntimeOptionConstants.CartridgeExtensions, RequiresMachineRecreation: true)] : [];
    internal static IReadOnlyList<string> InitialContentPaths(MachineConfiguration configuration)
    {
        var path = configuration.InitialContent is { Length: > 0 } initial ? initial : configuration.Media.SingleOrDefault(media => media.Slot == EmulationMediaSlot.Cartridge0 && media.IsInserted)?.Path;
        return path is null ? [] : [ContentFunctions.Validate(path, RuntimeOptionConstants.CartridgeExtensions.Select(extension => extension.TrimStart(MediaConstants.ExtensionPrefix)).ToHashSet(StringComparer.OrdinalIgnoreCase))];
    }

    internal static LoadedContent? Prepare(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions)
    {
        if (configuration.Model != MachineModel.NeoGeoMvs)
            throw new ArgumentException(nameof(configuration));
        var paths = InitialContentPaths(configuration);
        if (paths.Count == 0)
            return null;
        var source = paths[0];
        if (Path.GetFileName(source).Equals(RuntimeOptionConstants.SystemArchive, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(configuration));
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
                foreach (var extension in RuntimeOptionConstants.CartridgeExtensions)
                {
                    var parentArchive = Path.Combine(sourceDirectory, parent + extension);
                    if (File.Exists(parentArchive))
                        directory.AddFile(parentArchive);
                }

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
                try
                {
                    content?.Dispose();
                }
                finally
                {
                    directory.Dispose();
                }
            }
        }
    }
}