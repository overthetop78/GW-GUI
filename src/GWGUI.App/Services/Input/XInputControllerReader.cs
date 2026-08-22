using GWGUI.App.Constants.Input.Controllers;
using GWGUI.App.Contracts.Services.Input;
using System.Runtime.InteropServices;
using GWGUI.Emulation;
using Windows.Gaming.Input;


namespace GWGUI.App.Services.Input;

internal static class XInputControllerReader
{
    internal static IReadOnlyList<EmulationControllerState> ReadAll()
    {
        var states = new EmulationControllerState[4];
        var gamepads = GetWindowsGamepads();
        for (uint port = 0; port < states.Length; port++)
            states[port] = TryGetState(port, out var state)
                ? Map(state.Gamepad)
                : port < gamepads.Count ? Map(gamepads[(int)port].GetCurrentReading()) : EmulationControllerState.Empty;
        return states;
    }

    internal static IReadOnlyList<GameControllerDevice> GetConnectedDevices()
    {
        var gamepads = GetWindowsGamepads();
        var devices = new List<GameControllerDevice>();
        for (uint port = 0; port < 4; port++)
        {
            if (TryGetState(port, out _))
            {
                var xinputName = $"XInput {port + 1}";
                var displayName = port < gamepads.Count ? GetDisplayName(gamepads[(int)port]) : null;
                var name = !string.IsNullOrWhiteSpace(displayName)
                    ? $"{displayName} · {xinputName}"
                    : xinputName;
                devices.Add(new GameControllerDevice($"xinput:{port}", name));
            }
            else if (port < gamepads.Count)
            {
                var name = GetDisplayName(gamepads[(int)port]);
                devices.Add(new GameControllerDevice($"gamepad:{port}",
                    string.IsNullOrWhiteSpace(name) ? $"Gamepad {port + 1}" : name));
            }
        }
        return devices;
    }

    private static IReadOnlyList<Gamepad> GetWindowsGamepads()
    {
        try
        {
            return Gamepad.Gamepads.ToArray();
        }
        catch (Exception exception) when (exception is COMException or InvalidOperationException or TypeInitializationException)
        {
            return [];
        }
    }

    private static string? GetDisplayName(Gamepad gamepad)
    {
        try { return RawGameController.FromGameController(gamepad)?.DisplayName; }
        catch (Exception exception) when (exception is COMException or InvalidOperationException) { return null; }
    }

    private static bool TryGetState(uint port, out XInputState state)
    {
        try { return XInputGetState14(port, out state) == 0; }
        catch (DllNotFoundException) { }
        catch (EntryPointNotFoundException) { }
        try { return XInputGetState910(port, out state) == 0; }
        catch (DllNotFoundException) { state = default; return false; }
        catch (EntryPointNotFoundException) { state = default; return false; }
    }

    internal static EmulationControllerState Map(ushort buttons, byte leftTrigger, byte rightTrigger,
        short leftX, short leftY, short rightX, short rightY) => Map(new XInputGamepad
        {
            Buttons = buttons,
            LeftTrigger = leftTrigger,
            RightTrigger = rightTrigger,
            LeftX = leftX,
            LeftY = leftY,
            RightX = rightX,
            RightY = rightY
        });

    private static EmulationControllerState Map(XInputGamepad gamepad)
    {
        uint buttons = 0;
        buttons = SetButton(buttons, gamepad.Buttons, XInputButtonConstants.B, 0);
        buttons = SetButton(buttons, gamepad.Buttons, XInputButtonConstants.Y, 1);
        buttons = SetButton(buttons, gamepad.Buttons, XInputButtonConstants.Back, 2);
        buttons = SetButton(buttons, gamepad.Buttons, XInputButtonConstants.Start, 3);
        buttons = SetButton(buttons, gamepad.Buttons, XInputButtonConstants.DpadUp, 4);
        buttons = SetButton(buttons, gamepad.Buttons, XInputButtonConstants.DpadDown, 5);
        buttons = SetButton(buttons, gamepad.Buttons, XInputButtonConstants.DpadLeft, 6);
        buttons = SetButton(buttons, gamepad.Buttons, XInputButtonConstants.DpadRight, 7);
        buttons = SetButton(buttons, gamepad.Buttons, XInputButtonConstants.A, 8);
        buttons = SetButton(buttons, gamepad.Buttons, XInputButtonConstants.X, 9);
        buttons = SetButton(buttons, gamepad.Buttons, XInputButtonConstants.LeftShoulder, 10);
        buttons = SetButton(buttons, gamepad.Buttons, XInputButtonConstants.RightShoulder, 11);
        if (gamepad.LeftTrigger > 30) buttons |= 1u << 12;
        if (gamepad.RightTrigger > 30) buttons |= 1u << 13;
        buttons = SetButton(buttons, gamepad.Buttons, XInputButtonConstants.LeftThumb, 14);
        buttons = SetButton(buttons, gamepad.Buttons, XInputButtonConstants.RightThumb, 15);
        buttons = SetButton(buttons, gamepad.Buttons, XInputButtonConstants.Guide, 16);
        return new EmulationControllerState(buttons, gamepad.LeftX, InvertY(gamepad.LeftY),
            gamepad.RightX, InvertY(gamepad.RightY), Trigger(gamepad.LeftTrigger), Trigger(gamepad.RightTrigger));
    }

