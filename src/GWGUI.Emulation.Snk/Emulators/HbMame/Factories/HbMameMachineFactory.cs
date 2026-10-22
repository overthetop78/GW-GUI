using GWGUI.Emulation.Snk.Emulators.HbMame.Functions;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Snk.Emulators.HbMame.Constants;
using GWGUI.Emulation.Snk.Emulators.HbMame.Dictionaries;
using NativeOptions = GWGUI.Emulation.Snk.Emulators.HbMame.Functions.RuntimeOptionFunctions;
using NativeMedia = GWGUI.Emulation.Snk.Emulators.HbMame.Functions.MediaRuntimeFunctions;
using NativeFirmware = GWGUI.Emulation.Snk.Emulators.HbMame.Functions.FirmwareRuntimeFunctions;

namespace GWGUI.Emulation.Snk.Emulators.HbMame.Factories;
internal sealed class HbMameMachineFactory() : LibretroMachineFactory(EmulatorConstants.Entry)
{
    internal override string ExpectedNativeLibraryName => EmulatorConstants.NativeLibraryName;
    public override IReadOnlyList<CoreOption> CoreOptions => OptionCatalog.Options;

    public override IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null) => configuration is null ? CoreOptions : NativeOptions.Describe(configuration);
    public override IReadOnlyList<FirmwareCategory> FirmwareRoles(MachineModel model) => NativeFirmware.Roles;
    public override bool IsFirmwareRequired(MachineModel model, FirmwareCategory category) => category == FirmwareCategory.SystemBios;
    public override ValueTask<EmulationFirmwareCandidate> ScanFirmwareFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken) => NativeFirmware.Inspect(path, configuration.Model, cancellationToken);
    internal override void PrepareFirmware(MachineConfiguration configuration, string systemDirectory) => NativeFirmware.Prepare(configuration, systemDirectory);
    internal override LoadedContent? PrepareContent(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions) => NativeMedia.Prepare(configuration, sessionDirectory, supportedExtensions);
    internal override object? PreparePersistentMedia(MachineConfiguration configuration, string saveDirectory, LoadedContent? content) => NativeMedia.PrepareMemoryCard(configuration, content);
    internal override void CompletePersistentMedia(object? state) => NativeMedia.CompleteMemoryCard(state as GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions.MameMediaFunctions.PreparedMemoryCard);
    protected override bool RequiresMediaRestart(MachineConfiguration configuration, EmulationMediaSlot slot, string? path) => true;
    public override IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => NativeMedia.Devices(configuration.Model);
    public override IReadOnlyList<string> ResolveInitialContentPaths(MachineConfiguration configuration) => NativeMedia.InitialContentPaths(configuration);
    public override IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => NativeOptions.Prepare(configuration);
}