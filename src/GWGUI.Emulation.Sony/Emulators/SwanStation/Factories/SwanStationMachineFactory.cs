using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Sony.Emulators.SwanStation.Constants;
using GWGUI.Emulation.Sony.Emulators.SwanStation.Functions;

namespace GWGUI.Emulation.Sony.Emulators.SwanStation.Factories;

internal sealed class SwanStationMachineFactory : CoreMachineFactory
{
    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
    public override IReadOnlyList<PeripheralCategory> GetPeripherals(MachineConfiguration configuration, int port) =>
        base.GetPeripherals(configuration, port).Where(type =>
            (type != PeripheralCategory.GunCon || port < CoreConstants.LightGunPortCount)
            && (type is not (PeripheralCategory.NeGcon or PeripheralCategory.NeGconRumble)
                || port < CoreConstants.SteeringControllerPortCount)).ToArray();
    public override int GetControllerPortCount(MachineConfiguration configuration) =>
        configuration.Options?.GetValueOrDefault(OptionConstants.ControllerPortsMultitapModeKey) switch
        {
            OptionConstants.ControllerPortsMultitapModePort1Only or
                OptionConstants.ControllerPortsMultitapModePort2Only =>
                ControllerPortConstants.SingleMultitapControllerCount,
            OptionConstants.ControllerPortsMultitapModeBothPorts =>
                ControllerPortConstants.MaximumGamepadPortCount,
            _ => base.GetControllerPortCount(configuration)
        };
    protected override MachineConfiguration Configure(MachineConfiguration configuration) => SwanStationOptionFunctions.ToNative(base.Configure(configuration));
}
