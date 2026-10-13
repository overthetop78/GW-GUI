using GWGUI.Emulation.Philips.Emulators.Common.Interop.Dictionaries;
using GWGUI.Emulation.Philips.Common.Machines.P2000.Dictionaries;

namespace GWGUI.Emulation.Philips.Emulators.Common.Interop.Functions;

internal static class NativeInputMappingFunctions
{
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, InputConfiguration input, MachineModel model)
    {
        var controllers = new EmulationControllerState[ConfigurationFunctions.ControllerPortCount(model)];
        var pointer = snapshot.Pointer with { Left = false, Right = false, Middle = false,
            ExtendedButton1 = false, ExtendedButton2 = false };
        for (var port = ControllerConstants.FirstPort; port < controllers.Length; port++)
        {
            var binding = input.Controllers?.FirstOrDefault(item => item.Port == port) ?? new ControllerBinding(port, InputSettingsFunctions.DefaultPeripheral(model));
            var source = EmulationInputMappingFunctions.ResolveController(binding.DeviceId, snapshot.Controllers, port);
            uint buttons = 0;
            float horizontal = 0, vertical = 0;
            foreach (var action in ControllerProtocolDictionary.Actions(model, binding.Peripheral))
            {
                var mapping = binding.Mappings?.GetValueOrDefault(action.Id) ?? string.Empty;
                if (string.IsNullOrWhiteSpace(mapping)) continue;
                var physical = EmulationInputMappingFunctions.ResolveSourceController(mapping, snapshot.Controllers, source);
                if (IsPressed(mapping, physical, snapshot)) buttons |= action.ButtonMask;
                var magnitude = Magnitude(mapping, physical, snapshot, binding.DeadZonePercent);
                switch (action.Id)
                {
                    case "Left": horizontal -= magnitude; break;
                    case "Right": horizontal += magnitude; break;
                    case "Up": vertical -= magnitude; break;
                    case "Down": vertical += magnitude; break;
                }
            }
            controllers[port] = EmulationControllerState.Empty with { DeviceId = source.DeviceId, Buttons = buttons,
                LeftX = Axis(horizontal), LeftY = Axis(vertical) };
            if (binding.Peripheral == PeripheralCategory.Mouse && port == ControllerConstants.FirstPort)
                pointer = pointer with { Left = (buttons & 1) != 0, Right = (buttons & 2) != 0, Middle = (buttons & 4) != 0 };
        }
        var keys = ConfigurationFunctions.Family(model) == MachineFamily.P2000 ? P2000KeyboardDictionary.Apply(snapshot.Keys, input.Keyboard) : new HashSet<EmulationKey>(snapshot.Keys);
        return snapshot with { Keys = keys, Pointer = pointer, Controllers = controllers };
    }
    private static short Axis(float value) => (short)(Math.Clamp(value, -1f, 1f) * short.MaxValue);
    private static float Magnitude(string mapping, EmulationControllerState controller, EmulationInputSnapshot snapshot, int deadZone)
    {
        if (mapping.Contains('+') && !mapping.EndsWith('+'))
        {
            var combined = EmulationInputMappingFunctions.InputSourcePressure(mapping, controller, snapshot);
            return combined <= deadZone / 100f ? 0f : Math.Clamp(combined, 0f, 1f);
        }
        if (!mapping.StartsWith(InputMappingConstants.ControllerPrefix, StringComparison.OrdinalIgnoreCase))
            return IsPressed(mapping, controller, snapshot) ? 1f : 0f;
        var value = EmulationInputMappingFunctions.ControllerSourcePressure(mapping, controller);
        return value <= deadZone / 100f ? 0f : Math.Clamp(value, 0f, 1f);
    }
    private static bool IsPressed(string mapping, EmulationControllerState controller, EmulationInputSnapshot snapshot)
    {
        if (mapping.Contains('+' ) && !mapping.EndsWith('+')) return EmulationInputMappingFunctions.InputSourcePressure(mapping, controller, snapshot) > .5f;
        if (mapping.StartsWith(InputMappingConstants.KeyboardPrefix, StringComparison.OrdinalIgnoreCase))
            return Enum.TryParse<EmulationKey>(mapping[InputMappingConstants.KeyboardPrefix.Length..], out var key) && snapshot.Keys.Contains(key);
        if (mapping.StartsWith(InputMappingConstants.MousePrefix, StringComparison.OrdinalIgnoreCase))
            return mapping[InputMappingConstants.MousePrefix.Length..] switch
            {
                "Left" => snapshot.Pointer.Left, "Right" => snapshot.Pointer.Right, "Middle" => snapshot.Pointer.Middle,
                "XButton1" => snapshot.Pointer.ExtendedButton1, "XButton2" => snapshot.Pointer.ExtendedButton2,
                "WheelUp" => snapshot.Pointer.Wheel > 0, "WheelDown" => snapshot.Pointer.Wheel < 0,
                "WheelLeft" => snapshot.Pointer.HorizontalWheel < 0, "WheelRight" => snapshot.Pointer.HorizontalWheel > 0,
                _ => false
            };
        return mapping.StartsWith(InputMappingConstants.ControllerPrefix, StringComparison.OrdinalIgnoreCase) && EmulationInputMappingFunctions.IsControllerSourcePressed(mapping, controller);
    }
}