    private static EmulationControllerState Map(GamepadReading gamepad)
    {
        uint buttons = 0;
        buttons = SetButton(buttons, gamepad.Buttons, GamepadButtons.B, 0);
        buttons = SetButton(buttons, gamepad.Buttons, GamepadButtons.Y, 1);
        buttons = SetButton(buttons, gamepad.Buttons, GamepadButtons.View, 2);
        buttons = SetButton(buttons, gamepad.Buttons, GamepadButtons.Menu, 3);
        buttons = SetButton(buttons, gamepad.Buttons, GamepadButtons.DPadUp, 4);
        buttons = SetButton(buttons, gamepad.Buttons, GamepadButtons.DPadDown, 5);
        buttons = SetButton(buttons, gamepad.Buttons, GamepadButtons.DPadLeft, 6);
        buttons = SetButton(buttons, gamepad.Buttons, GamepadButtons.DPadRight, 7);
        buttons = SetButton(buttons, gamepad.Buttons, GamepadButtons.A, 8);
        buttons = SetButton(buttons, gamepad.Buttons, GamepadButtons.X, 9);
        buttons = SetButton(buttons, gamepad.Buttons, GamepadButtons.LeftShoulder, 10);
        buttons = SetButton(buttons, gamepad.Buttons, GamepadButtons.RightShoulder, 11);
        if (gamepad.LeftTrigger > 0.12d) buttons |= 1u << 12;
        if (gamepad.RightTrigger > 0.12d) buttons |= 1u << 13;
        buttons = SetButton(buttons, gamepad.Buttons, GamepadButtons.LeftThumbstick, 14);
        buttons = SetButton(buttons, gamepad.Buttons, GamepadButtons.RightThumbstick, 15);
        return new EmulationControllerState(buttons, Axis(gamepad.LeftThumbstickX), Axis(-gamepad.LeftThumbstickY),
            Axis(gamepad.RightThumbstickX), Axis(-gamepad.RightThumbstickY),
            Trigger(gamepad.LeftTrigger), Trigger(gamepad.RightTrigger));
    }

    private static uint SetButton(uint result, ushort pressedButtons, ushort source, int target) =>
        (pressedButtons & source) == 0 ? result : result | 1u << target;

    private static uint SetButton(uint result, GamepadButtons pressedButtons, GamepadButtons source, int target) =>
        (pressedButtons & source) == 0 ? result : result | 1u << target;

    private static short InvertY(short value) => value == short.MinValue ? short.MaxValue : (short)-value;
    private static short Trigger(byte value) => (short)Math.Round(value / 255d * short.MaxValue);
    private static short Trigger(double value) => Axis(Math.Clamp(value, 0d, 1d));
    private static short Axis(double value) => (short)Math.Round(Math.Clamp(value, -1d, 1d) * short.MaxValue);

    [DllImport("xinput1_4.dll", EntryPoint = "XInputGetState")]
    private static extern uint XInputGetState14(uint userIndex, out XInputState state);

    [DllImport("xinput9_1_0.dll", EntryPoint = "XInputGetState")]
    private static extern uint XInputGetState910(uint userIndex, out XInputState state);

    [StructLayout(LayoutKind.Sequential)]
    private struct XInputState { public uint PacketNumber; public XInputGamepad Gamepad; }

    [StructLayout(LayoutKind.Sequential)]
    private struct XInputGamepad
    {
        public ushort Buttons;
        public byte LeftTrigger;
        public byte RightTrigger;
        public short LeftX;
        public short LeftY;
        public short RightX;
        public short RightY;
    }
}


