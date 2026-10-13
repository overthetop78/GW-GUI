using GWGUI.Emulation.Sinclair.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Sinclair.Emulators.Common.Interop.Factories;
using NativeSettings = GWGUI.Emulation.Sinclair.Emulators.Fuse.Functions.SettingsDescriptionFunctions;
using GWGUI.Emulation.Sinclair.Emulators.Fuse.Constants;
using NativeMedia = GWGUI.Emulation.Sinclair.Emulators.Fuse.Functions.MediaRuntimeFunctions;
using NativeOptions = GWGUI.Emulation.Sinclair.Emulators.Fuse.Functions.RuntimeOptionFunctions;
using NativeFirmware = GWGUI.Emulation.Sinclair.Emulators.Fuse.Functions.FirmwareRuntimeFunctions;

namespace GWGUI.Emulation.Sinclair.Emulators.Fuse.Factories;

internal sealed class FuseMachineFactory() : LibretroMachineFactory(EmulatorConstants.Entry), IEmulatorSettings
{
    public override IReadOnlyList<PeripheralCategory> GetPeripherals(MachineModel model) => Enum.GetValues<PeripheralCategory>();
    protected override bool SupportsInitialContent => true;

    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration) => NativeSettings.Create(configuration);
    public IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null) => NativeSettings.GetOptions(configuration);

    protected override bool SupportsNoContent => true;
    protected override bool SupportsPointerCapture => true;
    protected override bool RequiresMediaRestart(MachineConfiguration configuration, EmulationMediaSlot slot, string? path) => !NativeMedia.CanExchange(configuration, slot);
    internal override bool SupportsMediaExchange(MachineConfiguration configuration, EmulationMediaSlot slot) => NativeMedia.CanExchange(configuration, slot);
    internal override LoadedContent? PrepareContent(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions) => NativeMedia.Prepare(configuration, sessionDirectory, supportedExtensions);
    internal override void CompleteMediaLoad(MachineConfiguration configuration, string sessionDirectory, ExternalHostCallbacks callbacks) => NativeMedia.CompleteLoad(configuration, sessionDirectory, callbacks);
    internal override void PrepareFirmware(MachineConfiguration configuration, string systemDirectory) => NativeFirmware.Prepare(configuration, systemDirectory);
    public override IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => NativeOptions.Prepare(configuration);
    public override IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => NativeMedia.Devices(configuration);
    public override IReadOnlyList<string> ResolveInitialContentPaths(MachineConfiguration configuration) => NativeMedia.InitialContentPaths(configuration);
}
