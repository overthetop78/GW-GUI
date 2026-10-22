using CommonControllerFunctions = GWGUI.Emulation.Snk.Common.Functions.ControllerFunctions;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Dictionaries;

namespace GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions;
internal static class NativeInputMappingFunctions
{
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, MachineConfiguration configuration, int controllerPortCount)
    {
        var input = configuration.Input;
        var model = configuration.Model;
        var controllers = new EmulationControllerState[controllerPortCount];
        for (var port = ControllerConstants.FirstPort; port < controllers.Length; port++)
        {
            var binding = input.Controllers?.FirstOrDefault(item => item.Port == port) ?? new ControllerBinding(port, InputSettingsFunctions.DefaultPeripheral(model));
            var source = CommonControllerFunctions.ResolveController(binding.DeviceId, snapshot.Controllers, port);
            uint buttons = 0;
            float horizontal = 0, vertical = 0;
            foreach (var action in ControllerProtocolDictionary.Actions(model, binding.Peripheral))
            {
                var mapping = binding.Mappings?.GetValueOrDefault(action.Id) ?? string.Empty;
                if (string.IsNullOrWhiteSpace(mapping))
                    continue;
                var physical = CommonControllerFunctions.ResolveSourceController(mapping, snapshot.Controllers, source);
                if (CommonControllerFunctions.IsMappedInputPressed(mapping, physical, snapshot))
                    buttons |= action.ButtonMask;
                var magnitude = CommonControllerFunctions.MappedInputMagnitude(mapping, physical, snapshot, binding.DeadZonePercent);
                switch (action.Command)
                {
                    case ControllerCommand.Left:
                        horizontal -= magnitude;
                        break;
                    case ControllerCommand.Right:
                        horizontal += magnitude;
                        break;
                    case ControllerCommand.Up:
                        vertical -= magnitude;
                        break;
                    case ControllerCommand.Down:
                        vertical += magnitude;
                        break;
                }
            }

            controllers[port] = EmulationControllerState.Empty with
            {
                DeviceId = source.DeviceId,
                Buttons = buttons,
                LeftX = CommonControllerFunctions.NormalizedAxis(horizontal),
                LeftY = CommonControllerFunctions.NormalizedAxis(vertical)
            };
        }

        return snapshot with
        {
            Keys = EmulationInputSnapshot.Empty.Keys,
            Pointer = EmulationInputSnapshot.Empty.Pointer,
            Controllers = controllers
        };
    }


}
