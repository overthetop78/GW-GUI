using CommonControllerFunctions = GWGUI.Emulation.Mattel.Common.Functions.ControllerFunctions;
using GWGUI.Emulation.Mattel.Emulators.Common.Interop.Dictionaries;
namespace GWGUI.Emulation.Mattel.Emulators.Common.Interop.Functions;

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
            var source = CommonControllerFunctions.ResolveController(binding.DeviceId, snapshot.Controllers, port);
            uint buttons = 0;
            float horizontal = 0, vertical = 0;
            foreach (var action in ControllerProtocolDictionary.Actions(model, binding.Peripheral))
            {
                var mapping = binding.Mappings?.GetValueOrDefault(action.Id) ?? string.Empty;
                if (string.IsNullOrWhiteSpace(mapping)) continue;
                var physical = CommonControllerFunctions.ResolveSourceController(mapping, snapshot.Controllers, source);
                if (CommonControllerFunctions.IsMappedInputPressed(mapping, physical, snapshot)) buttons |= action.ButtonMask;
                var magnitude = CommonControllerFunctions.MappedInputMagnitude(mapping, physical, snapshot, binding.DeadZonePercent);
                switch (action.Id)
                {
                    case "Left": horizontal -= magnitude; break;
                    case "Right": horizontal += magnitude; break;
                    case "Up": vertical -= magnitude; break;
                    case "Down": vertical += magnitude; break;
                }
            }
            controllers[port] = EmulationControllerState.Empty with { DeviceId = source.DeviceId, Buttons = buttons,
                LeftX = CommonControllerFunctions.NormalizedAxis(horizontal), LeftY = CommonControllerFunctions.NormalizedAxis(vertical) };

        }
        var keys = new HashSet<EmulationKey>();
        return snapshot with { Keys = keys, Pointer = pointer, Controllers = controllers };
    }
}
