using GWGUI.Emulation;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Commodore.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    internal static string KeyResource(EmulationKey key) => key switch
    {
        EmulationKey.Help => InputSettingsFunctionsConstants.ResourceKeyHelp,
        EmulationKey.LeftAmiga => InputSettingsFunctionsConstants.ResourceKeyLeftAmiga,
        EmulationKey.RightAmiga => InputSettingsFunctionsConstants.ResourceKeyRightAmiga,
        _ => key.ToString()
    };

    internal static string DefaultKey(EmulationKey key, IReadOnlyDictionary<EmulationKey, EmulationKey> defaults) => defaults.GetValueOrDefault(key, key).ToString();

    private static IReadOnlyList<string>? CompatibleVisualIds(PeripheralCategory type) => type switch
    {
        PeripheralCategory.Joystick =>
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
        PeripheralCategory.AnalogJoystick => [EmulationControllerVisualIds.KonixSpeedkingAnalog],
        PeripheralCategory.Cd32Pad =>
            [EmulationControllerVisualIds.CommodoreCd32, EmulationControllerVisualIds.CompetitionProCd32],
        _ => null
    };

    private static string? DefaultVisualId(PeripheralCategory type) => type switch
    {
        PeripheralCategory.Joystick => EmulationControllerVisualIds.QuickShot,
        PeripheralCategory.AnalogJoystick => EmulationControllerVisualIds.KonixSpeedkingAnalog,
        PeripheralCategory.Cd32Pad => EmulationControllerVisualIds.CommodoreCd32,
        _ => null
    };

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string>? VisualCommandIds(PeripheralCategory type) => type switch
    {
        PeripheralCategory.Joystick or PeripheralCategory.AnalogJoystick =>
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
        PeripheralCategory.Cd32Pad => new Dictionary<EmulationControllerVisualControl, string>
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

    private static string ControllerResourceKey(PeripheralCategory type) => type switch
    {
        PeripheralCategory.Joystick => InputSettingsFunctionsConstants.ResourceAmigaControllerJoystick,
        PeripheralCategory.AnalogJoystick => InputSettingsFunctionsConstants.ResourceControllerAnalogJoystick,
        PeripheralCategory.Cd32Pad => InputSettingsFunctionsConstants.ResourceAmigaControllerCd32,
        PeripheralCategory.RetroPad => InputSnapshotFunctionsConstants.GamepadResourceKey,
        PeripheralCategory.Keyboard => InputSnapshotFunctionsConstants.KeyboardResourceKey,
        PeripheralCategory.Automatic => InputSettingsFunctionsConstants.ResourceControllerAutomatic,
        PeripheralCategory.None => InputSettingsFunctionsConstants.ResourceControllerNone,
        _ => string.Format(System.Globalization.CultureInfo.InvariantCulture, InputSnapshotFunctionsConstants.ControllerResourceFormat, type)
    };
}
