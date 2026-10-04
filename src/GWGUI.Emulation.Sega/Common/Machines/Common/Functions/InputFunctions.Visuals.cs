namespace GWGUI.Emulation.Sega.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    private static IReadOnlyList<string>? CompatibleVisualIds(ControllerType type) =>
        type switch
        {
            ControllerType.Joypad => [EmulationControllerVisualIds.QuickShot,
                EmulationControllerVisualIds.CompetitionPro5000,
                EmulationControllerVisualIds.ZipstikSuperPro],
            ControllerType.SegaMasterSystemController => [EmulationControllerVisualIds.MasterSystem],
            ControllerType.SegaControlStick => [EmulationControllerVisualIds.SegaControlStick],
            ControllerType.SegaGameGearController => [EmulationControllerVisualIds.GameGearControls],
            ControllerType.SegaMegaDriveThreeButton => [EmulationControllerVisualIds.MegaDrive3],
            ControllerType.SegaMegaDriveSixButton => [EmulationControllerVisualIds.MegaDrive6],
            ControllerType.SegaSaturnController => [EmulationControllerVisualIds.Saturn],
            ControllerType.SegaSaturnThreeDControlPad => [EmulationControllerVisualIds.Saturn3D],
            ControllerType.SegaDreamcastController => [EmulationControllerVisualIds.Dreamcast],
            ControllerType.SegaArcadePowerStick => [EmulationControllerVisualIds.SegaArcadePowerStick3],
            ControllerType.SegaArcadePowerStickSixButton => [EmulationControllerVisualIds.SegaArcadePowerStick6],
            ControllerType.SegaDreamcastArcadeStick or ControllerType.SegaDreamcastTwinStick =>
                [EmulationControllerVisualIds.ArcadeStick],
            ControllerType.SegaHandleController or ControllerType.SegaSaturnMissionStick =>
                [EmulationControllerVisualIds.FlightStick],
            ControllerType.SegaSaturnArcadeRacer => [EmulationControllerVisualIds.RacingWheel],
            _ => null
        };

    private static string? DefaultVisualId(ControllerType type) =>
        type switch
        {
            ControllerType.Joypad => EmulationControllerVisualIds.QuickShot,
            ControllerType.SegaMasterSystemController => EmulationControllerVisualIds.MasterSystem,
            ControllerType.SegaControlStick => EmulationControllerVisualIds.SegaControlStick,
            ControllerType.SegaGameGearController => EmulationControllerVisualIds.GameGearControls,
            ControllerType.SegaMegaDriveThreeButton => EmulationControllerVisualIds.MegaDrive3,
            ControllerType.SegaMegaDriveSixButton => EmulationControllerVisualIds.MegaDrive6,
            ControllerType.SegaSaturnController => EmulationControllerVisualIds.Saturn,
            ControllerType.SegaSaturnThreeDControlPad => EmulationControllerVisualIds.Saturn3D,
            ControllerType.SegaDreamcastController => EmulationControllerVisualIds.Dreamcast,
            ControllerType.SegaArcadePowerStick => EmulationControllerVisualIds.SegaArcadePowerStick3,
            ControllerType.SegaArcadePowerStickSixButton => EmulationControllerVisualIds.SegaArcadePowerStick6,
            ControllerType.SegaDreamcastArcadeStick or ControllerType.SegaDreamcastTwinStick =>
                EmulationControllerVisualIds.ArcadeStick,
            ControllerType.SegaHandleController or ControllerType.SegaSaturnMissionStick =>
                EmulationControllerVisualIds.FlightStick,
            ControllerType.SegaSaturnArcadeRacer => EmulationControllerVisualIds.RacingWheel,
            _ => null
        };

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string>?
        VisualCommandIds(ControllerType type) => type is ControllerType.Joypad
            or ControllerType.SegaMasterSystemController
            or ControllerType.SegaControlStick
            or ControllerType.SegaGameGearController
            or ControllerType.SegaMegaDriveThreeButton or ControllerType.SegaMegaDriveSixButton
            or ControllerType.SegaSaturnController or ControllerType.SegaSaturnThreeDControlPad
            or ControllerType.SegaDreamcastController
            or ControllerType.SegaArcadePowerStick or ControllerType.SegaArcadePowerStickSixButton
            or ControllerType.SegaDreamcastArcadeStick or ControllerType.SegaDreamcastTwinStick
            or ControllerType.SegaHandleController or ControllerType.SegaSaturnMissionStick
            or ControllerType.SegaSaturnArcadeRacer
        ? BuildVisualCommandIds(type) : null;

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string>
        BuildVisualCommandIds(ControllerType type)
    {
        var commands = new Dictionary<EmulationControllerVisualControl, string>
        {
            [EmulationControllerVisualControl.DirectionUp] = EmulationControllerCommandIds.Up,
            [EmulationControllerVisualControl.DirectionDown] = EmulationControllerCommandIds.Down,
            [EmulationControllerVisualControl.DirectionLeft] = EmulationControllerCommandIds.Left,
            [EmulationControllerVisualControl.DirectionRight] = EmulationControllerCommandIds.Right
        };
        if (type == ControllerType.SegaGameGearController)
        {
            commands[EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.B;
            commands[EmulationControllerVisualControl.SecondaryAction] = EmulationControllerCommandIds.A;
            commands[EmulationControllerVisualControl.Start] = EmulationControllerCommandIds.Start;
        }
        else if (type == ControllerType.SegaDreamcastController)
        {
            commands[EmulationControllerVisualControl.FaceA] = EmulationControllerCommandIds.B;
            commands[EmulationControllerVisualControl.FaceB] = EmulationControllerCommandIds.A;
            commands[EmulationControllerVisualControl.FaceX] = EmulationControllerCommandIds.Y;
            commands[EmulationControllerVisualControl.FaceY] = EmulationControllerCommandIds.X;
            commands[EmulationControllerVisualControl.Start] = EmulationControllerCommandIds.Start;
            commands[EmulationControllerVisualControl.LeftTrigger] = EmulationControllerCommandIds.L2;
            commands[EmulationControllerVisualControl.RightTrigger] = EmulationControllerCommandIds.R2;
        }
        else if (type is ControllerType.SegaSaturnController
            or ControllerType.SegaSaturnThreeDControlPad)
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
        else if (type is ControllerType.SegaMegaDriveThreeButton
            or ControllerType.SegaMegaDriveSixButton
            or ControllerType.SegaArcadePowerStick
            or ControllerType.SegaArcadePowerStickSixButton)
        {
            commands[EmulationControllerVisualControl.FaceA] = EmulationControllerCommandIds.Y;
            commands[EmulationControllerVisualControl.FaceB] = EmulationControllerCommandIds.B;
            commands[EmulationControllerVisualControl.FaceC] = EmulationControllerCommandIds.A;
            commands[EmulationControllerVisualControl.Start] = EmulationControllerCommandIds.Start;
            if (type is ControllerType.SegaMegaDriveSixButton
                or ControllerType.SegaArcadePowerStickSixButton)
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

    private static string? NormalizeVisualId(ControllerType type, string? visualId)
    {
        var compatible = CompatibleVisualIds(type);
        return compatible is null ? null
            : compatible.Contains(visualId ?? string.Empty, StringComparer.Ordinal)
                ? visualId : DefaultVisualId(type);
    }

    private static string ControllerResourceKey(ControllerType type) => type switch
    {
        ControllerType.Joypad => InputSettingsFunctionsConstants.ResourceControllerJoypad,
        ControllerType.Automatic => InputSettingsFunctionsConstants.ResourceControllerAutomatic,
        ControllerType.None => InputSettingsFunctionsConstants.ResourceControllerNone,
        _ => $"Emulation.Sega.Controller.{type}"
    };

    private static IReadOnlyDictionary<string, string> ToStrings(
        IReadOnlyDictionary<string, EmulationKey>? values) => values?.ToDictionary(
            item => item.Key, item => item.Value.ToString(), StringComparer.Ordinal)
        ?? new Dictionary<string, string>();

    private static IReadOnlyDictionary<string, EmulationKey> ToKeys(
        IReadOnlyDictionary<string, string> values) => values
        .Where(item => Enum.TryParse<EmulationKey>(item.Value, true, out _))
        .ToDictionary(item => item.Key,
            item => Enum.Parse<EmulationKey>(item.Value, true), StringComparer.Ordinal);
}
