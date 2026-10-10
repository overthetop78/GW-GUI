using GWGUI.Emulation.Epoch.Common.Machines.Common.Constants;

namespace GWGUI.Emulation.Epoch.Common.Machines.Common.Dictionaries;

internal sealed record NativeInputAction(string Id, string DefaultBinding,
    int Button = InputMappingConstants.Unassigned, EmulationKey? Key = null,
    int Axis = InputMappingConstants.Unassigned, int Direction = 1, uint CarrierMask = 0)
{
    internal InputBindingDefinition Definition => new(Id,
        Id is "Up" or "Down" or "Left" or "Right" ? "Emulation.Controller.Action." + Id : InputMappingConstants.ResourcePrefix + Id, DefaultBinding,
        Id.StartsWith(InputMappingConstants.KeypadPrefix, StringComparison.Ordinal)
            ? Id[InputMappingConstants.KeypadPrefix.Length..] : null);
}

internal static class ControllerProtocolDictionary
{
    private enum NativeButton { B = 0, Y = 1, Select = 2, Start = 3, Up = 4, Down = 5, Left = 6, Right = 7, A = 8, X = 9, R = 11 }

    private static NativeInputAction Button(string id, string source, NativeButton nativeButton) =>
        new(id, InputMappingConstants.ControllerPrefix + source, Button: (int)nativeButton);

    private static void AddAxis(List<NativeInputAction> actions, string id, int axis,
        string negativeSource, string positiveSource)
    {
        actions.Add(new(id + "Decrease", InputMappingConstants.ControllerPrefix + negativeSource, Axis: axis, Direction: -1));
        actions.Add(new(id + "Increase", InputMappingConstants.ControllerPrefix + positiveSource, Axis: axis));
    }

    internal static IReadOnlyList<NativeInputAction> Actions(MachineModel model, Emulator core, int port)
    {
        if (port < ControllerConstants.FirstPort || port >= ControllerConstants.PortCount)
            return Array.Empty<NativeInputAction>();
        var actions = new List<NativeInputAction>();
        if (core == Emulator.EmuSCV)
        {
            actions.AddRange([
                Button("Up", "Up", NativeButton.Up), Button("Down", "Down", NativeButton.Down),
                Button("Left", "Left", NativeButton.Left), Button("Right", "Right", NativeButton.Right),
                Button("FireLeft", "B", NativeButton.B), Button("FireRight", "A", NativeButton.A)]);
            if (port == ControllerConstants.FirstPort)
            {
                for (var digit = 0; digit < KeyboardConstants.DigitCount; digit++)
                    actions.Add(new(InputMappingConstants.KeypadPrefix + digit,
                        InputMappingConstants.KeyboardPrefix + ((EmulationKey)((int)EmulationKey.Numpad0 + digit)),
                        Key: (EmulationKey)((int)EmulationKey.D0 + digit)));
                actions.AddRange([
                    new("Enter", "Keyboard:Return", Key: EmulationKey.Return),
                    new("Clear", "Keyboard:Backspace", Key: EmulationKey.Backspace),
                    new("Power", "Keyboard:P", Key: EmulationKey.EpochPower),
                    new("Reset", "Keyboard:R", Key: EmulationKey.EpochReset)]);
            }
            return actions;
        }
        actions.AddRange([Button("LeverLeft", "Left", NativeButton.Left), Button("LeverRight", "Right", NativeButton.Right)]);
        // PD777 polls these shared console buttons from both host pads.
        actions.AddRange([
                Button("Push1", "X", NativeButton.X), Button("Push2", "B", NativeButton.B),
                Button("Push3", "Y", NativeButton.Y), Button("Push4", "A", NativeButton.A),
                Button("Select", "Select", NativeButton.Select), Button("Start", "Start", NativeButton.Start), Button("Aux", "R", NativeButton.R),
                Button("CourseIncrease", "Up", NativeButton.Up), Button("CourseDecrease", "Down", NativeButton.Down)]);
        if (port == ControllerConstants.FirstPort)
        {
            AddAxis(actions, "GunX", InputMappingConstants.RightX, "RightStickLeft", "RightStickRight");
            AddAxis(actions, "GunY", InputMappingConstants.RightY, "RightStickUp", "RightStickDown");
            actions.Add(new("GunFire", "Mouse:Left", CarrierMask: InputConstants.GunTriggerMask));
            actions.Add(new("GunReload", "Mouse:Right", CarrierMask: InputConstants.GunReloadMask));
        }
        if (model != MachineModel.CassetteVisionJr)
        {
            AddAxis(actions, port == ControllerConstants.FirstPort ? "Paddle2" : "Paddle3",
                InputMappingConstants.LeftX, "LeftStickLeft", "LeftStickRight");
            AddAxis(actions, port == ControllerConstants.FirstPort ? "Paddle1" : "Paddle4",
                InputMappingConstants.LeftY, "LeftStickUp", "LeftStickDown");
        }
        return actions;
    }
}
