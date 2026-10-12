namespace GWGUI.Emulation.Snk.Common.Machines.Common.Dictionaries;

internal static class ControllerVisualDictionary
{
    internal static IReadOnlyList<string> VisualIds(PeripheralCategory peripheral) => peripheral switch
    {
        PeripheralCategory.None => [],
        PeripheralCategory.NeoGeoArcadeStick => [EmulationControllerVisualIds.SnkNeoGeoArcadeStick],
        PeripheralCategory.NeoGeoCdPad => [EmulationControllerVisualIds.SnkNeoGeoCdPad],
        PeripheralCategory.NeoGeoPocket => [EmulationControllerVisualIds.SnkNeoGeoPocket],
        PeripheralCategory.NeoGeoPocketColor => [EmulationControllerVisualIds.SnkNeoGeoPocketColor],
        _ => throw new ArgumentOutOfRangeException(nameof(peripheral))};
    internal static string? DefaultVisualId(PeripheralCategory peripheral) => VisualIds(peripheral).FirstOrDefault();

    internal static IReadOnlyDictionary<EmulationControllerVisualControl, string> Commands(PeripheralCategory peripheral)
    {
        if (!Enum.IsDefined(peripheral))
            throw new ArgumentOutOfRangeException(nameof(peripheral));
        if (peripheral == PeripheralCategory.None)
            return new Dictionary<EmulationControllerVisualControl, string>();
        var result = new Dictionary<EmulationControllerVisualControl, string>
        {
            [EmulationControllerVisualControl.DirectionUp] = nameof(ControllerCommand.Up),
            [EmulationControllerVisualControl.DirectionDown] = nameof(ControllerCommand.Down),
            [EmulationControllerVisualControl.DirectionLeft] = nameof(ControllerCommand.Left),
            [EmulationControllerVisualControl.DirectionRight] = nameof(ControllerCommand.Right),
            [EmulationControllerVisualControl.PrimaryAction] = nameof(ControllerCommand.A),
            [EmulationControllerVisualControl.SecondaryAction] = nameof(ControllerCommand.B)
        };
        if (peripheral is PeripheralCategory.NeoGeoPocket or PeripheralCategory.NeoGeoPocketColor)
            result[EmulationControllerVisualControl.Option] = nameof(ControllerCommand.Option);
        else
        {
            result[EmulationControllerVisualControl.TertiaryAction] = nameof(ControllerCommand.C);
            result[EmulationControllerVisualControl.QuaternaryAction] = nameof(ControllerCommand.D);
            result[EmulationControllerVisualControl.Start] = nameof(ControllerCommand.Start);
            result[EmulationControllerVisualControl.Option] = nameof(ControllerCommand.Select);
        }
    
        return result;
    }
}
