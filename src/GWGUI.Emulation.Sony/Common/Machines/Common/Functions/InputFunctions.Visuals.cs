namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    private static IReadOnlyList<string>? CompatibleVisualIds(ControllerType type,
        Model model) => type != ControllerType.Joystick ? null : model.Id switch
        {
            ModelConstants.PlayStation => [EmulationControllerVisualIds.SonyPlayStationController,
                EmulationControllerVisualIds.SonyDualShock1],
            ModelConstants.PlayStation2 => [EmulationControllerVisualIds.SonyDualShock2],
            ModelConstants.Psp => [EmulationControllerVisualIds.SonyPsp1000],
            ModelConstants.PlayStation4 => [EmulationControllerVisualIds.SonyDualShock4],
            ModelConstants.PlayStation5 => [EmulationControllerVisualIds.SonyDualSense],
            _ => [EmulationControllerVisualIds.QuickShot,
                EmulationControllerVisualIds.CompetitionPro5000,
                EmulationControllerVisualIds.ZipstikSuperPro]
        };

    private static string? DefaultVisualId(ControllerType type, Model model) =>
        CompatibleVisualIds(type, model)?.FirstOrDefault();

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string>?
        VisualCommandIds(ControllerType type, Model model) => type != ControllerType.Joystick
            ? null : model.Id == ModelConstants.Psp
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
            ModelConstants.Psp => InputSettingsFunctionsConstants.ResourcePspIntegrated,
            ModelConstants.PlayStation4 => InputSettingsFunctionsConstants.ResourceDualShock4,
            ModelConstants.PlayStation5 => InputSettingsFunctionsConstants.ResourceDualSense,
            _ => InputSettingsFunctionsConstants.ResourceControllerJoystick
        } : type switch
    {
        ControllerType.Automatic => InputSettingsFunctionsConstants.ResourceControllerAutomatic,
        ControllerType.None => InputSettingsFunctionsConstants.ResourceControllerNone,
        _ => $"Emulation.Controller.{type}"
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
