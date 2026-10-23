using GWGUI.Emulation.Mattel.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Mattel.Emulators.FreeIntv.Functions;
using GWGUI.Emulation.Mattel.Emulators.FreeIntv.Constants;
using Settings = GWGUI.Emulation.Mattel.Emulators.FreeIntv.Functions.SettingsDescriptionFunctions;

namespace GWGUI.Emulation.Mattel.Emulators.FreeIntv.Factories;

internal sealed class FreeIntvMachineFactory() : LibretroMachineFactory(EmulatorConstants.Entry), IEmulatorSettingsAdapter, IEmulatorMediaCapabilities
{
    public override IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => Settings.Create(configuration);
    public IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null) => Settings.GetOptions();

    public IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => GWGUI.Emulation.Mattel.Emulators.FreeIntv.Functions.StorageDescriptionFunctions.Describe(configuration);

    public override bool RequiresMediaRestart(MachineConfiguration configuration, EmulationMediaSlot slot, string? path) => true;
}
