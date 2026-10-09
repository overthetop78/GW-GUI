using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Constants;

namespace GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Factories;

internal sealed class PcsxRearmedMachineFactory : CoreMachineFactory
{
    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
    public override int GetControllerPortCount(MachineConfiguration configuration) =>
        configuration.Options?.GetValueOrDefault(OptionConstants.MultitapKey) switch
        {
            OptionConstants.MultitapPort1 or OptionConstants.MultitapPort2 =>
                ControllerPortConstants.SingleMultitapControllerCount,
            OptionConstants.MultitapPorts1And2 => ControllerPortConstants.MaximumControllerPortCount,
            _ => base.GetControllerPortCount(configuration)
        };
}
