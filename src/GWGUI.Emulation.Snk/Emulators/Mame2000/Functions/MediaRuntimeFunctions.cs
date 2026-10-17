using GWGUI.Emulation.Snk.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions;
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
    private static readonly ArchiveMediaFunctions Media = new(RuntimeOptionConstants.CartridgeExtensions,
        RuntimeOptionConstants.SystemArchive, RuntimeOptionConstants.NeedsFullPath);

    internal static IReadOnlyList<EmulationMediaDevice> Devices(MachineModel model) => model == MachineModel.NeoGeoMvs ? [new(EmulationMediaSlot.Cartridge0, EmulationMediaType.Cartridge, RuntimeOptionConstants.CartridgeExtensions, RequiresMachineRecreation: true)] : [];
    internal static IReadOnlyList<string> InitialContentPaths(MachineConfiguration configuration) => Media.InitialContentPaths(configuration);
    internal static LoadedContent? Prepare(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions) =>
        Media.PrepareMame(configuration, sessionDirectory, supportedExtensions, ParentDrivers);
}
