using GWGUI.Emulation.Epoch.Common.Machines.Common.Constants;

namespace GWGUI.Emulation.Epoch.Common.Machines.Common.Dictionaries;

internal sealed record NativeInputAction(string Id,
    int Button = InputMappingConstants.Unassigned, EmulationKey? Key = null,
    int Axis = InputMappingConstants.Unassigned, int Direction = 1, uint CarrierMask = 0)
{
    internal string DefaultBinding => string.Empty;

    internal InputBindingDefinition Definition => new(Id,
        Id is "Up" or "Down" or "Left" or "Right" ? "Emulation.Controller.Action." + Id : InputMappingConstants.ResourcePrefix + Id, DefaultBinding,
        Id.StartsWith(InputMappingConstants.KeypadPrefix, StringComparison.Ordinal) ? Id[InputMappingConstants.KeypadPrefix.Length..] : null);
}

internal static class ControllerProtocolDictionary
{
    private enum NativeButton { B = 0, Y = 1, Select = 2, Start = 3, Up = 4, Down = 5, Left = 6, Right = 7, A = 8, X = 9, R = 11 }

    private static NativeInputAction Button(string id, NativeButton nativeButton) => new(id, Button: (int)nativeButton);

    private static void AddAxis(List<NativeInputAction> actions, string id, int axis)
    {
        actions.Add(new(id + "Decrease", Axis: axis, Direction: -1));
        actions.Add(new(id + "Increase", Axis: axis));
    }

    internal static IReadOnlyList<NativeInputAction> Actions(MachineModel model, Emulator core, int port)
    {
        if (port < ControllerConstants.FirstPort || port >= ControllerConstants.PortCount)
            return Array.Empty<NativeInputAction>();
        var actions = new List<NativeInputAction>();
        if (core == Emulator.EmuSCV)
        {
            actions.AddRange([
                Button("Up", NativeButton.Up), Button("Down", NativeButton.Down),
                Button("Left", NativeButton.Left), Button("Right", NativeButton.Right),
                Button("FireLeft", NativeButton.B), Button("FireRight", NativeButton.A)]);
            if (port == ControllerConstants.FirstPort)
            {
                for (var digit = 0; digit < KeyboardConstants.DigitCount; digit++)
                    actions.Add(new(InputMappingConstants.KeypadPrefix + digit, Key: (EmulationKey)((int)EmulationKey.D0 + digit)));
                actions.AddRange([new("Enter", Key: EmulationKey.Return), new("Clear", Key: EmulationKey.Backspace), new("Power", Key: EmulationKey.EpochPower), new("Reset", Key: EmulationKey.EpochReset)]);
            }
            return actions;
        }
        actions.AddRange([Button("LeverLeft", NativeButton.Left), Button("LeverRight", NativeButton.Right)]);
        // PD777 polls these shared console buttons from both host pads.
        actions.AddRange([
                Button("Push1", NativeButton.X), Button("Push2", NativeButton.B),
                Button("Push3", NativeButton.Y), Button("Push4", NativeButton.A),
                Button("Select", NativeButton.Select), Button("Start", NativeButton.Start), Button("Aux", NativeButton.R),
                Button("CourseIncrease", NativeButton.Up), Button("CourseDecrease", NativeButton.Down)]);
        if (port == ControllerConstants.FirstPort)
        {
            AddAxis(actions, "GunX", InputMappingConstants.RightX);
            AddAxis(actions, "GunY", InputMappingConstants.RightY);
            actions.Add(new("GunFire", CarrierMask: InputConstants.GunTriggerMask));
            actions.Add(new("GunReload", CarrierMask: InputConstants.GunReloadMask));
        }
        if (model != MachineModel.CassetteVisionJr)
        {
            AddAxis(actions, port == ControllerConstants.FirstPort ? "Paddle2" : "Paddle3", InputMappingConstants.LeftX);
            AddAxis(actions, port == ControllerConstants.FirstPort ? "Paddle1" : "Paddle4", InputMappingConstants.LeftY);
        }
        return actions;
    }
}
