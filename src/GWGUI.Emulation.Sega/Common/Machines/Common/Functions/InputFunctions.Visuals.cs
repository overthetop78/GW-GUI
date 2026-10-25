namespace GWGUI.Emulation.Sega.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    private static IReadOnlyList<string>? CompatibleVisualIds(PeripheralCategory type) =>
        type switch
        {
            PeripheralCategory.Joypad => [EmulationControllerVisualIds.QuickShot,
                EmulationControllerVisualIds.CompetitionPro5000,
                EmulationControllerVisualIds.ZipstikSuperPro],
            PeripheralCategory.SegaSg1000Joystick => [EmulationControllerVisualIds.SegaSg1000Joystick],
            PeripheralCategory.SegaSg1000IiJoypad => [EmulationControllerVisualIds.SegaSg1000Ii],
            PeripheralCategory.SegaMasterSystemController => [EmulationControllerVisualIds.MasterSystem],
            PeripheralCategory.SegaControlStick => [EmulationControllerVisualIds.SegaControlStick],
            PeripheralCategory.SegaGameGearController => [EmulationControllerVisualIds.GameGearControls],
            PeripheralCategory.SegaMegaDriveThreeButton => [EmulationControllerVisualIds.MegaDrive3],
            PeripheralCategory.SegaMegaDriveSixButton => [EmulationControllerVisualIds.MegaDrive6],
            PeripheralCategory.SegaSaturnController => [EmulationControllerVisualIds.Saturn],
            PeripheralCategory.SegaSaturnThreeDControlPad => [EmulationControllerVisualIds.Saturn3D],
            PeripheralCategory.SegaSaturnVirtuaStick =>
                [EmulationControllerVisualIds.SegaSaturnVirtuaStickHss0104,
                    EmulationControllerVisualIds.SegaSaturnVirtuaStickHss0136],
            PeripheralCategory.SegaDreamcastController => [EmulationControllerVisualIds.Dreamcast],
            PeripheralCategory.SegaArcadePowerStick => [EmulationControllerVisualIds.SegaArcadePowerStick3],
            PeripheralCategory.SegaArcadePowerStickSixButton => [EmulationControllerVisualIds.SegaArcadePowerStick6],
            PeripheralCategory.SegaHandleController or PeripheralCategory.SegaSaturnMissionStick =>
                [EmulationControllerVisualIds.FlightStick],
            PeripheralCategory.SegaSaturnArcadeRacer => [EmulationControllerVisualIds.RacingWheel],
            _ => null
        };

    private static string? DefaultVisualId(PeripheralCategory type) =>
        type switch
        {
            PeripheralCategory.Joypad => EmulationControllerVisualIds.QuickShot,
            PeripheralCategory.SegaSg1000Joystick => EmulationControllerVisualIds.SegaSg1000Joystick,
            PeripheralCategory.SegaSg1000IiJoypad => EmulationControllerVisualIds.SegaSg1000Ii,
            PeripheralCategory.SegaMasterSystemController => EmulationControllerVisualIds.MasterSystem,
            PeripheralCategory.SegaControlStick => EmulationControllerVisualIds.SegaControlStick,
            PeripheralCategory.SegaGameGearController => EmulationControllerVisualIds.GameGearControls,
            PeripheralCategory.SegaMegaDriveThreeButton => EmulationControllerVisualIds.MegaDrive3,
            PeripheralCategory.SegaMegaDriveSixButton => EmulationControllerVisualIds.MegaDrive6,
            PeripheralCategory.SegaSaturnController => EmulationControllerVisualIds.Saturn,
            PeripheralCategory.SegaSaturnThreeDControlPad => EmulationControllerVisualIds.Saturn3D,
            PeripheralCategory.SegaSaturnVirtuaStick => EmulationControllerVisualIds.SegaSaturnVirtuaStickHss0104,
            PeripheralCategory.SegaDreamcastController => EmulationControllerVisualIds.Dreamcast,
            PeripheralCategory.SegaArcadePowerStick => EmulationControllerVisualIds.SegaArcadePowerStick3,
            PeripheralCategory.SegaArcadePowerStickSixButton => EmulationControllerVisualIds.SegaArcadePowerStick6,
            PeripheralCategory.SegaHandleController or PeripheralCategory.SegaSaturnMissionStick =>
                EmulationControllerVisualIds.FlightStick,
            PeripheralCategory.SegaSaturnArcadeRacer => EmulationControllerVisualIds.RacingWheel,
            _ => null
        };

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string>?
        VisualCommandIds(PeripheralCategory type) => type is PeripheralCategory.Joypad
            or PeripheralCategory.SegaSg1000Joystick
            or PeripheralCategory.SegaSg1000IiJoypad
            or PeripheralCategory.SegaMasterSystemController
            or PeripheralCategory.SegaControlStick
            or PeripheralCategory.SegaGameGearController
            or PeripheralCategory.SegaMegaDriveThreeButton or PeripheralCategory.SegaMegaDriveSixButton
            or PeripheralCategory.SegaSaturnController or PeripheralCategory.SegaSaturnThreeDControlPad
            or PeripheralCategory.SegaSaturnVirtuaStick
            or PeripheralCategory.SegaDreamcastController
            or PeripheralCategory.SegaArcadePowerStick or PeripheralCategory.SegaArcadePowerStickSixButton
            or PeripheralCategory.SegaDreamcastArcadeStick or PeripheralCategory.SegaDreamcastTwinStick
            or PeripheralCategory.SegaHandleController or PeripheralCategory.SegaSaturnMissionStick
            or PeripheralCategory.SegaSaturnArcadeRacer
        ? BuildVisualCommandIds(type) : null;

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string>
        BuildVisualCommandIds(PeripheralCategory type)
    {
        var commands = new Dictionary<EmulationControllerVisualControl, string>
        {
            [EmulationControllerVisualControl.DirectionUp] = EmulationControllerCommandIds.Up,
            [EmulationControllerVisualControl.DirectionDown] = EmulationControllerCommandIds.Down,
            [EmulationControllerVisualControl.DirectionLeft] = EmulationControllerCommandIds.Left,
            [EmulationControllerVisualControl.DirectionRight] = EmulationControllerCommandIds.Right
        };
        if (type == PeripheralCategory.SegaGameGearController)
        {
            commands[EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.B;
            commands[EmulationControllerVisualControl.SecondaryAction] = EmulationControllerCommandIds.A;
            commands[EmulationControllerVisualControl.Start] = EmulationControllerCommandIds.Start;
        }
        else if (type == PeripheralCategory.SegaDreamcastController)
        {
            commands[EmulationControllerVisualControl.FaceA] = EmulationControllerCommandIds.B;
            commands[EmulationControllerVisualControl.FaceB] = EmulationControllerCommandIds.A;
            commands[EmulationControllerVisualControl.FaceX] = EmulationControllerCommandIds.Y;
            commands[EmulationControllerVisualControl.FaceY] = EmulationControllerCommandIds.X;
            commands[EmulationControllerVisualControl.Start] = EmulationControllerCommandIds.Start;
            commands[EmulationControllerVisualControl.LeftTrigger] = EmulationControllerCommandIds.L2;
            commands[EmulationControllerVisualControl.RightTrigger] = EmulationControllerCommandIds.R2;
        }
        else if (type is PeripheralCategory.SegaSaturnController
            or PeripheralCategory.SegaSaturnThreeDControlPad
            or PeripheralCategory.SegaSaturnVirtuaStick)
        {
            commands[EmulationControllerVisualControl.FaceA] = EmulationControllerCommandIds.B;
            commands[EmulationControllerVisualControl.FaceB] = EmulationControllerCommandIds.A;
            commands[EmulationControllerVisualControl.FaceC] = EmulationControllerCommandIds.R;
            commands[EmulationControllerVisualControl.FaceX] = EmulationControllerCommandIds.Y;
            commands[EmulationControllerVisualControl.FaceY] = EmulationControllerCommandIds.X;
            commands[EmulationControllerVisualControl.FaceZ] = EmulationControllerCommandIds.L;
            commands[EmulationControllerVisualControl.Start] = EmulationControllerCommandIds.Start;
            commands[EmulationControllerVisualControl.LeftTrigger] = EmulationControllerCommandIds.L2;
            commands[EmulationControllerVisualControl.RightTrigger] = EmulationControllerCommandIds.R2;
        }
        else if (type is PeripheralCategory.SegaMegaDriveThreeButton
            or PeripheralCategory.SegaMegaDriveSixButton
            or PeripheralCategory.SegaArcadePowerStick
            or PeripheralCategory.SegaArcadePowerStickSixButton)
        {
            commands[EmulationControllerVisualControl.FaceA] = EmulationControllerCommandIds.Y;
            commands[EmulationControllerVisualControl.FaceB] = EmulationControllerCommandIds.B;
            commands[EmulationControllerVisualControl.FaceC] = EmulationControllerCommandIds.A;
            commands[EmulationControllerVisualControl.Start] = EmulationControllerCommandIds.Start;
            if (type is PeripheralCategory.SegaMegaDriveSixButton
                or PeripheralCategory.SegaArcadePowerStickSixButton)
            {
                commands[EmulationControllerVisualControl.FaceX] = EmulationControllerCommandIds.L;
                commands[EmulationControllerVisualControl.FaceY] = EmulationControllerCommandIds.X;
                commands[EmulationControllerVisualControl.FaceZ] = EmulationControllerCommandIds.R;
                commands[EmulationControllerVisualControl.Option] = EmulationControllerCommandIds.Select;
            }
        }
        else
        {
            commands[EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.B;
            commands[EmulationControllerVisualControl.SecondaryAction] = EmulationControllerCommandIds.A;
        }
        return commands;
    }

    private static string? NormalizeVisualId(PeripheralCategory type, string? visualId)
    {
        var compatible = CompatibleVisualIds(type);
        return compatible is null ? null : compatible.Contains(visualId ?? string.Empty, StringComparer.Ordinal) ? visualId : DefaultVisualId(type);
    }

    private static string ControllerResourceKey(PeripheralCategory type) => type switch
    {
        PeripheralCategory.Joypad => InputSettingsFunctionsConstants.ResourceControllerJoypad,
        PeripheralCategory.Automatic => InputSettingsFunctionsConstants.ResourceControllerAutomatic,
        PeripheralCategory.None => InputSettingsFunctionsConstants.ResourceControllerNone,
        _ => $"Emulation.Sega.Controller.{type}"
    };
}
