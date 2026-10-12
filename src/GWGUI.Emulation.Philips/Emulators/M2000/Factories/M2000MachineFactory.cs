using GWGUI.Emulation.Philips.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Philips.Emulators.M2000.Functions;
using Settings = GWGUI.Emulation.Philips.Emulators.M2000.Functions.SettingsDescriptionFunctions;
using GWGUI.Emulation.Philips.Emulators.M2000.Constants;

namespace GWGUI.Emulation.Philips.Emulators.M2000.Factories;

internal sealed class M2000MachineFactory() : LibretroMachineFactory(EmulatorConstants.Entry), IEmulatorSettings, IEmulatorStorageSettings
{
    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration) => Settings.Create(configuration);
    public IReadOnlyList<CoreOption> GetSettingsOptions() => Settings.GetOptions();
    public IReadOnlyList<EmulationMediaDevice> DescribeStorage(MachineConfiguration configuration) => GWGUI.Emulation.Philips.Emulators.M2000.Functions.StorageDescriptionFunctions.Describe(configuration);

    protected override bool RequiresMediaRestart(EmulationMediaSlot slot, string? path) => true;

    public override IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => GWGUI.Emulation.Philips.Emulators.M2000.Functions.M2000RuntimeFunctions.Options(configuration);
}
