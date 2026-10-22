using CommonControllerFunctions = GWGUI.Emulation.Sinclair.Common.Functions.ControllerFunctions;
using GWGUI.Emulation.Sinclair.Emulators.Common.Interop.Dictionaries;

namespace GWGUI.Emulation.Sinclair.Emulators.Common.Interop.Functions;
internal static class NativeInputMappingFunctions
{
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, InputConfiguration input, MachineModel model, Emulator core)
    {
        var controllers = new EmulationControllerState[ConfigurationFunctions.ControllerPortCount(model)];
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

        var pointer = EmulationInputSnapshot.Empty.Pointer;
        if (core == Emulator.Fuse)
        {
            var physical = CommonControllerFunctions.ResolveController(null, snapshot.Controllers, ControllerConstants.FirstPort);
            var left = input.Mouse?.GetValueOrDefault(InputMappingConstants.MouseLeft, InputMappingConstants.MousePrefix + InputMappingConstants.MouseLeft) ?? InputMappingConstants.MousePrefix + InputMappingConstants.MouseLeft;
            var right = input.Mouse?.GetValueOrDefault(InputMappingConstants.MouseRight, InputMappingConstants.MousePrefix + InputMappingConstants.MouseRight) ?? InputMappingConstants.MousePrefix + InputMappingConstants.MouseRight;
            pointer = pointer with
            {
                DeltaX = Scale(snapshot.Pointer.DeltaX, input.MouseSpeedPercent),
                DeltaY = Scale(snapshot.Pointer.DeltaY, input.MouseSpeedPercent),
                Left = CommonControllerFunctions.IsMappedInputPressed(left, CommonControllerFunctions.ResolveSourceController(left, snapshot.Controllers, physical), snapshot),
                Right = CommonControllerFunctions.IsMappedInputPressed(right, CommonControllerFunctions.ResolveSourceController(right, snapshot.Controllers, physical), snapshot)
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
