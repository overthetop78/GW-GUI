using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Functions;

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

    internal static IReadOnlySet<AtariPeripheralCategory> Peripherals(AtariMachineModel model)
    {
        if (model == AtariMachineModel.Atari2600)
            return new HashSet<AtariPeripheralCategory>
            {
                AtariPeripheralCategory.None, AtariPeripheralCategory.Automatic, AtariPeripheralCategory.Joystick,
                AtariPeripheralCategory.Paddle, AtariPeripheralCategory.DrivingController,
                AtariPeripheralCategory.BoosterGrip, AtariPeripheralCategory.GenesisController,
                AtariPeripheralCategory.Joy2BPlus
            };
        if (AtariConfigurationFunctions.GetFamily(model) == AtariMachineFamily.St)
            return new HashSet<AtariPeripheralCategory>
            {
                AtariPeripheralCategory.None, AtariPeripheralCategory.Automatic,
                AtariPeripheralCategory.Joystick
            };
        return new HashSet<AtariPeripheralCategory>(AtariClassicModelCatalog.Get(model).Ports
            .Where(port => port.Capability != AtariClassicPortCapability.Keyboard)
            .Select(port => FromCapability(port.Capability)).Append(AtariPeripheralCategory.None)
            .Append(AtariPeripheralCategory.Automatic));
    }

    private static AtariPeripheralCategory FromCapability(AtariClassicPortCapability capability) => capability switch
    {
        AtariClassicPortCapability.Keyboard => AtariPeripheralCategory.Keyboard,
        AtariClassicPortCapability.Joystick => AtariPeripheralCategory.Joystick,
        AtariClassicPortCapability.AnalogJoystick => AtariPeripheralCategory.AnalogJoystick,
        AtariClassicPortCapability.Paddle => AtariPeripheralCategory.Paddle,
        AtariClassicPortCapability.DrivingController => AtariPeripheralCategory.DrivingController,
        AtariClassicPortCapability.NumericKeypad => AtariPeripheralCategory.NumericKeypad,
        AtariClassicPortCapability.LightGun => AtariPeripheralCategory.LightGun,
        AtariClassicPortCapability.ProLineController => AtariPeripheralCategory.ProLineController,
        AtariClassicPortCapability.EnhancedController => AtariPeripheralCategory.EnhancedController,
        _ => throw new ArgumentOutOfRangeException(nameof(capability), capability, null)
    };
}
