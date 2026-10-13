using GWGUI.Emulation.Fairchild.Emulators.Common.Interop.Dictionaries;
namespace GWGUI.Emulation.Fairchild.Emulators.Common.Interop.Functions;

internal static class NativeInputSnapshotFunctions
{
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, InputConfiguration input, MachineModel model, Emulator core)
    {
        var controllers = new EmulationControllerState[ControllerConstants.PortCount];
        for (var port = ControllerConstants.FirstPort; port < controllers.Length; port++)
        {
            var binding = input.Controllers?.FirstOrDefault(item => item.Port == port) ?? new ControllerBinding(port, PeripheralCategory.Standard);
            var source = EmulationInputMappingFunctions.ResolveController(binding.DeviceId, snapshot.Controllers, port);
            uint buttons = 0;
            foreach (var action in ControllerProtocolDictionary.Actions(model, core, port))
            {
                var mapping = binding.Mappings?.GetValueOrDefault(action.Id, action.DefaultBinding) ?? action.DefaultBinding;
                if (string.IsNullOrWhiteSpace(mapping)) continue;
                var physical = EmulationInputMappingFunctions.ResolveSourceController(mapping, snapshot.Controllers, source);
                if (IsPressed(mapping, physical, snapshot)) buttons |= 1u << action.Button;
            }
            controllers[port] = source with { Buttons = buttons, LeftX = 0, LeftY = 0, RightX = 0, RightY = 0 };
        }
        return snapshot with { Keys = new HashSet<EmulationKey>(), Controllers = controllers };
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
