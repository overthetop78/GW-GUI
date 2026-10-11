namespace GWGUI.Emulation.Sinclair.Common.Machines.Common.Dictionaries;

internal sealed record NativeInputAction(ControllerCommand Command)
{
    internal string Id => Command.ToString();
    internal uint ButtonMask => 1u << (int)Command;
    internal string DefaultBinding => string.Empty;
    internal InputBindingDefinition Definition => new(Id, "Emulation.Controller.Action." + Id, DefaultBinding);
}

internal static class ControllerProtocolDictionary
{
    private static readonly IReadOnlyList<NativeInputAction> JoystickActions = Array.AsReadOnly(Enum.GetValues<ControllerCommand>().Select(command => new NativeInputAction(command)).ToArray());

    internal static IReadOnlyList<NativeInputAction> Actions(MachineModel model, PeripheralCategory peripheral)
    {
        if (!Enum.IsDefined(model) || !Enum.IsDefined(peripheral)) throw new ArgumentOutOfRangeException(nameof(peripheral));
        if (peripheral == PeripheralCategory.None) return [];
        if (model is MachineModel.Zx80 or MachineModel.Zx81 && peripheral != PeripheralCategory.CursorJoystick) throw new ArgumentOutOfRangeException(nameof(peripheral));
        return JoystickActions;
    }
}
