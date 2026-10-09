namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    private static IReadOnlyList<string>? CompatibleVisualIds(ControllerType type,
        Model model) => type is not (ControllerType.Joystick or ControllerType.DualShock)
            ? null : model.Id switch
        {
            ModelConstants.PlayStation => type == ControllerType.DualShock
                ? [EmulationControllerVisualIds.SonyDualShock1]
                : [EmulationControllerVisualIds.SonyPlayStationController],
            ModelConstants.PlayStation2 => [EmulationControllerVisualIds.SonyDualShock2],
            ModelConstants.PocketStation => null,
            ModelConstants.Psp => [EmulationControllerVisualIds.SonyPsp1000],
            ModelConstants.PlayStation4 => [EmulationControllerVisualIds.SonyDualShock4],
            ModelConstants.PlayStation5 => [EmulationControllerVisualIds.SonyDualSense],
            _ => null
        };

    private static string? DefaultVisualId(ControllerType type, Model model) =>
        CompatibleVisualIds(type, model)?.FirstOrDefault();

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string>?
        VisualCommandIds(ControllerType type, Model model) =>
            type is not (ControllerType.Joystick or ControllerType.DualShock)
            ? null : model.Id == ModelConstants.PocketStation ? null
            : model.Id == ModelConstants.Psp
            ? new Dictionary<EmulationControllerVisualControl, string>
            {
                [EmulationControllerVisualControl.DirectionUp] = EmulationControllerCommandIds.Up,
                [EmulationControllerVisualControl.DirectionDown] = EmulationControllerCommandIds.Down,
                [EmulationControllerVisualControl.DirectionLeft] = EmulationControllerCommandIds.Left,
                [EmulationControllerVisualControl.DirectionRight] = EmulationControllerCommandIds.Right,
                [EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.B,
                [EmulationControllerVisualControl.SecondaryAction] = EmulationControllerCommandIds.A,
                [EmulationControllerVisualControl.TertiaryAction] = EmulationControllerCommandIds.Y,
                [EmulationControllerVisualControl.QuaternaryAction] = EmulationControllerCommandIds.X,
                [EmulationControllerVisualControl.Option] = EmulationControllerCommandIds.Select,
                [EmulationControllerVisualControl.Start] = EmulationControllerCommandIds.Start,
                [EmulationControllerVisualControl.LeftShoulder] = EmulationControllerCommandIds.L,
                [EmulationControllerVisualControl.RightShoulder] = EmulationControllerCommandIds.R,
                [EmulationControllerVisualControl.StickUp] = InputSettingsFunctionsConstants.PspStickUp,
                [EmulationControllerVisualControl.StickDown] = InputSettingsFunctionsConstants.PspStickDown,
                [EmulationControllerVisualControl.StickLeft] = InputSettingsFunctionsConstants.PspStickLeft,
                [EmulationControllerVisualControl.StickRight] = InputSettingsFunctionsConstants.PspStickRight
            }
            : model.Id is ModelConstants.PlayStation or ModelConstants.PlayStation2
                or ModelConstants.PlayStation3
            ? new Dictionary<EmulationControllerVisualControl, string>
            {
                [EmulationControllerVisualControl.DirectionUp] = EmulationControllerCommandIds.Up,
                [EmulationControllerVisualControl.DirectionDown] = EmulationControllerCommandIds.Down,
                [EmulationControllerVisualControl.DirectionLeft] = EmulationControllerCommandIds.Left,
                [EmulationControllerVisualControl.DirectionRight] = EmulationControllerCommandIds.Right,
                [EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.B,
                [EmulationControllerVisualControl.SecondaryAction] = EmulationControllerCommandIds.A,
                [EmulationControllerVisualControl.TertiaryAction] = EmulationControllerCommandIds.Y,
                [EmulationControllerVisualControl.QuaternaryAction] = EmulationControllerCommandIds.X,
                [EmulationControllerVisualControl.Option] = EmulationControllerCommandIds.Select,
                [EmulationControllerVisualControl.Start] = EmulationControllerCommandIds.Start,
                [EmulationControllerVisualControl.LeftShoulder] = EmulationControllerCommandIds.L,
                [EmulationControllerVisualControl.RightShoulder] = EmulationControllerCommandIds.R,
                [EmulationControllerVisualControl.LeftTrigger] = EmulationControllerCommandIds.L2,
                [EmulationControllerVisualControl.RightTrigger] = EmulationControllerCommandIds.R2,
                [EmulationControllerVisualControl.LeftStick] = EmulationControllerCommandIds.L3,
                [EmulationControllerVisualControl.RightStick] = EmulationControllerCommandIds.R3
            }
        : new Dictionary<EmulationControllerVisualControl, string>
        {
            [EmulationControllerVisualControl.DirectionUp] = EmulationControllerCommandIds.Up,
            [EmulationControllerVisualControl.DirectionDown] = EmulationControllerCommandIds.Down,
            [EmulationControllerVisualControl.DirectionLeft] = EmulationControllerCommandIds.Left,
            [EmulationControllerVisualControl.DirectionRight] = EmulationControllerCommandIds.Right,
            [EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.B,
            [EmulationControllerVisualControl.SecondaryAction] = EmulationControllerCommandIds.A
        };

    private static string ControllerResourceKey(ControllerType type, Model model) =>
        type == ControllerType.Joystick ? model.Id switch
        {
            ModelConstants.PlayStation => InputSettingsFunctionsConstants.ResourcePlayStationController,
            ModelConstants.PlayStation2 => InputSettingsFunctionsConstants.ResourceDualShock2,
            ModelConstants.PlayStation3 => InputSettingsFunctionsConstants.ResourceDualShock3,
            ModelConstants.PocketStation => Machines.PocketStation.Constants.InputConstants.IntegratedControllerLabel,
            ModelConstants.Psp => InputSettingsFunctionsConstants.ResourcePspIntegrated,
            ModelConstants.PlayStation4 => InputSettingsFunctionsConstants.ResourceDualShock4,
            ModelConstants.PlayStation5 => InputSettingsFunctionsConstants.ResourceDualSense,
            _ => InputSettingsFunctionsConstants.ResourceControllerJoystick
        } : type switch
    {
        ControllerType.Automatic => InputSettingsFunctionsConstants.ResourceControllerAutomatic,
        ControllerType.None => InputSettingsFunctionsConstants.ResourceControllerNone,
        ControllerType.DualShock => InputSettingsFunctionsConstants.ResourceDualShock,
        ControllerType.AnalogController => InputSettingsFunctionsConstants.ResourceDualAnalog,
        ControllerType.AnalogJoystick => InputSettingsFunctionsConstants.ResourceAnalogJoystick,
        ControllerType.GunCon => InputSettingsFunctionsConstants.ResourceGunCon,
        ControllerType.Justifier => InputSettingsFunctionsConstants.ResourceJustifier,
        ControllerType.Mouse => InputSettingsFunctionsConstants.ResourcePlayStationMouse,
        ControllerType.Keyboard => InputSettingsFunctionsConstants.ResourceControllerKeyboard,
        ControllerType.NeGcon or ControllerType.NeGconRumble or ControllerType.KeyboardAndMouse
            => string.Empty,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
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
