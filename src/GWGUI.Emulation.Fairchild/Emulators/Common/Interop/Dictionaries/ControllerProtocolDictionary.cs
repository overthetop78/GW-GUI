namespace GWGUI.Emulation.Fairchild.Emulators.Common.Interop.Dictionaries;

internal sealed record NativeInputAction(string Id, int Button)
{
    internal string DefaultBinding => string.Empty;

    internal InputBindingDefinition Definition => new(Id, Id is "Up" or "Down" or "Left" or "Right" ? "Emulation.Controller.Action." + Id : InputMappingConstants.ResourcePrefix + Id, DefaultBinding);
}

internal static class ControllerProtocolDictionary
{
    private enum NativeButton { Push = 0, RotateLeft = 1, SwapControllers = 2, ConsoleOverlay = 3, Up = 4, Down = 5, Left = 6, Right = 7, RotateRight = 8, Pull = 9 }

    private static NativeInputAction Button(string id, NativeButton nativeButton) => new(id, (int)nativeButton);

    internal static IReadOnlyList<NativeInputAction> Actions(MachineModel model, Emulator core, int port)
    {
        if (!Enum.IsDefined(model) || core != Emulator.FreeChaF)
            throw new ArgumentOutOfRangeException(nameof(model));
        if (port < ControllerConstants.FirstPort || port >= ControllerConstants.PortCount)
            return [];
        return [
            Button("Up", NativeButton.Up), Button("Down", NativeButton.Down),
            Button("Left", NativeButton.Left), Button("Right", NativeButton.Right),
            Button("RotateLeft", NativeButton.RotateLeft), Button("RotateRight", NativeButton.RotateRight),
            Button("Pull", NativeButton.Pull), Button("Push", NativeButton.Push),
            Button("ConsoleOverlay", NativeButton.ConsoleOverlay),
            Button("SwapControllers", NativeButton.SwapControllers)];
    }
}
