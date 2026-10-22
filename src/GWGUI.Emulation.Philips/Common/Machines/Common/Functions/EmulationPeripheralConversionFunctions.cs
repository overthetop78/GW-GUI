namespace GWGUI.Emulation.Philips.Common.Machines.Common.Functions;

internal static class EmulationPeripheralConversionFunctions
{
    internal static PeripheralCategory ToModule(EmulationPeripheralCategory peripheral, MachineModel model) => peripheral switch
    {
        EmulationPeripheralCategory.Automatic or EmulationPeripheralCategory.RetroPad => InputSettingsFunctions.DefaultPeripheral(model),
        EmulationPeripheralCategory.Joystick when ConfigurationFunctions.Family(model) == MachineFamily.Videopac => PeripheralCategory.Joystick,
        EmulationPeripheralCategory.Joystick when ConfigurationFunctions.Family(model) == MachineFamily.Cdi => PeripheralCategory.CdiGamepad,
        EmulationPeripheralCategory.None => PeripheralCategory.None,
        EmulationPeripheralCategory.Mouse when ConfigurationFunctions.Family(model) == MachineFamily.Cdi => PeripheralCategory.Mouse,
        EmulationPeripheralCategory.LightGun when ConfigurationFunctions.Family(model) == MachineFamily.Cdi => PeripheralCategory.LightGun,
        _ => throw new ArgumentOutOfRangeException(nameof(peripheral))
    };
}
