namespace GWGUI.Emulation.Atari.Common.Dictionaries;

public static class ControllerCatalog
{
    public static IReadOnlyList<PeripheralCategory> Types(MachineModel model)
    {
        if (model == MachineModel.Atari2600)
            return [PeripheralCategory.Joystick, PeripheralCategory.Paddle,
                PeripheralCategory.DrivingController, PeripheralCategory.BoosterGrip,
                PeripheralCategory.GenesisController, PeripheralCategory.Joy2BPlus,
                PeripheralCategory.None];
        if (CompatibilityCatalog.Get(model).Core == Emulator.Hatari)
            return [PeripheralCategory.Joystick, PeripheralCategory.None];
        return ClassicModelCatalog.Get(model).Ports
            .Where(port => port.Capability != ClassicPortCapability.Keyboard)
            .Select(port => port.Capability switch
            {
                ClassicPortCapability.AnalogJoystick => PeripheralCategory.AnalogJoystick,
                ClassicPortCapability.Paddle => PeripheralCategory.Paddle,
                ClassicPortCapability.DrivingController => PeripheralCategory.DrivingController,
                ClassicPortCapability.LightGun => PeripheralCategory.LightGun,
                ClassicPortCapability.NumericKeypad => model == MachineModel.Atari5200
                    ? PeripheralCategory.AnalogJoystick : PeripheralCategory.NumericKeypad,
                ClassicPortCapability.ProLineController => PeripheralCategory.ProLineController,
                ClassicPortCapability.EnhancedController => PeripheralCategory.EnhancedController,
                _ => PeripheralCategory.Joystick
            }).Append(PeripheralCategory.None).Distinct().ToArray();
    }
}
