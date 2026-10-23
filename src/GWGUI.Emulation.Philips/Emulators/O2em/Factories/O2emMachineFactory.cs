using GWGUI.Emulation.Philips.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Philips.Emulators.O2em.Functions;
using Settings = GWGUI.Emulation.Philips.Emulators.O2em.Functions.SettingsDescriptionFunctions;
using GWGUI.Emulation.Philips.Emulators.O2em.Constants;

namespace GWGUI.Emulation.Philips.Emulators.O2em.Factories;

internal sealed class O2emMachineFactory() : LibretroMachineFactory(EmulatorConstants.Entry), IEmulatorSettingsAdapter, IEmulatorMediaCapabilities, IEmulatorVideoSettings
{
    public override IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => Settings.Create(configuration);
    public IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null) => Settings.GetOptions();
    public IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => GWGUI.Emulation.Philips.Emulators.O2em.Functions.StorageDescriptionFunctions.Describe(configuration);

    public (int Width, int Height)? GetOutputResolution(MachineConfiguration configuration) => GWGUI.Emulation.Philips.Emulators.O2em.Functions.VideoSettingsFunctions.GetOutputResolution(configuration);

    public override bool RequiresMediaRestart(MachineConfiguration configuration, EmulationMediaSlot slot, string? path) => true;

    public override IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => GWGUI.Emulation.Philips.Emulators.O2em.Functions.O2emRuntimeFunctions.Options(configuration);
}
