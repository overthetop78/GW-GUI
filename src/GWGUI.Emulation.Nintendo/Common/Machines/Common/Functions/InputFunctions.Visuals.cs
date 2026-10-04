namespace GWGUI.Emulation.Nintendo.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    private static IReadOnlyList<string>? CompatibleVisualIds(ControllerType type,
        Model model) => type != ControllerType.Joystick ? null : model.Id switch
        {
            ModelConstants.Nes or ModelConstants.FamicomDisk =>
                [EmulationControllerVisualIds.NintendoNesPad,
                    EmulationControllerVisualIds.NintendoNesDogbonePad,
                    EmulationControllerVisualIds.NintendoFamicomPad1],
            ModelConstants.Snes => [EmulationControllerVisualIds.NintendoSuperNesPad,
                EmulationControllerVisualIds.NintendoSuperFamicomPad],
            ModelConstants.GameBoy => [EmulationControllerVisualIds.NintendoGameBoy],
            ModelConstants.GameBoyColor => [EmulationControllerVisualIds.NintendoGameBoyColor],
            ModelConstants.GameBoyAdvance => [EmulationControllerVisualIds.NintendoGameBoyAdvance],
            _ => [EmulationControllerVisualIds.QuickShot,
                EmulationControllerVisualIds.CompetitionPro5000,
                EmulationControllerVisualIds.ZipstikSuperPro]
        };

    private static string? DefaultVisualId(ControllerType type, Model model) =>
        CompatibleVisualIds(type, model)?.FirstOrDefault();

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string>?
        VisualCommandIds(ControllerType type, Model model) => type != ControllerType.Joystick
            ? null : model.Id is ModelConstants.Nes or ModelConstants.FamicomDisk
                or ModelConstants.Snes or ModelConstants.GameBoy
                or ModelConstants.GameBoyColor or ModelConstants.GameBoyAdvance
            ? NintendoVisualCommandIds(model.Id == ModelConstants.Snes,
                model.Id == ModelConstants.GameBoyAdvance)
            : new Dictionary<EmulationControllerVisualControl, string>
        {
            [EmulationControllerVisualControl.DirectionUp] = EmulationControllerCommandIds.Up,
            [EmulationControllerVisualControl.DirectionDown] = EmulationControllerCommandIds.Down,
            [EmulationControllerVisualControl.DirectionLeft] = EmulationControllerCommandIds.Left,
            [EmulationControllerVisualControl.DirectionRight] = EmulationControllerCommandIds.Right,
            [EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.B,
            [EmulationControllerVisualControl.SecondaryAction] = EmulationControllerCommandIds.A
        };

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string>
        NintendoVisualCommandIds(bool superNintendo, bool gameBoyAdvance)
    {
        var commands = new Dictionary<EmulationControllerVisualControl, string>
        {
            [EmulationControllerVisualControl.DirectionUp] = EmulationControllerCommandIds.Up,
            [EmulationControllerVisualControl.DirectionDown] = EmulationControllerCommandIds.Down,
            [EmulationControllerVisualControl.DirectionLeft] = EmulationControllerCommandIds.Left,
            [EmulationControllerVisualControl.DirectionRight] = EmulationControllerCommandIds.Right,
            [EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.B,
            [EmulationControllerVisualControl.SecondaryAction] = EmulationControllerCommandIds.A,
            [EmulationControllerVisualControl.Option] = EmulationControllerCommandIds.Select,
            [EmulationControllerVisualControl.Start] = EmulationControllerCommandIds.Start
        };
        if (superNintendo)
        {
            commands[EmulationControllerVisualControl.TertiaryAction] =
                EmulationControllerCommandIds.Y;
            commands[EmulationControllerVisualControl.QuaternaryAction] =
                EmulationControllerCommandIds.X;
        }
        if (superNintendo || gameBoyAdvance)
        {
            commands[EmulationControllerVisualControl.LeftShoulder] =
                EmulationControllerCommandIds.L;
            commands[EmulationControllerVisualControl.RightShoulder] =
                EmulationControllerCommandIds.R;
        }
        return commands;
    }

    private static string ControllerResourceKey(ControllerType type, Model model) =>
        type == ControllerType.Joystick ? model.Id switch
        {
            ModelConstants.Nes or ModelConstants.FamicomDisk =>
                InputSettingsFunctionsConstants.ResourceNesPad,
            ModelConstants.Snes => InputSettingsFunctionsConstants.ResourceSuperNesPad,
            ModelConstants.GameBoy => InputSettingsFunctionsConstants.ResourceGameBoy,
            ModelConstants.GameBoyColor => InputSettingsFunctionsConstants.ResourceGameBoyColor,
            ModelConstants.GameBoyAdvance => InputSettingsFunctionsConstants.ResourceGameBoyAdvance,
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
