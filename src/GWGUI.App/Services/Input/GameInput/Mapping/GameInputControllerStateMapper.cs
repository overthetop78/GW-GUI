using GWGUI.App.Constants.Input.GameInput;
using GWGUI.Emulation;

namespace GWGUI.App.Services.Input.GameInput;

internal static class GameInputControllerStateMapper
{
    internal static EmulationControllerState MapGamepad(
        string deviceId,
        GameInputGamepadState gamepad,
        IReadOnlyDictionary<string, GameInputSystemButtons> systemButtons)
    {
        uint buttons = 0;
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.B, EmulationControllerButtonBit.B);
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.Y, EmulationControllerButtonBit.Y);
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.View, EmulationControllerButtonBit.View);
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.Menu, EmulationControllerButtonBit.Menu);
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.DPadUp, EmulationControllerButtonBit.DPadUp);
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.DPadDown, EmulationControllerButtonBit.DPadDown);
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.DPadLeft, EmulationControllerButtonBit.DPadLeft);
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.DPadRight, EmulationControllerButtonBit.DPadRight);
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.A, EmulationControllerButtonBit.A);
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.X, EmulationControllerButtonBit.X);
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.LeftShoulder, EmulationControllerButtonBit.LeftShoulder);
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.RightShoulder, EmulationControllerButtonBit.RightShoulder);
        if (gamepad.LeftTrigger > GameInputControllerMappingConstants.TriggerPressedThreshold ||
            gamepad.Buttons.HasFlag(GameInputGamepadButtons.LeftTriggerButton))
            buttons |= Bit(EmulationControllerButtonBit.LeftTrigger);
        if (gamepad.RightTrigger > GameInputControllerMappingConstants.TriggerPressedThreshold ||
            gamepad.Buttons.HasFlag(GameInputGamepadButtons.RightTriggerButton))
            buttons |= Bit(EmulationControllerButtonBit.RightTrigger);
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.LeftThumbstick, EmulationControllerButtonBit.LeftThumbstick);
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.RightThumbstick, EmulationControllerButtonBit.RightThumbstick);
        var system = systemButtons.GetValueOrDefault(deviceId);
        if (system.HasFlag(GameInputSystemButtons.Guide)) buttons |= Bit(EmulationControllerButtonBit.Guide);
        if (system.HasFlag(GameInputSystemButtons.Share)) buttons |= Bit(EmulationControllerButtonBit.Share);
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.PaddleLeft1, EmulationControllerButtonBit.PaddleLeft1);
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.PaddleLeft2, EmulationControllerButtonBit.PaddleLeft2);
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.PaddleRight1, EmulationControllerButtonBit.PaddleRight1);
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.PaddleRight2, EmulationControllerButtonBit.PaddleRight2);
        return new EmulationControllerState(buttons,
            Axis(gamepad.LeftThumbstickX), Axis(-gamepad.LeftThumbstickY),
            Axis(gamepad.RightThumbstickX), Axis(-gamepad.RightThumbstickY),
            Trigger(gamepad.LeftTrigger), Trigger(gamepad.RightTrigger)) { DeviceId = deviceId };
    }

    internal static unsafe EmulationControllerState MapController(
        string deviceId,
        IGameInputReading reading,
        IGameInputMapper? mapper,
        IReadOnlyDictionary<string, GameInputSystemButtons> systemButtons)
    {
        var buttonCount = checked((int)reading.GetControllerButtonCount());
        var buttonStates = new byte[buttonCount];
        fixed (byte* pointer = buttonStates)
            reading.GetControllerButtonState((uint)buttonCount, (IntPtr)pointer);
        var axisCount = checked((int)reading.GetControllerAxisCount());
        var axisStates = new float[axisCount];
        fixed (float* pointer = axisStates)
            reading.GetControllerAxisState((uint)axisCount, (IntPtr)pointer);
        var switchCount = checked((int)reading.GetControllerSwitchCount());
        var switchStates = new int[switchCount];
        fixed (int* pointer = switchStates)
            reading.GetControllerSwitchState((uint)switchCount, (IntPtr)pointer);

        var controls = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < buttonStates.Length; index++)
            controls[$"{GameInputControllerMappingConstants.ButtonControlPrefix}{index}"] = buttonStates[index] != 0 ? 1f : 0f;
        for (var index = 0; index < axisStates.Length; index++)
            controls[$"{GameInputControllerMappingConstants.AxisControlPrefix}{index}"] = axisStates[index];
        for (var index = 0; index < switchStates.Length; index++)
            controls[$"{GameInputControllerMappingConstants.SwitchControlPrefix}{index}"] = switchStates[index];

        if (mapper is null)
            return new EmulationControllerState(0, 0, 0, 0, 0, 0, 0)
                { DeviceId = deviceId, Controls = new EmulationControllerControls(controls) };

        var mapped = new GameInputGamepadState
        {
            LeftTrigger = ReadMappedAxis(mapper, GameInputGamepadAxes.LeftTrigger, axisStates, buttonStates, switchStates, 0f),
            RightTrigger = ReadMappedAxis(mapper, GameInputGamepadAxes.RightTrigger, axisStates, buttonStates, switchStates, 0f),
            LeftThumbstickX = ToThumbAxis(ReadMappedAxis(mapper, GameInputGamepadAxes.LeftThumbstickX, axisStates, buttonStates, switchStates, .5f)),
            LeftThumbstickY = ToThumbAxis(ReadMappedAxis(mapper, GameInputGamepadAxes.LeftThumbstickY, axisStates, buttonStates, switchStates, .5f)),
            RightThumbstickX = ToThumbAxis(ReadMappedAxis(mapper, GameInputGamepadAxes.RightThumbstickX, axisStates, buttonStates, switchStates, .5f)),
            RightThumbstickY = ToThumbAxis(ReadMappedAxis(mapper, GameInputGamepadAxes.RightThumbstickY, axisStates, buttonStates, switchStates, .5f))
        };
        foreach (var button in Enum.GetValues<GameInputGamepadButtons>())
            if (button != 0 && ReadMappedButton(mapper, button, axisStates, buttonStates, switchStates))
                mapped.Buttons |= button;
        return MapGamepad(deviceId, mapped, systemButtons) with
        {
            Controls = new EmulationControllerControls(controls)
        };
    }

    private static float ReadMappedAxis(IGameInputMapper mapper, GameInputGamepadAxes axis,
        float[] axes, byte[] buttons, int[] switches, float unmappedValue)
    {
        if (!mapper.GetGamepadAxisMappingInfo(axis, out var mapping)) return unmappedValue;
        var value = mapping.ControllerElementKind switch
        {
            GameInputElementKind.Axis when mapping.ControllerIndex < axes.Length => axes[mapping.ControllerIndex],
            GameInputElementKind.Button when mapping.ControllerIndex < buttons.Length => ReadButtonAxis(mapping, buttons),
            GameInputElementKind.Switch when mapping.ControllerIndex < switches.Length =>
                ReadSwitchAxis(switches[mapping.ControllerIndex], (int)mapping.ReferenceDirection),
            _ => 0f
        };
        return mapping.IsInverted ? 1f - value : value;
    }

    private static float ReadButtonAxis(GameInputAxisMapping mapping, byte[] buttons)
    {
        var maximum = buttons[mapping.ControllerIndex] != 0;
        if (!mapping.FromTwoButtons || mapping.ButtonMinIndexValue >= buttons.Length)
            return maximum ? 1f : 0f;
        var minimum = buttons[mapping.ButtonMinIndexValue] != 0;
        return maximum == minimum ? .5f : maximum ? 1f : 0f;
    }

    private static float ReadSwitchAxis(int position, int referenceDirection)
    {
        if (position == 0) return .5f;
        if (position == referenceDirection) return 1f;
        var opposite = ((referenceDirection - GameInputControllerMappingConstants.FirstSwitchDirection +
                         GameInputControllerMappingConstants.CardinalDirectionCount) %
                        GameInputControllerMappingConstants.SwitchDirectionCount) +
                       GameInputControllerMappingConstants.FirstSwitchDirection;
        return position == opposite ? 0f : .5f;
    }

    private static bool ReadMappedButton(IGameInputMapper mapper, GameInputGamepadButtons button,
        float[] axes, byte[] buttons, int[] switches)
    {
        if (!mapper.GetGamepadButtonMappingInfo(button, out var mapping)) return false;
        return mapping.ControllerElementKind switch
        {
            GameInputElementKind.Button when mapping.ControllerIndex < buttons.Length => buttons[mapping.ControllerIndex] != 0,
            GameInputElementKind.Axis when mapping.ControllerIndex < axes.Length =>
                mapping.IsInverted ? axes[mapping.ControllerIndex] < .5f : axes[mapping.ControllerIndex] > .5f,
            GameInputElementKind.Switch when mapping.ControllerIndex < switches.Length =>
                switches[mapping.ControllerIndex] == (int)mapping.SwitchPosition,
            _ => false
        };
    }

    private static float ToThumbAxis(float value) => Math.Clamp(value * 2f - 1f, -1f, 1f);

    private static uint Set(uint result, GameInputGamepadButtons buttons,
        GameInputGamepadButtons source, EmulationControllerButtonBit target) =>
        (buttons & source) == 0 ? result : result | Bit(target);

    private static uint Bit(EmulationControllerButtonBit target) => 1u << (int)target;
    private static short Trigger(float value) => Axis(Math.Clamp(value, 0f, 1f));
    private static short Axis(float value) =>
        (short)Math.Round(Math.Clamp(value, -1f, 1f) * short.MaxValue);
}
