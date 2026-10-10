using GWGUI.Emulation.Philips.Emulators.O2em.Constants;

namespace GWGUI.Emulation.Philips.Emulators.O2em.Factories;

internal sealed class O2emMachineFactory() : LibretroMachineFactory(EmulatorConstants.Entry)
{
    protected override bool RequiresMediaRestart(EmulationMediaSlot slot, string? path) => true;

    public override IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) =>
        GWGUI.Emulation.Philips.Emulators.O2em.Functions.O2emRuntimeFunctions.Options(configuration);
}
