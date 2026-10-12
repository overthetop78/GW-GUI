namespace GWGUI.Emulation.Sharp.Common.Machines.Common.Dictionaries;

internal static class ControllerVisualDictionary
{
    internal static IReadOnlyList<string> VisualIds(PeripheralCategory peripheral) => peripheral switch
    {
        PeripheralCategory.None => [],
        PeripheralCategory.Joystick => [EmulationControllerVisualIds.SegaControlStick, EmulationControllerVisualIds.MasterSystem, EmulationControllerVisualIds.SegaSg1000Joystick, EmulationControllerVisualIds.SegaSg1000Ii],
        PeripheralCategory.X1Joystick or PeripheralCategory.CpsfSfc => [EmulationControllerVisualIds.NintendoSuperFamicomPad, EmulationControllerVisualIds.NintendoSuperNesPad],
        PeripheralCategory.CpsfMd => [EmulationControllerVisualIds.MegaDrive6, EmulationControllerVisualIds.SegaArcadePowerStick6],
        PeripheralCategory.CyberstickDigital or PeripheralCategory.CyberstickAnalog => [EmulationControllerVisualIds.SharpCyberstick],
        _ => throw new ArgumentOutOfRangeException(nameof(peripheral))};
    internal static string? DefaultVisualId(PeripheralCategory peripheral) => VisualIds(peripheral).FirstOrDefault();

    internal static IReadOnlyDictionary<EmulationControllerVisualControl, string> Commands(PeripheralCategory peripheral)
    {
        var commands = new Dictionary<EmulationControllerVisualControl, string>();
        if (peripheral == PeripheralCategory.None) return commands;
        commands[EmulationControllerVisualControl.DirectionUp] = nameof(ControllerCommand.Up);
        commands[EmulationControllerVisualControl.DirectionDown] = nameof(ControllerCommand.Down);
        commands[EmulationControllerVisualControl.DirectionLeft] = nameof(ControllerCommand.Left);
        commands[EmulationControllerVisualControl.DirectionRight] = nameof(ControllerCommand.Right);
        if (peripheral == PeripheralCategory.CpsfMd)
        {
            commands[EmulationControllerVisualControl.FaceA] = nameof(ControllerCommand.Fire1);
            commands[EmulationControllerVisualControl.FaceB] = nameof(ControllerCommand.Fire2);
            commands[EmulationControllerVisualControl.FaceX] = nameof(ControllerCommand.Fire3);
            commands[EmulationControllerVisualControl.FaceY] = nameof(ControllerCommand.Fire4);
            commands[EmulationControllerVisualControl.FaceZ] = nameof(ControllerCommand.Fire5);
            commands[EmulationControllerVisualControl.FaceC] = nameof(ControllerCommand.Fire6);
        }
        else if (peripheral is PeripheralCategory.X1Joystick or PeripheralCategory.CpsfSfc)
        {
            commands[EmulationControllerVisualControl.SecondaryAction] = nameof(ControllerCommand.Fire1);
            commands[EmulationControllerVisualControl.PrimaryAction] = nameof(ControllerCommand.Fire2);
            commands[EmulationControllerVisualControl.QuaternaryAction] = nameof(ControllerCommand.Fire3);
            commands[EmulationControllerVisualControl.TertiaryAction] = nameof(ControllerCommand.Fire4);
            if (peripheral == PeripheralCategory.X1Joystick) return commands;
            commands[EmulationControllerVisualControl.LeftShoulder] = nameof(ControllerCommand.Fire5);
            commands[EmulationControllerVisualControl.RightShoulder] = nameof(ControllerCommand.Fire6);
        }
        else
        {
            commands[EmulationControllerVisualControl.PrimaryAction] = nameof(ControllerCommand.Fire1);
            commands[EmulationControllerVisualControl.SecondaryAction] = nameof(ControllerCommand.Fire2);
            if (peripheral == PeripheralCategory.Joystick) return commands;
            commands[EmulationControllerVisualControl.TertiaryAction] = nameof(ControllerCommand.Fire3);
            commands[EmulationControllerVisualControl.QuaternaryAction] = nameof(ControllerCommand.Fire4);
            commands[EmulationControllerVisualControl.LeftShoulder] = nameof(ControllerCommand.Fire5);
            commands[EmulationControllerVisualControl.RightShoulder] = nameof(ControllerCommand.Fire6);
        }
        commands[EmulationControllerVisualControl.Start] = nameof(ControllerCommand.Fire7);
        commands[EmulationControllerVisualControl.Option] = nameof(ControllerCommand.Fire8);
        if (peripheral == PeripheralCategory.CyberstickAnalog)
        {
            commands[EmulationControllerVisualControl.RightStickUp] = nameof(ControllerCommand.ThrottleIncrease);
            commands[EmulationControllerVisualControl.RightStickDown] = nameof(ControllerCommand.ThrottleDecrease);
        }
        return commands;
    }
}
