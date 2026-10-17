using GWGUI.Emulation.Snk.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Constants;
using System.IO;
using GWGUI.Emulation.Snk.Emulators.FbAlpha2012NeoGeo.Constants;

namespace GWGUI.Emulation.Snk.Emulators.FbAlpha2012NeoGeo.Functions;

internal static class MediaRuntimeFunctions
{
    private static readonly ArchiveMediaFunctions Media = new(RuntimeOptionConstants.CartridgeExtensions,
        RuntimeOptionConstants.SystemArchive, RuntimeOptionConstants.NeedsFullPath);

    internal static IReadOnlyList<EmulationMediaDevice> Devices => [new(EmulationMediaSlot.Cartridge0, EmulationMediaType.Cartridge, RuntimeOptionConstants.CartridgeExtensions, RequiresMachineRecreation: true)];
    internal static IReadOnlyList<string> InitialContentPaths(MachineConfiguration configuration) => Media.InitialContentPaths(configuration);
    internal static LoadedContent? Prepare(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions) =>
        Media.PrepareFba(configuration, sessionDirectory, supportedExtensions);
}
