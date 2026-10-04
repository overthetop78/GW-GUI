namespace GWGUI.Emulation.Nec.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    private static IReadOnlyList<string>? CompatibleVisualIds(ControllerType type) => type switch
    {
        ControllerType.PcEnginePad => [EmulationControllerVisualIds.NecPcEnginePad],
        ControllerType.PcEngineTurboPad => [EmulationControllerVisualIds.NecPcEngineTurboPad],
        ControllerType.TurboGrafxTurboPad => [EmulationControllerVisualIds.NecTurboGrafxTurboPad],
        ControllerType.CoreGrafxTurboPad => [EmulationControllerVisualIds.NecCoreGrafxTurboPad],
        ControllerType.CoreGrafxIITurboPad => [EmulationControllerVisualIds.NecCoreGrafxIiTurboPad],
        ControllerType.DuoRTurboPad => [EmulationControllerVisualIds.NecDuoRTurboPad],
        ControllerType.PcEngineTurboPadII => [EmulationControllerVisualIds.NecPcEngineTurboPadIi],
        ControllerType.PcEngineTurboStick => [EmulationControllerVisualIds.NecPcEngineTurboStick],
        ControllerType.AvenuePad3 => [EmulationControllerVisualIds.NecAvenuePad3],
        ControllerType.AvenuePad6 => [EmulationControllerVisualIds.NecAvenuePad6],
        ControllerType.ArcadePad6 => [EmulationControllerVisualIds.NecArcadePad6],
        ControllerType.TurboGrafxTurboStick => [EmulationControllerVisualIds.NecTurboGrafxTurboStick],
        ControllerType.DuoPad => [EmulationControllerVisualIds.NecDuoPad],
        ControllerType.CordlessPad => [EmulationControllerVisualIds.NecCordlessPad],
        ControllerType.PcEngineMouse => [EmulationControllerVisualIds.NecPcEngineMouse],
        ControllerType.TurboExpressControls => [EmulationControllerVisualIds.NecTurboExpressControls],
        ControllerType.PcEngineLtControls => [EmulationControllerVisualIds.NecPcEngineLtControls],
        ControllerType.PcFxPad => [EmulationControllerVisualIds.NecPcFxPad],
        ControllerType.PcFxMouse => [EmulationControllerVisualIds.NecPcFxMouse],
        _ => null
    };

    private static string? DefaultVisualId(ControllerType type) => type switch
    {
        ControllerType.PcEnginePad => EmulationControllerVisualIds.NecPcEnginePad,
        ControllerType.PcEngineTurboPad => EmulationControllerVisualIds.NecPcEngineTurboPad,
        ControllerType.TurboGrafxTurboPad => EmulationControllerVisualIds.NecTurboGrafxTurboPad,
        ControllerType.CoreGrafxTurboPad => EmulationControllerVisualIds.NecCoreGrafxTurboPad,
        ControllerType.CoreGrafxIITurboPad => EmulationControllerVisualIds.NecCoreGrafxIiTurboPad,
        ControllerType.DuoRTurboPad => EmulationControllerVisualIds.NecDuoRTurboPad,
        ControllerType.PcEngineTurboPadII => EmulationControllerVisualIds.NecPcEngineTurboPadIi,
        ControllerType.PcEngineTurboStick => EmulationControllerVisualIds.NecPcEngineTurboStick,
        ControllerType.AvenuePad3 => EmulationControllerVisualIds.NecAvenuePad3,
        ControllerType.AvenuePad6 => EmulationControllerVisualIds.NecAvenuePad6,
        ControllerType.ArcadePad6 => EmulationControllerVisualIds.NecArcadePad6,
        ControllerType.TurboGrafxTurboStick => EmulationControllerVisualIds.NecTurboGrafxTurboStick,
        ControllerType.DuoPad => EmulationControllerVisualIds.NecDuoPad,
        ControllerType.CordlessPad => EmulationControllerVisualIds.NecCordlessPad,
        ControllerType.PcEngineMouse => EmulationControllerVisualIds.NecPcEngineMouse,
        ControllerType.TurboExpressControls => EmulationControllerVisualIds.NecTurboExpressControls,
        ControllerType.PcEngineLtControls => EmulationControllerVisualIds.NecPcEngineLtControls,
        ControllerType.PcFxPad => EmulationControllerVisualIds.NecPcFxPad,
        ControllerType.PcFxMouse => EmulationControllerVisualIds.NecPcFxMouse,
        _ => null
    };

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string>?
        VisualCommandIds(ControllerType type)
    {
        if (ControllerCatalog.IsMouse(type))
            return new Dictionary<EmulationControllerVisualControl, string>
            {
                [EmulationControllerVisualControl.MouseLeftButton] = nameof(MouseAction.LeftButton),
                [EmulationControllerVisualControl.MouseRightButton] = nameof(MouseAction.RightButton)
            };
        if (type is not (ControllerType.PcEnginePad or ControllerType.PcEngineTurboPad
            or ControllerType.TurboGrafxTurboPad or ControllerType.CoreGrafxTurboPad
            or ControllerType.CoreGrafxIITurboPad or ControllerType.DuoRTurboPad
            or ControllerType.PcEngineTurboPadII or ControllerType.PcEngineTurboStick
            or ControllerType.AvenuePad3 or ControllerType.AvenuePad6
            or ControllerType.ArcadePad6 or ControllerType.TurboGrafxTurboStick
            or ControllerType.DuoPad or ControllerType.CordlessPad
            or ControllerType.TurboExpressControls or ControllerType.PcEngineLtControls
            or ControllerType.PcFxPad)) return null;

        var commands = new Dictionary<EmulationControllerVisualControl, string>
        {
            [EmulationControllerVisualControl.DirectionUp] = EmulationControllerCommandIds.Up,
            [EmulationControllerVisualControl.DirectionDown] = EmulationControllerCommandIds.Down,
            [EmulationControllerVisualControl.DirectionLeft] = EmulationControllerCommandIds.Left,
            [EmulationControllerVisualControl.DirectionRight] = EmulationControllerCommandIds.Right,
            [EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.B,
            [EmulationControllerVisualControl.SecondaryAction] = EmulationControllerCommandIds.A,
            [EmulationControllerVisualControl.Option] = EmulationControllerCommandIds.Option,
            [EmulationControllerVisualControl.Start] = EmulationControllerCommandIds.Start
        };
        if (type == ControllerType.AvenuePad3)
            commands[EmulationControllerVisualControl.TertiaryAction] =
                EmulationControllerCommandIds.Start;
        if (ControllerCatalog.HasSixButtons(type))
        {
            commands[EmulationControllerVisualControl.TertiaryAction] = EmulationControllerCommandIds.Y;
            commands[EmulationControllerVisualControl.QuaternaryAction] = EmulationControllerCommandIds.X;
            commands[EmulationControllerVisualControl.LeftShoulder] = EmulationControllerCommandIds.L;
            commands[EmulationControllerVisualControl.RightShoulder] = EmulationControllerCommandIds.R;
            if (type == ControllerType.PcFxPad)
                commands[EmulationControllerVisualControl.Turbo] = EmulationControllerCommandIds.L2;
        }
        return commands;
    }

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
