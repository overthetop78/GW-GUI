using GWGUI.Emulation.Snk.Emulators.Geolith.Functions;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Snk.Emulators.Geolith.Constants;
using GWGUI.Emulation.Snk.Emulators.Geolith.Dictionaries;
using NativeMedia = GWGUI.Emulation.Snk.Emulators.Geolith.Functions.MediaRuntimeFunctions;
using NativeOptions = GWGUI.Emulation.Snk.Emulators.Geolith.Functions.RuntimeOptionFunctions;
using NativeFirmware = GWGUI.Emulation.Snk.Emulators.Geolith.Functions.FirmwareRuntimeFunctions;

namespace GWGUI.Emulation.Snk.Emulators.Geolith.Factories;
internal sealed class GeolithMachineFactory() : LibretroMachineFactory(EmulatorConstants.Entry)
{
    public override IReadOnlyList<CoreOption> CoreOptions => OptionCatalog.Options;

    public override IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null) => configuration is null ? CoreOptions : NativeOptions.Describe(configuration);
    public override int GetControllerPortCount(MachineConfiguration configuration) => NativeOptions.ControllerPorts(configuration);
    protected override IReadOnlyList<FirmwareCategory> FirmwareRoles(MachineModel model) => NativeFirmware.Roles(model);
    protected override bool IsFirmwareRequired(MachineModel model, FirmwareCategory category) => NativeFirmware.Required(model, category);
    public override async ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken) => [await (NativeFirmware.Inspect(path, configuration.Model, cancellationToken)).ConfigureAwait(false)];
    public override void PrepareFirmware(MachineConfiguration configuration, string systemDirectory) => NativeFirmware.Prepare(configuration, systemDirectory);
    internal override LoadedContent? PrepareContent(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions) => NativeMedia.Prepare(configuration, sessionDirectory, supportedExtensions);
    internal override object? PreparePersistentMedia(MachineConfiguration configuration, string saveDirectory) => NativeMedia.PrepareMemoryCard(configuration, saveDirectory);
    internal override void CompletePersistentMedia(object? state) => NativeMedia.CompleteMemoryCard(state as NativeMedia.PreparedMemoryCard);
    public override bool RequiresMediaRestart(MachineConfiguration configuration, EmulationMediaSlot slot, string? path) => true;
    public override IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => NativeMedia.Devices(configuration);
    public override IReadOnlyList<string> ResolveInitialContentPaths(MachineConfiguration configuration) => NativeMedia.InitialContentPaths(configuration);
    public override IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => NativeOptions.Prepare(configuration);
}