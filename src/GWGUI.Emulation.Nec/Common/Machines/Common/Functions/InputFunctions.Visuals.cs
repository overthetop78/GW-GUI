namespace GWGUI.Emulation.Nec.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    private static IReadOnlyList<string>? CompatibleVisualIds(PeripheralCategory type) => type switch
    {
        PeripheralCategory.PcEnginePad => [EmulationControllerVisualIds.NecPcEnginePad],
        PeripheralCategory.PcEngineTurboPad => [EmulationControllerVisualIds.NecPcEngineTurboPad],
        PeripheralCategory.TurboGrafxTurboPad => [EmulationControllerVisualIds.NecTurboGrafxTurboPad],
        PeripheralCategory.CoreGrafxTurboPad => [EmulationControllerVisualIds.NecCoreGrafxTurboPad],
        PeripheralCategory.CoreGrafxIITurboPad => [EmulationControllerVisualIds.NecCoreGrafxIiTurboPad],
        PeripheralCategory.DuoRTurboPad => [EmulationControllerVisualIds.NecDuoRTurboPad],
        PeripheralCategory.PcEngineTurboPadII => [EmulationControllerVisualIds.NecPcEngineTurboPadIi],
        PeripheralCategory.PcEngineTurboStick => [EmulationControllerVisualIds.NecPcEngineTurboStick],
        PeripheralCategory.AvenuePad3 => [EmulationControllerVisualIds.NecAvenuePad3],
        PeripheralCategory.AvenuePad6 => [EmulationControllerVisualIds.NecAvenuePad6],
        PeripheralCategory.ArcadePad6 => [EmulationControllerVisualIds.NecArcadePad6],
        PeripheralCategory.TurboGrafxTurboStick => [EmulationControllerVisualIds.NecTurboGrafxTurboStick],
        PeripheralCategory.DuoPad => [EmulationControllerVisualIds.NecDuoPad],
        PeripheralCategory.CordlessPad => [EmulationControllerVisualIds.NecCordlessPad],
        PeripheralCategory.PcEngineMouse => [EmulationControllerVisualIds.NecPcEngineMouse],
        PeripheralCategory.TurboExpressControls => [EmulationControllerVisualIds.NecTurboExpressControls],
        PeripheralCategory.PcEngineLtControls => [EmulationControllerVisualIds.NecPcEngineLtControls],
        PeripheralCategory.PcFxPad => [EmulationControllerVisualIds.NecPcFxPad],
        PeripheralCategory.PcFxMouse => [EmulationControllerVisualIds.NecPcFxMouse],
        _ => null
    };

    private static string? DefaultVisualId(PeripheralCategory type) => type switch
    {
        PeripheralCategory.PcEnginePad => EmulationControllerVisualIds.NecPcEnginePad,
        PeripheralCategory.PcEngineTurboPad => EmulationControllerVisualIds.NecPcEngineTurboPad,
        PeripheralCategory.TurboGrafxTurboPad => EmulationControllerVisualIds.NecTurboGrafxTurboPad,
        PeripheralCategory.CoreGrafxTurboPad => EmulationControllerVisualIds.NecCoreGrafxTurboPad,
        PeripheralCategory.CoreGrafxIITurboPad => EmulationControllerVisualIds.NecCoreGrafxIiTurboPad,
        PeripheralCategory.DuoRTurboPad => EmulationControllerVisualIds.NecDuoRTurboPad,
        PeripheralCategory.PcEngineTurboPadII => EmulationControllerVisualIds.NecPcEngineTurboPadIi,
        PeripheralCategory.PcEngineTurboStick => EmulationControllerVisualIds.NecPcEngineTurboStick,
        PeripheralCategory.AvenuePad3 => EmulationControllerVisualIds.NecAvenuePad3,
        PeripheralCategory.AvenuePad6 => EmulationControllerVisualIds.NecAvenuePad6,
        PeripheralCategory.ArcadePad6 => EmulationControllerVisualIds.NecArcadePad6,
        PeripheralCategory.TurboGrafxTurboStick => EmulationControllerVisualIds.NecTurboGrafxTurboStick,
        PeripheralCategory.DuoPad => EmulationControllerVisualIds.NecDuoPad,
        PeripheralCategory.CordlessPad => EmulationControllerVisualIds.NecCordlessPad,
        PeripheralCategory.PcEngineMouse => EmulationControllerVisualIds.NecPcEngineMouse,
        PeripheralCategory.TurboExpressControls => EmulationControllerVisualIds.NecTurboExpressControls,
        PeripheralCategory.PcEngineLtControls => EmulationControllerVisualIds.NecPcEngineLtControls,
        PeripheralCategory.PcFxPad => EmulationControllerVisualIds.NecPcFxPad,
        PeripheralCategory.PcFxMouse => EmulationControllerVisualIds.NecPcFxMouse,
        _ => null
    };

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string>?
        VisualCommandIds(PeripheralCategory type)
    {
        if (ControllerCatalog.IsMouse(type))
            return new Dictionary<EmulationControllerVisualControl, string>
            {
                [EmulationControllerVisualControl.MouseLeftButton] = nameof(MouseAction.LeftButton),
                [EmulationControllerVisualControl.MouseRightButton] = nameof(MouseAction.RightButton)
            };
        if (type is not (PeripheralCategory.PcEnginePad or PeripheralCategory.PcEngineTurboPad
            or PeripheralCategory.TurboGrafxTurboPad or PeripheralCategory.CoreGrafxTurboPad
            or PeripheralCategory.CoreGrafxIITurboPad or PeripheralCategory.DuoRTurboPad
            or PeripheralCategory.PcEngineTurboPadII or PeripheralCategory.PcEngineTurboStick
            or PeripheralCategory.AvenuePad3 or PeripheralCategory.AvenuePad6
            or PeripheralCategory.ArcadePad6 or PeripheralCategory.TurboGrafxTurboStick
            or PeripheralCategory.DuoPad or PeripheralCategory.CordlessPad
            or PeripheralCategory.TurboExpressControls or PeripheralCategory.PcEngineLtControls
            or PeripheralCategory.PcFxPad)) return null;

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
        if (type == PeripheralCategory.AvenuePad3)
            commands[EmulationControllerVisualControl.TertiaryAction] = EmulationControllerCommandIds.Start;
        if (ControllerCatalog.HasSixButtons(type))
        {
            commands[EmulationControllerVisualControl.TertiaryAction] = EmulationControllerCommandIds.Y;
            commands[EmulationControllerVisualControl.QuaternaryAction] = EmulationControllerCommandIds.X;
            commands[EmulationControllerVisualControl.LeftShoulder] = EmulationControllerCommandIds.L;
            commands[EmulationControllerVisualControl.RightShoulder] = EmulationControllerCommandIds.R;
            if (type == PeripheralCategory.PcFxPad)
                commands[EmulationControllerVisualControl.Turbo] = EmulationControllerCommandIds.L2;
        }
        return commands;
    }

    private static string ControllerResourceKey(PeripheralCategory type) => type switch
    {
        PeripheralCategory.Joystick => InputSettingsFunctionsConstants.ResourceControllerJoystick,
        PeripheralCategory.Mouse => InputSettingsFunctionsConstants.ResourceControllerMouse,
        PeripheralCategory.Automatic => InputSettingsFunctionsConstants.ResourceControllerAutomatic,
        PeripheralCategory.None => InputSettingsFunctionsConstants.ResourceControllerNone,
        _ => InputSettingsFunctionsConstants.ResourceControllerPrefix + type
    };
}
