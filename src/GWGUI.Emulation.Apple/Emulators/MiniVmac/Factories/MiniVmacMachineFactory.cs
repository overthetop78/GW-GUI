using GWGUI.Emulation.Apple.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Apple.Emulators.MiniVmac.Constants;

namespace GWGUI.Emulation.Apple.Emulators.MiniVmac.Factories;

internal sealed class MiniVmacMachineFactory() : MachineFactory(EmulatorConstants.Entry), IEmulatorSettings, IEmulatorPointerCapabilities, IEmulatorConfigurationOptions, IEmulatorRuntimePolicy, IEmulatorFirmwareAdapter, IEmulatorFirmwareSettings
{
    public bool SupportsPointer(MachineConfiguration configuration) => true;
    public override IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => GWGUI.Emulation.Apple.Emulators.MiniVmac.Functions.SettingsDescriptionFunctions.Create(configuration);
    public void InitializeOptions(MachineConfiguration configuration, IDictionary<string, string> options) => GWGUI.Emulation.Apple.Emulators.MiniVmac.Functions.ConfigurationFunctions.InitializeOptions(configuration, options);
    public override void ValidateConfiguration(MachineConfiguration configuration) => GWGUI.Emulation.Apple.Emulators.MiniVmac.Functions.ConfigurationFunctions.ValidateConfiguration(configuration);
    public bool RequiresMediaRestart(MachineConfiguration configuration, EmulationMediaSlot slot, string? path) => GWGUI.Emulation.Apple.Emulators.MiniVmac.Functions.RuntimeFunctions.RequiresMediaRestart(slot, path);
    public bool RequiresOptionRestart(MachineConfiguration configuration, string key) => GWGUI.Emulation.Apple.Emulators.MiniVmac.Functions.RuntimeFunctions.RequiresOptionRestart(key);
    public void PrepareFirmware(MachineConfiguration configuration, string systemDirectory) => GWGUI.Emulation.Apple.Emulators.MiniVmac.Functions.FirmwareFunctions.Prepare(configuration, systemDirectory);
    public async ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken) => [await (GWGUI.Emulation.Apple.Emulators.MiniVmac.Functions.FirmwareFunctions.ScanFileAsync(path, cancellationToken)).ConfigureAwait(false)];
}
