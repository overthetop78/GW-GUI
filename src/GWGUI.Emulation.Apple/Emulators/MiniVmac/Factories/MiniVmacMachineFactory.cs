using GWGUI.Emulation.Apple.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Apple.Emulators.MiniVmac.Constants;

namespace GWGUI.Emulation.Apple.Emulators.MiniVmac.Factories;

internal sealed class MiniVmacMachineFactory() : MachineFactory(EmulatorConstants.Entry), IEmulatorSettings, IEmulatorConfiguration, IEmulatorRuntimePolicy, IEmulatorFirmwareAdapter
{
    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration) => GWGUI.Emulation.Apple.Emulators.MiniVmac.Functions.SettingsDescriptionFunctions.Create(configuration);
    public void InitializeOptions(MachineConfiguration configuration, IDictionary<string, string> options) => GWGUI.Emulation.Apple.Emulators.MiniVmac.Functions.ConfigurationFunctions.InitializeOptions(configuration, options);
    public void ValidateConfiguration(MachineConfiguration configuration) => GWGUI.Emulation.Apple.Emulators.MiniVmac.Functions.ConfigurationFunctions.ValidateConfiguration(configuration);
    public bool RequiresMediaRestart(EmulationMediaSlot slot, string? path) => GWGUI.Emulation.Apple.Emulators.MiniVmac.Functions.RuntimeFunctions.RequiresMediaRestart(slot, path);
    public bool RequiresOptionRestart(string key) => GWGUI.Emulation.Apple.Emulators.MiniVmac.Functions.RuntimeFunctions.RequiresOptionRestart(key);
    public void PrepareFirmware(MachineConfiguration configuration, string systemDirectory) => GWGUI.Emulation.Apple.Emulators.MiniVmac.Functions.FirmwareFunctions.Prepare(configuration, systemDirectory);
    public ValueTask<EmulationFirmwareCandidate> ScanFirmwareFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken) => GWGUI.Emulation.Apple.Emulators.MiniVmac.Functions.FirmwareFunctions.ScanFileAsync(path, cancellationToken);
}
