using GWGUI.Emulation.Snk.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions;
using System.IO;
using GWGUI.Emulation.Snk.Emulators.Mame2003.Constants;

namespace GWGUI.Emulation.Snk.Emulators.Mame2003.Functions;
internal static class MediaRuntimeFunctions
{
    private static readonly IReadOnlyDictionary<string, string> ParentDrivers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["ridheroh"] = "ridhero",
        ["kotmh"] = "kotm",
        ["sengokh"] = "sengoku",
        ["burningh"] = "burningf",
        ["2020bbh"] = "2020bb",
        ["fatfursa"] = "fatfursp",
        ["kof95a"] = "kof95",
        ["kof96h"] = "kof96",
        ["kof97a"] = "kof97",
        ["lastblda"] = "lastblad",
        ["rbff2a"] = "rbff2",
        ["kof98n"] = "kof98",
        ["kof99a"] = "kof99",
        ["kof99e"] = "kof99",
        ["kof99n"] = "kof99",
        ["kof99p"] = "kof99",
        ["garouo"] = "garou",
        ["garoup"] = "garou",
        ["mslug3n"] = "mslug3",
        ["kof2000n"] = "kof2000",
        ["maglordh"] = "maglord",
        ["wh1h"] = "wh1",
        ["shocktra"] = "shocktro",
        ["pbobblna"] = "pbobblen",
        ["fightfva"] = "fightfev",
        ["puzzldpr"] = "puzzledp",
        ["breakrev"] = "breakers",
        ["bangbedp"] = "bangbead"
    };
    private static readonly ArchiveMediaFunctions Media = new(RuntimeOptionConstants.CartridgeExtensions,
        RuntimeOptionConstants.SystemArchive, RuntimeOptionConstants.NeedsFullPath);

    internal static IReadOnlyList<EmulationMediaDevice> Devices(MachineModel model) => model == MachineModel.NeoGeoMvs ? [new(EmulationMediaSlot.Cartridge0, EmulationMediaType.Cartridge, RuntimeOptionConstants.CartridgeExtensions, RequiresMachineRecreation: true)] : [];
    internal static IReadOnlyList<string> InitialContentPaths(MachineConfiguration configuration) => Media.InitialContentPaths(configuration);
    internal static LoadedContent? Prepare(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions) =>
        Media.PrepareMame(configuration, sessionDirectory, supportedExtensions, ParentDrivers);
}
