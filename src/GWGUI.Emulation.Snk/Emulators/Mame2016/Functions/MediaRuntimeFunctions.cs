using GWGUI.Emulation.Snk.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Snk.Emulators.Mame2016.Constants;

namespace GWGUI.Emulation.Snk.Emulators.Mame2016.Functions;

internal static class MediaRuntimeFunctions
{
    private static readonly MameMediaFunctions Media = new(RuntimeOptionConstants.CartridgeExtensions,
        RuntimeOptionConstants.NeedsFullPath, 512, RuntimeOptionConstants.NativeSystemDirectory,
        FirmwareRuntimeFunctions.BiosDirectory, FirmwareRuntimeFunctions.Driver, FirmwareRuntimeFunctions.SelectedBios);

    internal static IReadOnlyList<EmulationMediaDevice> Devices(MachineModel model) => Media.Devices(model);
    internal static IReadOnlyList<string> InitialContentPaths(MachineConfiguration configuration) => Media.InitialContentPaths(configuration);
    internal static LoadedContent? Prepare(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions) =>
        Media.Prepare(configuration, sessionDirectory, supportedExtensions);
    internal static MameMediaFunctions.PreparedMemoryCard? PrepareMemoryCard(MachineConfiguration configuration, LoadedContent? content) =>
        Media.PrepareMemoryCard(configuration, content);
    internal static void CompleteMemoryCard(MameMediaFunctions.PreparedMemoryCard? card) => MameMediaFunctions.CompleteMemoryCard(card);
}
