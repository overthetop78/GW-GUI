using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Sony.Emulators.Pcee2.Constants;

namespace GWGUI.Emulation.Sony.Emulators.Pcee2.Factories;

internal sealed class Pcee2MachineFactory : CoreMachineFactory
{
    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
    public override int GetControllerPortCount(MachineConfiguration configuration) =>
        configuration.Options?.GetValueOrDefault(OptionConstants.MultitapKey) switch
        {
            OptionConstants.MultitapPort1 or OptionConstants.MultitapPort2 =>
                ControllerPortConstants.SingleMultitapControllerCount,
            OptionConstants.MultitapBoth => ControllerPortConstants.MaximumControllerPortCount,
            _ => base.GetControllerPortCount(configuration)
        };
}
