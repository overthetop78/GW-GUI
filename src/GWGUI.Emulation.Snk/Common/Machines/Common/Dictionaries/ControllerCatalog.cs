namespace GWGUI.Emulation.Snk.Common.Machines.Common.Dictionaries;

internal static class ControllerCatalog
{
    private static readonly IReadOnlyList<ControllerActionDefinition> ArcadeActions = Array.AsReadOnly(new[] { ControllerCommand.Up,
        ControllerCommand.Down,
        ControllerCommand.Left,
        ControllerCommand.Right,
        ControllerCommand.A,
        ControllerCommand.B,
        ControllerCommand.C,
        ControllerCommand.D,
        ControllerCommand.Start,
        ControllerCommand.Select }.Select(command => new ControllerActionDefinition(command)).ToArray());
    private static readonly IReadOnlyList<ControllerActionDefinition> PocketActions = Array.AsReadOnly(new[] { ControllerCommand.Up,
        ControllerCommand.Down,
        ControllerCommand.Left,
        ControllerCommand.Right,
        ControllerCommand.A,
        ControllerCommand.B,
        ControllerCommand.Option }.Select(command => new ControllerActionDefinition(command)).ToArray());
    internal static IReadOnlyList<ControllerActionDefinition> Actions(MachineModel model, PeripheralCategory peripheral)
    {
        if (!Enum.IsDefined(model) || !Enum.IsDefined(peripheral))
            throw new ArgumentOutOfRangeException(nameof(peripheral));
        if (peripheral == PeripheralCategory.None)
            return[];
        if (model is MachineModel.NeoGeoPocket or MachineModel.NeoGeoPocketColor)
            return peripheral is PeripheralCategory.NeoGeoPocket or PeripheralCategory.NeoGeoPocketColor ? PocketActions : throw new ArgumentOutOfRangeException(nameof(peripheral));
        return peripheral is PeripheralCategory.NeoGeoArcadeStick or PeripheralCategory.NeoGeoCdPad ? ArcadeActions : throw new ArgumentOutOfRangeException(nameof(peripheral));
    }
}