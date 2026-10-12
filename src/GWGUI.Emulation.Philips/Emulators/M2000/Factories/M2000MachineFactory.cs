using GWGUI.Emulation.Philips.Emulators.M2000.Constants;

namespace GWGUI.Emulation.Philips.Emulators.M2000.Factories;

internal sealed class M2000MachineFactory() : LibretroMachineFactory(EmulatorConstants.Entry)
{
    protected override bool RequiresMediaRestart(EmulationMediaSlot slot, string? path) => true;

    public override IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => GWGUI.Emulation.Philips.Emulators.M2000.Functions.M2000RuntimeFunctions.Options(configuration);
}
