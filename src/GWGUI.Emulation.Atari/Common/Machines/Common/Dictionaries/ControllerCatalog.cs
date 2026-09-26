namespace GWGUI.Emulation.Atari.Common.Machines.Common.Dictionaries;

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
        return HardwareModelCatalog.Get(model).Ports
            .Where(port => port.Capability != HardwarePortCapability.Keyboard)
            .Select(port => port.Capability switch
            {
                HardwarePortCapability.AnalogJoystick => PeripheralCategory.AnalogJoystick,
                HardwarePortCapability.Paddle => PeripheralCategory.Paddle,
                HardwarePortCapability.DrivingController => PeripheralCategory.DrivingController,
                HardwarePortCapability.LightGun => PeripheralCategory.LightGun,
                HardwarePortCapability.NumericKeypad => model == MachineModel.Atari5200
                    ? PeripheralCategory.AnalogJoystick : PeripheralCategory.NumericKeypad,
                HardwarePortCapability.ProLineController => PeripheralCategory.ProLineController,
                HardwarePortCapability.EnhancedController => PeripheralCategory.EnhancedController,
                _ => PeripheralCategory.Joystick
            }).Append(PeripheralCategory.None).Distinct().ToArray();
    }
}
