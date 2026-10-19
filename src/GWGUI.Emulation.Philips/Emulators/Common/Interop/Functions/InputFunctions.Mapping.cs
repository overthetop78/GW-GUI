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
                if (EmulationInputMappingFunctions.IsMappedInputPressed(mapping, physical, snapshot)) buttons |= action.ButtonMask;
                var magnitude = EmulationInputMappingFunctions.MappedInputMagnitude(mapping, physical, snapshot, binding.DeadZonePercent);
                switch (action.Id)
                {
                    case "Left": horizontal -= magnitude; break;
                    case "Right": horizontal += magnitude; break;
                    case "Up": vertical -= magnitude; break;
                    case "Down": vertical += magnitude; break;
                }
            }
            controllers[port] = EmulationControllerState.Empty with { DeviceId = source.DeviceId, Buttons = buttons,
                LeftX = EmulationInputMappingFunctions.NormalizedAxis(horizontal), LeftY = EmulationInputMappingFunctions.NormalizedAxis(vertical) };
            if (binding.Peripheral == PeripheralCategory.Mouse && port == ControllerConstants.FirstPort)
                pointer = pointer with { Left = (buttons & 1) != 0, Right = (buttons & 2) != 0, Middle = (buttons & 4) != 0 };
        }
        var keys = ConfigurationFunctions.Family(model) == MachineFamily.P2000 ? P2000KeyboardDictionary.Apply(snapshot, input.Keyboard) : new HashSet<EmulationKey>(snapshot.Keys);
        return snapshot with { Keys = keys, Pointer = pointer, Controllers = controllers };
    }
}
