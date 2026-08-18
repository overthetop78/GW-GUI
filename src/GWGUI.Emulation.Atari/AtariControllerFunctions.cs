using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari;

internal static class AtariControllerFunctions
{
    internal static short ApplyDeadZone(short value, int percent)
    {
        var threshold = AtariControllerConstants.MaximumAxisMagnitude * percent /
                        AtariControllerConstants.PercentageDivisor;
        return Math.Abs((int)value) <= threshold ? AtariControllerConstants.NeutralAxis : value;
    }

    internal static EmulationInputSnapshot ApplyDeadZones(EmulationInputSnapshot snapshot,
        IReadOnlyList<AtariControllerBinding>? bindings)
    {
        if (bindings is null || bindings.Count == AtariConstants.EmptyCollectionCount) return snapshot;
        var controllers = snapshot.Controllers.ToArray();
        foreach (var binding in bindings)
        {
            if (binding.Port < AtariConstants.MinimumControllerPort || binding.Port >= controllers.Length) continue;
            var controller = controllers[binding.Port];
            controllers[binding.Port] = controller with
            {
                LeftX = ApplyDeadZone(controller.LeftX, binding.DeadZonePercent),
                LeftY = ApplyDeadZone(controller.LeftY, binding.DeadZonePercent),
                RightX = ApplyDeadZone(controller.RightX, binding.DeadZonePercent),
                RightY = ApplyDeadZone(controller.RightY, binding.DeadZonePercent),
                LeftTrigger = ApplyDeadZone(controller.LeftTrigger, binding.DeadZonePercent),
                RightTrigger = ApplyDeadZone(controller.RightTrigger, binding.DeadZonePercent)
            };
        }
        return snapshot with { Controllers = controllers };
    }

    internal static IReadOnlySet<AtariPeripheralKind> Peripherals(AtariMachineModel model)
    {
        if (model == AtariMachineModel.Atari2600)
            return new HashSet<AtariPeripheralKind>
            {
                AtariPeripheralKind.None, AtariPeripheralKind.Automatic, AtariPeripheralKind.Joystick,
                AtariPeripheralKind.Paddle, AtariPeripheralKind.DrivingController,
                AtariPeripheralKind.BoosterGrip, AtariPeripheralKind.GenesisController,
                AtariPeripheralKind.Joy2BPlus
            };
        if (AtariConfigurationFunctions.GetFamily(model) == AtariMachineFamily.St)
            return new HashSet<AtariPeripheralKind>
            {
                AtariPeripheralKind.None, AtariPeripheralKind.Automatic,
                AtariPeripheralKind.Joystick
            };
        return new HashSet<AtariPeripheralKind>(AtariClassicModelCatalog.Get(model).Ports
            .Where(port => port.Capability != AtariClassicPortCapability.Keyboard)
            .Select(port => FromCapability(port.Capability)).Append(AtariPeripheralKind.None)
            .Append(AtariPeripheralKind.Automatic));
    }

    private static AtariPeripheralKind FromCapability(AtariClassicPortCapability capability) => capability switch
    {
        AtariClassicPortCapability.Keyboard => AtariPeripheralKind.Keyboard,
        AtariClassicPortCapability.Joystick => AtariPeripheralKind.Joystick,
        AtariClassicPortCapability.AnalogJoystick => AtariPeripheralKind.AnalogJoystick,
        AtariClassicPortCapability.Paddle => AtariPeripheralKind.Paddle,
        AtariClassicPortCapability.DrivingController => AtariPeripheralKind.DrivingController,
        AtariClassicPortCapability.NumericKeypad => AtariPeripheralKind.NumericKeypad,
        AtariClassicPortCapability.LightGun => AtariPeripheralKind.LightGun,
        AtariClassicPortCapability.ProLineController => AtariPeripheralKind.ProLineController,
        AtariClassicPortCapability.EnhancedController => AtariPeripheralKind.EnhancedController,
        _ => throw new ArgumentOutOfRangeException(nameof(capability), capability, null)
    };
}
