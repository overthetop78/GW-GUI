namespace GWGUI.Emulation.Sharp.Common.Machines.Common.Dictionaries;

internal static class ControllerCatalog
{
    internal static IReadOnlyList<PeripheralCategory> Peripherals(MachineModel model) => ConfigurationFunctions.Family(model) == MachineFamily.X1
        ? [PeripheralCategory.None, PeripheralCategory.X1Joystick]
        : [PeripheralCategory.None, PeripheralCategory.Joystick, PeripheralCategory.CpsfMd, PeripheralCategory.CpsfSfc, PeripheralCategory.CyberstickDigital, PeripheralCategory.CyberstickAnalog];

    private static readonly IReadOnlyList<ControllerCommand> Directions = Array.AsReadOnly(new[] { ControllerCommand.Up, ControllerCommand.Down, ControllerCommand.Left, ControllerCommand.Right });
    private static readonly IReadOnlyList<ControllerCommand> Buttons = Array.AsReadOnly(new[] { ControllerCommand.Fire1, ControllerCommand.Fire2, ControllerCommand.Fire3, ControllerCommand.Fire4, ControllerCommand.Fire5, ControllerCommand.Fire6, ControllerCommand.Fire7, ControllerCommand.Fire8 });
    internal static IReadOnlyList<InputActionDefinition> Actions(MachineModel model, PeripheralCategory peripheral)
    {
        var family = ConfigurationFunctions.Family(model);
        if (peripheral == PeripheralCategory.None)
            return[];
        if (family == MachineFamily.X1 && peripheral != PeripheralCategory.X1Joystick || family == MachineFamily.X68000 && peripheral == PeripheralCategory.X1Joystick)
            throw new ArgumentOutOfRangeException(nameof(peripheral));
        var count = peripheral switch
        {
            PeripheralCategory.Joystick => ControllerConstants.JoystickButtonCount,
            PeripheralCategory.X1Joystick => ControllerConstants.X1JoystickButtonCount,
            PeripheralCategory.CpsfMd or PeripheralCategory.CpsfSfc or PeripheralCategory.CyberstickDigital or PeripheralCategory.CyberstickAnalog => ControllerConstants.ExtendedJoystickButtonCount,
            _ => throw new ArgumentOutOfRangeException(nameof(peripheral))
        };
        var commands = Directions.Concat(Buttons.Take(count));
        if (peripheral == PeripheralCategory.CyberstickAnalog)
            commands = commands.Concat(new[] { ControllerCommand.ThrottleIncrease, ControllerCommand.ThrottleDecrease });
        return commands.Select(command => new InputActionDefinition(command, peripheral == PeripheralCategory.CyberstickAnalog)).ToArray();
    }
}