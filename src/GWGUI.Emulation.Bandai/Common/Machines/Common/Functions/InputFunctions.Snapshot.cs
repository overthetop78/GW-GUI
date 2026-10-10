namespace GWGUI.Emulation.Bandai.Common.Machines.Common.Functions;

internal static class InputSnapshotFunctions
{
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot,
        InputConfiguration? configuration, MachineModel model)
    {
        if (configuration?.Controllers is not { Count: > 0 }) return InputFunctions.Freeze(snapshot with { Controllers = [] });
        var controllers = Enumerable.Repeat(EmulationControllerState.Empty, ControllerConstants.PortCount).ToArray();
        var targets = InputSnapshotDictionary.ForModel(model);
        foreach (var binding in configuration.Controllers)
        {
            if (binding.Port != ControllerConstants.FirstPort || binding.Peripheral == PeripheralCategory.None) continue;
            var source = EmulationInputMappingFunctions.ResolveController(binding.DeviceId,
                snapshot.Controllers, binding.Port);
            if (binding.Mappings is not { Count: > 0 })
            {
                controllers[binding.Port] = EmulationControllerState.Empty;
                continue;
            }
            uint buttons = 0;
            foreach (var mapping in binding.Mappings)
            {
                if (!targets.TryGetValue(mapping.Key, out var target)) continue;
                var controller = EmulationInputMappingFunctions.ResolveSourceController(mapping.Value,
                    snapshot.Controllers, source);
                if (IsPressed(mapping.Value, controller, snapshot)) buttons |= 1u << target;
            }
            controllers[binding.Port] = source with { Buttons = buttons };
        }
        return snapshot with { Controllers = controllers };
    }

    private static bool IsPressed(string source, EmulationControllerState controller,
        EmulationInputSnapshot snapshot)
    {
        if (source.StartsWith(InputSnapshotFunctionsConstants.Keyboard, StringComparison.OrdinalIgnoreCase))
            return Enum.TryParse<EmulationKey>(source[InputSnapshotFunctionsConstants.Keyboard.Length..], true,
                out var key) && snapshot.Keys.Contains(key);
        if (source.StartsWith(InputSnapshotFunctionsConstants.Mouse, StringComparison.OrdinalIgnoreCase))
            return source[InputSnapshotFunctionsConstants.Mouse.Length..] switch
            {
                InputSnapshotFunctionsConstants.Left => snapshot.Pointer.Left,
                InputSnapshotFunctionsConstants.Right => snapshot.Pointer.Right,
                InputSnapshotFunctionsConstants.Middle => snapshot.Pointer.Middle,
                InputSnapshotFunctionsConstants.XButton1 => snapshot.Pointer.ExtendedButton1,
                InputSnapshotFunctionsConstants.XButton2 => snapshot.Pointer.ExtendedButton2,
                InputSnapshotFunctionsConstants.WheelUp => snapshot.Pointer.Wheel > 0,
                InputSnapshotFunctionsConstants.WheelDown => snapshot.Pointer.Wheel < 0,
                InputSnapshotFunctionsConstants.WheelLeft => snapshot.Pointer.HorizontalWheel < 0,
                InputSnapshotFunctionsConstants.WheelRight => snapshot.Pointer.HorizontalWheel > 0,
                _ => false
            };
        return EmulationInputMappingFunctions.IsControllerSourcePressed(source, controller);
    }
}
