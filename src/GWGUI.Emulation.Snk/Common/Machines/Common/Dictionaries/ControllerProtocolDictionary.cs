namespace GWGUI.Emulation.Snk.Common.Machines.Common.Dictionaries;
internal sealed record NativeInputAction(ControllerCommand Command)
{
    internal string Id => Command.ToString();
    internal uint ButtonMask => 1u << (int)Command;
    internal string DefaultBinding => string.Empty;
    internal InputBindingDefinition Definition => new(Id, ResourceKey, DefaultBinding);
    private string ResourceKey => Command is ControllerCommand.Up or ControllerCommand.Down or ControllerCommand.Left or ControllerCommand.Right ? "Emulation.Controller.Action." + Id : InputMappingConstants.ResourcePrefix + Id;
}

internal static class ControllerProtocolDictionary
{
    private static readonly IReadOnlyList<NativeInputAction> ArcadeActions = Array.AsReadOnly(new[] { ControllerCommand.Up,
        ControllerCommand.Down,
        ControllerCommand.Left,
        ControllerCommand.Right,
        ControllerCommand.A,
        ControllerCommand.B,
        ControllerCommand.C,
        ControllerCommand.D,
        ControllerCommand.Start,
        ControllerCommand.Select }.Select(command => new NativeInputAction(command)).ToArray());
    private static readonly IReadOnlyList<NativeInputAction> PocketActions = Array.AsReadOnly(new[] { ControllerCommand.Up,
        ControllerCommand.Down,
        ControllerCommand.Left,
        ControllerCommand.Right,
        ControllerCommand.A,
        ControllerCommand.B,
        ControllerCommand.Option }.Select(command => new NativeInputAction(command)).ToArray());
    internal static IReadOnlyList<NativeInputAction> Actions(MachineModel model, PeripheralCategory peripheral)
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