namespace GWGUI.Emulation.Philips.Emulators.Common.Interop.Dictionaries;

internal sealed record NativeInputAction(string Id, int Button, int AdditionalButton = InputMappingConstants.Unassigned)
{
    internal uint ButtonMask => (1u << Button) | (AdditionalButton >= 0 ? 1u << AdditionalButton : 0u);
    internal string DefaultBinding => string.Empty;
    internal InputBindingDefinition Definition => new(Id, Id is "Up" or "Down" or "Left" or "Right" ? "Emulation.Controller.Action." + Id : InputMappingConstants.ResourcePrefix + Id, DefaultBinding);
}

internal static class ControllerProtocolDictionary
{
    private enum NativeButton { First = 0, Second = 1, Third = 2, Up = 4, Down = 5, Left = 6, Right = 7, A = 8 }
    private static NativeInputAction Button(string id, NativeButton button) => new(id, (int)button);
    internal static IReadOnlyList<NativeInputAction> Actions(MachineModel model, PeripheralCategory peripheral)
    {
        if (ConfigurationFunctions.Family(model) == MachineFamily.P2000 || peripheral == PeripheralCategory.None) return [];
        if (peripheral == PeripheralCategory.Mouse)
            return [Button("Action1", NativeButton.First), Button("Action2", NativeButton.Second), Button("Action3", NativeButton.Third)];
        var directions = new[] { Button("Up", NativeButton.Up), Button("Down", NativeButton.Down),
            Button("Left", NativeButton.Left), Button("Right", NativeButton.Right) };
        return peripheral switch
        {
            PeripheralCategory.Joystick => [.. directions, Button("Fire", NativeButton.First)],
            PeripheralCategory.CdiGamepad => [.. directions, Button("Action1", NativeButton.A), Button("Action2", NativeButton.First),
                new("Action3", (int)NativeButton.A, (int)NativeButton.First)],
            PeripheralCategory.LightGun => [.. directions, Button("Trigger", NativeButton.First),
                Button("Auxiliary", NativeButton.Second), Button("Reload", NativeButton.Third)],
            _ => throw new ArgumentOutOfRangeException(nameof(peripheral))
        };
    }
}
