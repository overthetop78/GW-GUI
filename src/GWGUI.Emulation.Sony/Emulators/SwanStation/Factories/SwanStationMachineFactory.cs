using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Sony.Emulators.SwanStation.Constants;
using GWGUI.Emulation.Sony.Emulators.SwanStation.Functions;

namespace GWGUI.Emulation.Sony.Emulators.SwanStation.Factories;

internal sealed class SwanStationMachineFactory : CoreMachineFactory
{
    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
    public override IReadOnlyList<ControllerType> GetControllerTypes(MachineConfiguration configuration, int port) =>
        base.GetControllerTypes(configuration, port).Where(type =>
            (type != ControllerType.GunCon || port < CoreConstants.LightGunPortCount)
            && (type is not (ControllerType.NeGcon or ControllerType.NeGconRumble)
                || port < CoreConstants.SteeringControllerPortCount)).ToArray();
    public override int GetControllerPortCount(MachineConfiguration configuration) =>
        configuration.Options?.GetValueOrDefault(OptionConstants.ControllerPortsMultitapModeKey) switch
        {
            OptionConstants.ControllerPortsMultitapModePort1Only or
                OptionConstants.ControllerPortsMultitapModePort2Only =>
                ControllerPortConstants.SingleMultitapControllerCount,
            OptionConstants.ControllerPortsMultitapModeBothPorts =>
                ControllerPortConstants.MaximumControllerPortCount,
            _ => base.GetControllerPortCount(configuration)
        };
    protected override MachineConfiguration Configure(MachineConfiguration configuration) =>
        SwanStationOptionFunctions.ToNative(base.Configure(configuration));
}
