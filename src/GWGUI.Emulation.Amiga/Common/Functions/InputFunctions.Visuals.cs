using GWGUI.Emulation;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Amiga.Common.Functions;

internal static partial class InputSettingsFunctions
{
private static IReadOnlyList<string>? CompatibleVisualIds(ControllerType type) => type switch
    {
        ControllerType.Joystick =>
        [
            EmulationControllerVisualIds.QuickShot,
            EmulationControllerVisualIds.QuickShotDeluxe,
            EmulationControllerVisualIds.QuickShotIiTurbo,
            EmulationControllerVisualIds.CompetitionPro5000,
            EmulationControllerVisualIds.ZipstikSuperPro,
            EmulationControllerVisualIds.KonixSpeedkingLeftHand,
            EmulationControllerVisualIds.KonixSpeedkingRightHand,
            EmulationControllerVisualIds.SuncomTac2,
            EmulationControllerVisualIds.PowerplayCruiser,
            EmulationControllerVisualIds.SuzoTheArcadeTurbo,
            EmulationControllerVisualIds.AdvancedGravisGamepad
        ],
        ControllerType.AnalogJoystick => [EmulationControllerVisualIds.KonixSpeedkingAnalog],
        ControllerType.Cd32Pad =>
            [EmulationControllerVisualIds.CommodoreCd32, EmulationControllerVisualIds.CompetitionProCd32],
        _ => null
    };

    private static string? DefaultVisualId(ControllerType type) => type switch
    {
        ControllerType.Joystick => EmulationControllerVisualIds.QuickShot,
        ControllerType.AnalogJoystick => EmulationControllerVisualIds.KonixSpeedkingAnalog,
        ControllerType.Cd32Pad => EmulationControllerVisualIds.CommodoreCd32,
        _ => null
    };

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string>? VisualCommandIds(
        ControllerType type) => type switch
    {
        ControllerType.Joystick or ControllerType.AnalogJoystick =>
            new Dictionary<EmulationControllerVisualControl, string>
            {
                [EmulationControllerVisualControl.DirectionUp] = EmulationControllerCommandIds.Up,
                [EmulationControllerVisualControl.DirectionDown] = EmulationControllerCommandIds.Down,
                [EmulationControllerVisualControl.DirectionLeft] = EmulationControllerCommandIds.Left,
                [EmulationControllerVisualControl.DirectionRight] = EmulationControllerCommandIds.Right,
                [EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.B,
                [EmulationControllerVisualControl.SecondaryAction] = EmulationControllerCommandIds.A,
                [EmulationControllerVisualControl.Turbo] = EmulationControllerCommandIds.L2
            },
        ControllerType.Cd32Pad => new Dictionary<EmulationControllerVisualControl, string>
        {
            [EmulationControllerVisualControl.DirectionUp] = EmulationControllerCommandIds.Up,
            [EmulationControllerVisualControl.DirectionDown] = EmulationControllerCommandIds.Down,
            [EmulationControllerVisualControl.DirectionLeft] = EmulationControllerCommandIds.Left,
            [EmulationControllerVisualControl.DirectionRight] = EmulationControllerCommandIds.Right,
            [EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.B,
            [EmulationControllerVisualControl.SecondaryAction] = EmulationControllerCommandIds.A,
            [EmulationControllerVisualControl.TertiaryAction] = EmulationControllerCommandIds.Y,
            [EmulationControllerVisualControl.QuaternaryAction] = EmulationControllerCommandIds.X,
            [EmulationControllerVisualControl.LeftShoulder] = EmulationControllerCommandIds.L,
            [EmulationControllerVisualControl.RightShoulder] = EmulationControllerCommandIds.R,
            [EmulationControllerVisualControl.Start] = EmulationControllerCommandIds.Start,
            [EmulationControllerVisualControl.Turbo] = EmulationControllerCommandIds.L2
        },
        _ => null
    };

    private static string ControllerResourceKey(ControllerType type) => type switch
    {
        ControllerType.Joystick => InputSettingsFunctionsConstants.ResourceAmigaControllerJoystick,
        ControllerType.AnalogJoystick => InputSettingsFunctionsConstants.ResourceControllerAnalogJoystick,
        ControllerType.Cd32Pad => InputSettingsFunctionsConstants.ResourceAmigaControllerCd32,
        ControllerType.Automatic => InputSettingsFunctionsConstants.ResourceControllerAutomatic,
        ControllerType.None => InputSettingsFunctionsConstants.ResourceControllerNone,
        _ => $"Emulation.Controller.{type}"
    };

    private static IReadOnlyDictionary<string, string> ToStrings(
        IReadOnlyDictionary<string, EmulationKey>? values) => values?.ToDictionary(item => item.Key,
        item => item.Value.ToString(), StringComparer.Ordinal) ?? new Dictionary<string, string>();

    private static IReadOnlyDictionary<string, EmulationKey> ToKeys(IReadOnlyDictionary<string, string> values) =>
        values.Where(item => Enum.TryParse<EmulationKey>(item.Value, true, out _)).ToDictionary(item => item.Key,
            item => Enum.Parse<EmulationKey>(item.Value, true), StringComparer.Ordinal);
}
