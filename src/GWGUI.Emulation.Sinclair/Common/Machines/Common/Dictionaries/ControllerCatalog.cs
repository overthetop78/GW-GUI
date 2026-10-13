namespace GWGUI.Emulation.Sinclair.Common.Machines.Common.Dictionaries;

internal static class ControllerCatalog
{
    private static readonly IReadOnlyList<InputActionDefinition> JoystickActions = Array.AsReadOnly(Enum.GetValues<ControllerCommand>().Select(command => new InputActionDefinition(command)).ToArray());

    internal static IReadOnlyList<InputActionDefinition> Actions(MachineModel model, PeripheralCategory peripheral)
    {
        if (!Enum.IsDefined(model) || !Enum.IsDefined(peripheral)) throw new ArgumentOutOfRangeException(nameof(peripheral));
        if (peripheral == PeripheralCategory.None) return [];
        if (model is MachineModel.Zx80 or MachineModel.Zx81 && peripheral != PeripheralCategory.CursorJoystick) throw new ArgumentOutOfRangeException(nameof(peripheral));
        return JoystickActions;
    }
}
