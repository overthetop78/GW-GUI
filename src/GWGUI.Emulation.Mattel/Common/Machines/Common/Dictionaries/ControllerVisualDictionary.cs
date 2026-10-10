namespace GWGUI.Emulation.Mattel.Common.Machines.Common.Dictionaries;

internal static class ControllerVisualDictionary
{
    internal static string? VisualId(PeripheralCategory peripheral) =>
        peripheral == PeripheralCategory.Intellivision ? EmulationControllerVisualIds.MattelIntellivisionController : null;

    internal static IReadOnlyDictionary<EmulationControllerVisualControl, string> Commands(PeripheralCategory peripheral)
    {
        if (peripheral != PeripheralCategory.Intellivision) throw new ArgumentOutOfRangeException(nameof(peripheral));
        var commands = new Dictionary<EmulationControllerVisualControl, string>
        {
            [EmulationControllerVisualControl.DirectionUp] = nameof(ControllerCommand.Up),
            [EmulationControllerVisualControl.DirectionDown] = nameof(ControllerCommand.Down),
            [EmulationControllerVisualControl.DirectionLeft] = nameof(ControllerCommand.Left),
            [EmulationControllerVisualControl.DirectionRight] = nameof(ControllerCommand.Right),
            [EmulationControllerVisualControl.PrimaryAction] = nameof(ControllerCommand.Top),
            [EmulationControllerVisualControl.SecondaryAction] = nameof(ControllerCommand.BottomLeft),
            [EmulationControllerVisualControl.TertiaryAction] = nameof(ControllerCommand.BottomRight),
            [EmulationControllerVisualControl.KeyStar] = nameof(ControllerCommand.Clear),
            [EmulationControllerVisualControl.KeyHash] = nameof(ControllerCommand.Enter),
            [EmulationControllerVisualControl.Key0] = nameof(ControllerCommand.Key0)
        };
        for (var number = 1; number <= 9; number++)
            commands[Enum.Parse<EmulationControllerVisualControl>("Key" + number)] =
                Enum.Parse<ControllerCommand>("Key" + number).ToString();
        return commands;
    }
}
