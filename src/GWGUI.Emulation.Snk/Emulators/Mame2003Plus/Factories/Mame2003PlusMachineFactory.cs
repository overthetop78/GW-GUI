using GWGUI.Emulation.Snk.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Snk.Emulators.Mame2003Plus.Constants;
using GWGUI.Emulation.Snk.Emulators.Mame2003Plus.Dictionaries;
using NativeOptions = GWGUI.Emulation.Snk.Emulators.Mame2003Plus.Functions.RuntimeOptionFunctions;
using NativeMedia = GWGUI.Emulation.Snk.Emulators.Mame2003Plus.Functions.MediaRuntimeFunctions;
using NativeFirmware = GWGUI.Emulation.Snk.Emulators.Mame2003Plus.Functions.FirmwareRuntimeFunctions;

namespace GWGUI.Emulation.Snk.Emulators.Mame2003Plus.Factories;
internal sealed class Mame2003PlusMachineFactory() : LibretroMachineFactory(EmulatorConstants.Entry)
{
    public override IReadOnlyList<CoreOption> CoreOptions => OptionCatalog.Options;

    public override IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null) => configuration is null ? CoreOptions : NativeOptions.Describe(configuration);
    protected override IReadOnlyList<FirmwareCategory> FirmwareRoles(MachineModel model) => NativeFirmware.Roles;
    protected override bool IsFirmwareRequired(MachineModel model, FirmwareCategory category) => category == FirmwareCategory.SystemBios;
    public override async ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken) => [await (NativeFirmware.Inspect(path, configuration.Model, cancellationToken)).ConfigureAwait(false)];
    public override void PrepareFirmware(MachineConfiguration configuration, string systemDirectory) => NativeFirmware.Prepare(configuration, systemDirectory);
    internal override LoadedContent? PrepareContent(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions) => NativeMedia.Prepare(configuration, sessionDirectory, supportedExtensions);
    public override bool RequiresMediaRestart(MachineConfiguration configuration, EmulationMediaSlot slot, string? path) => true;
    public override IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => NativeMedia.Devices(configuration.Model);
    public override IReadOnlyList<string> ResolveInitialContentPaths(MachineConfiguration configuration) => NativeMedia.InitialContentPaths(configuration);
    public override IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => NativeOptions.Prepare(configuration);
}