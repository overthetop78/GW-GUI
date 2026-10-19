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
                if (EmulationInputMappingFunctions.IsMappedInputPressed(mapping, physical, snapshot)) buttons |= 1u << action.Button;
            }
            controllers[port] = source with { Buttons = buttons, LeftX = 0, LeftY = 0, RightX = 0, RightY = 0 };
        }
        return snapshot with { Keys = new HashSet<EmulationKey>(), Controllers = controllers };
    }
}
