namespace GWGUI.Emulation.Nec.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    private static IReadOnlyList<string>? CompatibleVisualIds(ControllerType type) => type switch
    {
        ControllerType.PcEnginePad => [EmulationControllerVisualIds.NecPcEnginePad],
        ControllerType.TurboGrafxTurboPad => [EmulationControllerVisualIds.NecTurboGrafxTurboPad],
        ControllerType.CoreGrafxTurboPad => [EmulationControllerVisualIds.NecCoreGrafxTurboPad],
        ControllerType.CoreGrafxIITurboPad => [EmulationControllerVisualIds.NecCoreGrafxIiTurboPad],
        _ => null
    };

    private static string? DefaultVisualId(ControllerType type) => type switch
    {
        ControllerType.PcEnginePad => EmulationControllerVisualIds.NecPcEnginePad,
        ControllerType.TurboGrafxTurboPad => EmulationControllerVisualIds.NecTurboGrafxTurboPad,
        ControllerType.CoreGrafxTurboPad => EmulationControllerVisualIds.NecCoreGrafxTurboPad,
        ControllerType.CoreGrafxIITurboPad => EmulationControllerVisualIds.NecCoreGrafxIiTurboPad,
        _ => null
    };

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string>?
        VisualCommandIds(ControllerType type) => type is ControllerType.PcEnginePad
            or ControllerType.TurboGrafxTurboPad or ControllerType.CoreGrafxTurboPad
            or ControllerType.CoreGrafxIITurboPad
            ? new Dictionary<EmulationControllerVisualControl, string>
            {
                [EmulationControllerVisualControl.DirectionUp] = EmulationControllerCommandIds.Up,
                [EmulationControllerVisualControl.DirectionDown] = EmulationControllerCommandIds.Down,
                [EmulationControllerVisualControl.DirectionLeft] = EmulationControllerCommandIds.Left,
                [EmulationControllerVisualControl.DirectionRight] = EmulationControllerCommandIds.Right,
                [EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.B,
                [EmulationControllerVisualControl.SecondaryAction] = EmulationControllerCommandIds.A,
                [EmulationControllerVisualControl.Option] = EmulationControllerCommandIds.Option,
                [EmulationControllerVisualControl.Start] = EmulationControllerCommandIds.Start
            }
            : null;

    private static string ControllerResourceKey(ControllerType type) => type switch
    {
        ControllerType.Joystick => InputSettingsFunctionsConstants.ResourceControllerJoystick,
        ControllerType.Mouse => InputSettingsFunctionsConstants.ResourceControllerMouse,
        ControllerType.Automatic => InputSettingsFunctionsConstants.ResourceControllerAutomatic,
        ControllerType.None => InputSettingsFunctionsConstants.ResourceControllerNone,
        _ => InputSettingsFunctionsConstants.ResourceControllerPrefix + type
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
