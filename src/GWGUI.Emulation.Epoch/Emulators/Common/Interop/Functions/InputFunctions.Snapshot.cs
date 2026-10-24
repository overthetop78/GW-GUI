using CommonControllerFunctions = GWGUI.Emulation.Epoch.Common.Functions.ControllerFunctions;
using GWGUI.Emulation.Epoch.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Epoch.Emulators.Common.Interop.Dictionaries;
using GWGUI.Emulation.Epoch.Common.Machines.Common.Constants;

namespace GWGUI.Emulation.Epoch.Emulators.Common.Interop.Functions;

internal static class NativeInputSnapshotFunctions
{
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, InputConfiguration input, MachineModel model, Emulator core)
    {
        // Console keys are reconstructed from their assignments. An old host key must
        // not survive a reassignment and reach the native keyboard callback.
        var keys = new HashSet<EmulationKey>();
        var controllers = new EmulationControllerState[ControllerConstants.PortCount];
        for (var port = ControllerConstants.FirstPort; port < controllers.Length; port++)
        {
            var binding = input.Controllers?.FirstOrDefault(item => item.Port == port) ?? new ControllerBinding(port, PeripheralCategory.Standard);
            var source = CommonControllerFunctions.ResolveController(binding.DeviceId, snapshot.Controllers, port);
            uint buttons = 0;
            var axes = new float[InputMappingConstants.AxisCount];
            foreach (var action in ControllerProtocolDictionary.Actions(model, core, port))
            {
                var mapping = binding.Mappings?.GetValueOrDefault(action.Id, action.DefaultBinding) ?? action.DefaultBinding;
                if (action.Key is EmulationKey key && input.KeyboardBindings?.TryGetValue(key.ToString(), out var keyboardSource) == true)
                    mapping = string.IsNullOrEmpty(keyboardSource) ? string.Empty : InputMappingConstants.KeyboardPrefix + keyboardSource;
                if (string.IsNullOrWhiteSpace(mapping)) continue;
                var physical = CommonControllerFunctions.ResolveSourceController(mapping, snapshot.Controllers, source);
                if (action.Id.StartsWith(InputMappingConstants.GunPrefix, StringComparison.Ordinal))
                    buttons |= InputConstants.GunEnabledMask;
                if (action.Axis >= 0)
                {
                    axes[action.Axis] += action.Direction * AxisValue(mapping, physical, snapshot, binding.DeadZonePercent);
                    continue;
                }
                var translatedShortcut = action.Key is EmulationKey.EpochPower or EmulationKey.EpochReset && snapshot.Keys.Contains(action.Key.Value);
                if (!translatedShortcut && !CommonControllerFunctions.IsMappedInputPressed(mapping, physical, snapshot)) continue;
                if (action.Button >= 0) buttons |= 1u << action.Button;
                if (action.Key is EmulationKey target) keys.Add(target);
                buttons |= action.CarrierMask;
            }
            controllers[port] = source with
            {
                Buttons = buttons,
                LeftX = EncodeAxis(axes[InputMappingConstants.LeftX]), LeftY = EncodeAxis(axes[InputMappingConstants.LeftY]),
                RightX = EncodeAxis(axes[InputMappingConstants.RightX]), RightY = EncodeAxis(axes[InputMappingConstants.RightY])
            };
        }
        return snapshot with { Keys = keys, Controllers = controllers };
    }

    private static short EncodeAxis(float value) => (short)Math.Round(Math.Clamp(value, -1f, 1f) * InputMappingConstants.AxisMaximum);

    private static float AxisValue(string mapping, EmulationControllerState controller, EmulationInputSnapshot snapshot, int deadZonePercent)
    {
        if (mapping.Contains('+') && !mapping.EndsWith('+'))
        {
            var combined = CommonControllerFunctions.InputSourcePressure(mapping, controller, snapshot);
            var combinedDeadZone = Math.Clamp(deadZonePercent, ControllerConstants.MinimumDeadZonePercent, ControllerConstants.MaximumDeadZonePercent) / InputMappingConstants.PercentMaximum;
            return combined <= combinedDeadZone || combinedDeadZone >= 1f ? 0f : (combined - combinedDeadZone) / (1f - combinedDeadZone);
        }
        if (!mapping.StartsWith(InputMappingConstants.ControllerPrefix, StringComparison.OrdinalIgnoreCase))
            return CommonControllerFunctions.IsMappedInputPressed(mapping, controller, snapshot) ? 1f : 0f;
        var value = CommonControllerFunctions.ControllerSourceValue(mapping, controller);
        var name = mapping[(mapping.LastIndexOf(':') + 1)..];
        var custom = controller.Controls.ContainsKey(name) || ((name.EndsWith('+') || name.EndsWith('-')) && controller.Controls.ContainsKey(name[..^1]));
        var analog = custom || name is "LeftStickLeft" or "LeftStickRight" or "LeftStickUp" or "LeftStickDown" or "RightStickLeft" or "RightStickRight" or "RightStickUp" or "RightStickDown";
        if (analog && !custom) value /= InputMappingConstants.AxisMaximum;
        value = Math.Clamp(value, 0f, 1f);
        if (!analog) return value;
        var deadZone = Math.Clamp(deadZonePercent, ControllerConstants.MinimumDeadZonePercent, ControllerConstants.MaximumDeadZonePercent) / InputMappingConstants.PercentMaximum;
        return value <= deadZone || deadZone >= 1f ? 0f : (value - deadZone) / (1f - deadZone);
    }

}
