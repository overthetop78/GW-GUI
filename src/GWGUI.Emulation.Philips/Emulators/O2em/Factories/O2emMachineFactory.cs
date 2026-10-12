using GWGUI.Emulation.Philips.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Philips.Emulators.O2em.Functions;
using Settings = GWGUI.Emulation.Philips.Emulators.O2em.Functions.SettingsDescriptionFunctions;
using GWGUI.Emulation.Philips.Emulators.O2em.Constants;

namespace GWGUI.Emulation.Philips.Emulators.O2em.Factories;

internal sealed class O2emMachineFactory() : LibretroMachineFactory(EmulatorConstants.Entry), IEmulatorSettings, IEmulatorStorageSettings, IEmulatorVideoSettings
{
    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration) => Settings.Create(configuration);
    public IReadOnlyList<CoreOption> GetSettingsOptions() => Settings.GetOptions();
    public IReadOnlyList<EmulationMediaDevice> DescribeStorage(MachineConfiguration configuration) => GWGUI.Emulation.Philips.Emulators.O2em.Functions.StorageDescriptionFunctions.Describe(configuration);

    public (int Width, int Height)? GetOutputResolution(MachineConfiguration configuration) => GWGUI.Emulation.Philips.Emulators.O2em.Functions.VideoSettingsFunctions.GetOutputResolution(configuration);

    protected override bool RequiresMediaRestart(EmulationMediaSlot slot, string? path) => true;

    public override IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => GWGUI.Emulation.Philips.Emulators.O2em.Functions.O2emRuntimeFunctions.Options(configuration);
}
