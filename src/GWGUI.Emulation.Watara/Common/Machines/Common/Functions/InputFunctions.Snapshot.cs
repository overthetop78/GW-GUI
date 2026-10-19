using GWGUI.Emulation.Watara.Common.Machines.Common.Dictionaries;

namespace GWGUI.Emulation.Watara.Common.Machines.Common.Functions;
internal static class InputSnapshotFunctions
{
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, InputConfiguration input, MachineModel model, Emulator core)
    {
        var controllers = new EmulationControllerState[ControllerConstants.PortCount];
        for (var port = ControllerConstants.FirstPort; port < controllers.Length; port++)
        {
            var binding = input.Controllers?.FirstOrDefault(item => item.Port == port);
            if (binding is null)
            {
                controllers[port] = EmulationControllerState.Empty;
                continue;
            }

            var source = EmulationInputMappingFunctions.ResolveController(binding.DeviceId, snapshot.Controllers, port);
            uint buttons = 0;
            float horizontal = 0, vertical = 0;
            foreach (var command in ControllerCatalog.Commands)
            {
                var mapping = binding.Mappings?.GetValueOrDefault(command.ToString()) ?? string.Empty;
                if (string.IsNullOrWhiteSpace(mapping))
                    continue;
                var physical = EmulationInputMappingFunctions.ResolveSourceController(mapping, snapshot.Controllers, source);
                if (EmulationInputMappingFunctions.IsMappedInputPressed(mapping, physical, snapshot))
                    buttons |= (uint)(command switch
                    {
                        ControllerCommand.A => ControllerButtons.A,
                        ControllerCommand.B => ControllerButtons.B,
                        ControllerCommand.Start => ControllerButtons.Start,
                        ControllerCommand.Select => ControllerButtons.Select,
                        _ => ControllerButtons.None
                    });
                var magnitude = EmulationInputMappingFunctions.MappedInputMagnitude(mapping, physical, snapshot, binding.DeadZonePercent);
                switch (command)
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
                LeftX = EmulationInputMappingFunctions.NormalizedAxis(horizontal),
                LeftY = EmulationInputMappingFunctions.NormalizedAxis(vertical)
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