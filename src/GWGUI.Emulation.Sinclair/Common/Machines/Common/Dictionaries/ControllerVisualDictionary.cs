namespace GWGUI.Emulation.Sinclair.Common.Machines.Common.Dictionaries;

internal static class ControllerVisualDictionary
{
    internal static IReadOnlyList<string> VisualIds(PeripheralCategory peripheral) => peripheral == PeripheralCategory.None ? [] : [EmulationControllerVisualIds.QuickShot, EmulationControllerVisualIds.QuickShotDeluxe, EmulationControllerVisualIds.QuickShotIiTurbo, EmulationControllerVisualIds.CompetitionPro5000];
    internal static string? DefaultVisualId(PeripheralCategory peripheral) => VisualIds(peripheral).FirstOrDefault();

    internal static IReadOnlyDictionary<EmulationControllerVisualControl, string> Commands(PeripheralCategory peripheral)
    {
        if (peripheral == PeripheralCategory.None) return new Dictionary<EmulationControllerVisualControl, string>();
        return new Dictionary<EmulationControllerVisualControl, string> { [EmulationControllerVisualControl.DirectionUp] = nameof(ControllerCommand.Up), [EmulationControllerVisualControl.DirectionDown] = nameof(ControllerCommand.Down), [EmulationControllerVisualControl.DirectionLeft] = nameof(ControllerCommand.Left), [EmulationControllerVisualControl.DirectionRight] = nameof(ControllerCommand.Right), [EmulationControllerVisualControl.PrimaryAction] = nameof(ControllerCommand.Fire1) };
    }
}
