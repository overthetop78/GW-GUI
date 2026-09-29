namespace GWGUI.Emulation.Sega.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    private static IReadOnlyList<string>? CompatibleVisualIds(ControllerType type) =>
        type switch
        {
            ControllerType.Joypad => [EmulationControllerVisualIds.QuickShot,
                EmulationControllerVisualIds.CompetitionPro5000,
                EmulationControllerVisualIds.ZipstikSuperPro],
            ControllerType.SegaMegaDriveThreeButton => [EmulationControllerVisualIds.MegaDrive3],
            ControllerType.SegaMegaDriveSixButton => [EmulationControllerVisualIds.MegaDrive6],
            _ => null
        };

    private static string? DefaultVisualId(ControllerType type) =>
        type switch
        {
            ControllerType.Joypad => EmulationControllerVisualIds.QuickShot,
            ControllerType.SegaMegaDriveThreeButton => EmulationControllerVisualIds.MegaDrive3,
            ControllerType.SegaMegaDriveSixButton => EmulationControllerVisualIds.MegaDrive6,
            _ => null
        };

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string>?
        VisualCommandIds(ControllerType type) => type is ControllerType.Joypad
            or ControllerType.SegaMegaDriveThreeButton or ControllerType.SegaMegaDriveSixButton
        ? new Dictionary<EmulationControllerVisualControl, string>
        {
            [EmulationControllerVisualControl.DirectionUp] = EmulationControllerCommandIds.Up,
            [EmulationControllerVisualControl.DirectionDown] = EmulationControllerCommandIds.Down,
            [EmulationControllerVisualControl.DirectionLeft] = EmulationControllerCommandIds.Left,
            [EmulationControllerVisualControl.DirectionRight] = EmulationControllerCommandIds.Right,
            [EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.B,
            [EmulationControllerVisualControl.SecondaryAction] = EmulationControllerCommandIds.A
        } : null;

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
