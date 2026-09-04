using GWGUI.App.Services.Input.GameInput;
using GWGUI.Emulation;

namespace GWGUI.App.Functions.Input.Controllers;

internal static class ControllerAnalogDeadZoneFunctions
{
    internal static EmulationControllerState ApplyConfigured(EmulationControllerState state) =>
        Apply(state, ControllerAnalogDeadZoneProfileStore.Get(state.DeviceId));

    internal static GameInputLiveState ApplyConfigured(GameInputLiveState state) =>
        Apply(state, ControllerAnalogDeadZoneProfileStore.Get(state.DeviceId));

    internal static EmulationControllerState Apply(
        EmulationControllerState state,
        ControllerAnalogDeadZoneProfile profile)
    {
        profile = profile.Normalize();
        var left = ApplyStick(state.LeftX / (float)short.MaxValue,
            state.LeftY / (float)short.MaxValue, profile);
        var right = ApplyStick(state.RightX / (float)short.MaxValue,
            state.RightY / (float)short.MaxValue, profile);
        var leftTrigger = ApplyTrigger(state.LeftTrigger / (float)short.MaxValue, profile.TriggerPercent);
        var rightTrigger = ApplyTrigger(state.RightTrigger / (float)short.MaxValue, profile.TriggerPercent);
        var buttons = ApplyTriggerButtons(state.Buttons, leftTrigger, rightTrigger, profile.TriggerPercent);
        return state with
        {
            Buttons = buttons,
            LeftX = ToShort(left.X),
            LeftY = ToShort(left.Y),
            RightX = ToShort(right.X),
            RightY = ToShort(right.Y),
            LeftTrigger = ToShort(leftTrigger),
            RightTrigger = ToShort(rightTrigger)
        };
    }

    internal static GameInputLiveState Apply(
        GameInputLiveState state,
        ControllerAnalogDeadZoneProfile profile)
    {
        if (state.Gamepad is not { } gamepad) return state;
        profile = profile.Normalize();
        var left = ApplyStick(gamepad.LeftThumbstickX, gamepad.LeftThumbstickY, profile);
        var right = ApplyStick(gamepad.RightThumbstickX, gamepad.RightThumbstickY, profile);
        var leftTrigger = ApplyTrigger(gamepad.LeftTrigger, profile.TriggerPercent);
        var rightTrigger = ApplyTrigger(gamepad.RightTrigger, profile.TriggerPercent);
        if (profile.TriggerPercent > 0)
        {
            gamepad.Buttons &= ~(GameInputGamepadButtons.LeftTriggerButton |
                                 GameInputGamepadButtons.RightTriggerButton);
            if (leftTrigger > 0f) gamepad.Buttons |= GameInputGamepadButtons.LeftTriggerButton;
            if (rightTrigger > 0f) gamepad.Buttons |= GameInputGamepadButtons.RightTriggerButton;
        }
        gamepad.LeftThumbstickX = left.X;
        gamepad.LeftThumbstickY = left.Y;
        gamepad.RightThumbstickX = right.X;
        gamepad.RightThumbstickY = right.Y;
        gamepad.LeftTrigger = leftTrigger;
        gamepad.RightTrigger = rightTrigger;
        return state with { Gamepad = gamepad };
    }

    private static (float X, float Y) ApplyStick(
        float x,
        float y,
        ControllerAnalogDeadZoneProfile profile)
    {
        x = Math.Clamp(x, -1f, 1f);
        y = Math.Clamp(y, -1f, 1f);
        var magnitude = MathF.Sqrt(x * x + y * y);
        var inner = profile.StickPercent / 100f;
        if (magnitude <= inner || magnitude <= float.Epsilon) return (0f, 0f);
        var outer = profile.OuterPercent / 100f;
        var usable = Math.Max(.001f, 1f - inner - outer);
        var scaledMagnitude = Math.Clamp((magnitude - inner) / usable, 0f, 1f);
        var scale = scaledMagnitude / magnitude;
        return (Math.Clamp(x * scale, -1f, 1f), Math.Clamp(y * scale, -1f, 1f));
    }

    private static float ApplyTrigger(float value, int deadZonePercent)
    {
        value = Math.Clamp(value, 0f, 1f);
        var deadZone = deadZonePercent / 100f;
        return value <= deadZone + .0001f
            ? 0f
            : Math.Clamp((value - deadZone) / (1f - deadZone), 0f, 1f);
    }

    private static uint ApplyTriggerButtons(uint buttons, float left, float right, int deadZonePercent)
    {
        if (deadZonePercent <= 0) return buttons;
        const uint triggerMask = (1u << 12) | (1u << 13);
        buttons &= ~triggerMask;
        if (left > 0f) buttons |= 1u << 12;
        if (right > 0f) buttons |= 1u << 13;
        return buttons;
    }

    private static short ToShort(float value) =>
        (short)Math.Round(Math.Clamp(value, -1f, 1f) * short.MaxValue);
}
