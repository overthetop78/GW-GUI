using GWGUI.Emulation.Sharp.Emulators.Common.Interop.Dictionaries;
namespace GWGUI.Emulation.Sharp.Emulators.Common.Interop.Functions;
internal static class NativeInputMappingFunctions
{
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, InputConfiguration input, MachineModel model, Emulator core)
    {
        var controllers = new EmulationControllerState[ConfigurationFunctions.ControllerPortCount(model)];
        for (var port = ControllerConstants.FirstPort; port < controllers.Length; port++)
        {
            var binding = input.Controllers?.FirstOrDefault(item => item.Port == port) ?? new ControllerBinding(port, InputSettingsFunctions.DefaultPeripheral(model));
            var source = EmulationInputMappingFunctions.ResolveController(binding.DeviceId, snapshot.Controllers, port);
            uint buttons = 0;
            float horizontal = 0, vertical = 0, throttle = 0;
            foreach (var action in ControllerProtocolDictionary.Actions(model, binding.Peripheral))
            {
                var mapping = binding.Mappings?.GetValueOrDefault(action.Id) ?? string.Empty;
                if (string.IsNullOrWhiteSpace(mapping))
                    continue;
                var physical = EmulationInputMappingFunctions.ResolveSourceController(mapping, snapshot.Controllers, source);
                if (EmulationInputMappingFunctions.IsMappedInputPressed(mapping, physical, snapshot))
                    buttons |= action.ButtonMask;
                var magnitude = EmulationInputMappingFunctions.MappedInputMagnitude(mapping, physical, snapshot, binding.DeadZonePercent);
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
                    case ControllerCommand.ThrottleIncrease:
                        throttle -= magnitude;
                        break;
                    case ControllerCommand.ThrottleDecrease:
                        throttle += magnitude;
                        break;
                }
            }

            controllers[port] = EmulationControllerState.Empty with
            {
                DeviceId = source.DeviceId,
                Buttons = buttons,
                LeftX = EmulationInputMappingFunctions.NormalizedAxis(horizontal),
                LeftY = EmulationInputMappingFunctions.NormalizedAxis(vertical),
                RightY = EmulationInputMappingFunctions.NormalizedAxis(throttle)
            };
        }

        var pointer = EmulationInputSnapshot.Empty.Pointer;
        if (core == Emulator.Px68k)
        {
            var physical = EmulationInputMappingFunctions.ResolveController(null, snapshot.Controllers, ControllerConstants.FirstPort);
            var left = input.Mouse?.GetValueOrDefault(InputMappingConstants.MouseLeft, InputMappingConstants.MousePrefix + InputMappingConstants.MouseLeft) ?? InputMappingConstants.MousePrefix + InputMappingConstants.MouseLeft;
            var right = input.Mouse?.GetValueOrDefault(InputMappingConstants.MouseRight, InputMappingConstants.MousePrefix + InputMappingConstants.MouseRight) ?? InputMappingConstants.MousePrefix + InputMappingConstants.MouseRight;
            pointer = pointer with
            {
                DeltaX = Scale(snapshot.Pointer.DeltaX, input.MouseSpeedPercent),
                DeltaY = Scale(snapshot.Pointer.DeltaY, input.MouseSpeedPercent),
                Left = EmulationInputMappingFunctions.IsMappedInputPressed(left, EmulationInputMappingFunctions.ResolveSourceController(left, snapshot.Controllers, physical), snapshot),
                Right = EmulationInputMappingFunctions.IsMappedInputPressed(right, EmulationInputMappingFunctions.ResolveSourceController(right, snapshot.Controllers, physical), snapshot)
            };
        }

        return snapshot with
        {
            Keys = KeyboardDictionary.Apply(snapshot, input, model),
            Pointer = pointer,
            Controllers = controllers
        };
    }

    private static int Scale(int value, int percent) => (int)Math.Clamp(Math.Round((double)value * percent / InputMappingConstants.PercentScale), int.MinValue, int.MaxValue);

}
