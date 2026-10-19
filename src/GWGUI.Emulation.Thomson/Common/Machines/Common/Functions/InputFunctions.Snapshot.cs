namespace GWGUI.Emulation.Thomson.Common.Machines.Common.Functions;
internal static class InputSnapshotFunctions
{
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, InputConfiguration input, MachineModel model, Emulator core)
    {
        var controllers = new EmulationControllerState[ControllerConstants.PortCount];
        for (var port = ControllerConstants.FirstPort; port < controllers.Length; port++)
        {
            var binding = input.Controllers?.FirstOrDefault(item => item.Port == port);
            if (binding is null || binding.Peripheral == PeripheralCategory.None)
            {
                controllers[port] = EmulationControllerState.Empty;
                continue;
            }

            var source = EmulationInputMappingFunctions.ResolveController(binding.DeviceId, snapshot.Controllers, port);
            uint buttons = 0;
            float horizontal = 0, vertical = 0;
            foreach (var command in ControllerCatalog.Commands(binding.Peripheral))
            {
                var mapping = binding.Mappings?.GetValueOrDefault(command.ToString()) ?? string.Empty;
                if (string.IsNullOrWhiteSpace(mapping))
                    continue;
                var physical = EmulationInputMappingFunctions.ResolveSourceController(mapping, snapshot.Controllers, source);
                if (command == ControllerCommand.Fire && EmulationInputMappingFunctions.IsMappedInputPressed(mapping, physical, snapshot))
                    buttons |= ControllerConstants.FireButtonMask;
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

        var physicalPointerSource = EmulationInputMappingFunctions.ResolveController(null, snapshot.Controllers, ControllerConstants.FirstPort);
        var left = input.Mouse?.GetValueOrDefault(InputMappingConstants.MouseLeft, InputMappingConstants.MousePrefix + InputMappingConstants.MouseLeft) ?? InputMappingConstants.MousePrefix + InputMappingConstants.MouseLeft;
        var right = input.Mouse?.GetValueOrDefault(InputMappingConstants.MouseRight, InputMappingConstants.MousePrefix + InputMappingConstants.MouseRight) ?? InputMappingConstants.MousePrefix + InputMappingConstants.MouseRight;
        var pointer = snapshot.Pointer with
        {
            DeltaX = Scale(snapshot.Pointer.DeltaX, input.MouseSpeedPercent),
            DeltaY = Scale(snapshot.Pointer.DeltaY, input.MouseSpeedPercent),
            Left = EmulationInputMappingFunctions.IsMappedInputPressed(left, EmulationInputMappingFunctions.ResolveSourceController(left, snapshot.Controllers, physicalPointerSource), snapshot),
            Right = EmulationInputMappingFunctions.IsMappedInputPressed(right, EmulationInputMappingFunctions.ResolveSourceController(right, snapshot.Controllers, physicalPointerSource), snapshot)
        };
        return snapshot with
        {
            Keys = KeyboardDictionary.Apply(snapshot, input, model),
            Pointer = pointer,
            Controllers = controllers
        };
    }

    private static int Scale(int value, int percent) => (int)Math.Clamp(Math.Round((double)value * percent / InputMappingConstants.PercentScale), int.MinValue, int.MaxValue);

}
