using System.Runtime.InteropServices;
using GWGUI.Emulation;
using Windows.Gaming.Input;

namespace GWGUI.App.Services;

internal static class XInputControllerReader
{
    private const ushort DpadUp = 0x0001, DpadDown = 0x0002, DpadLeft = 0x0004, DpadRight = 0x0008;
    private const ushort Start = 0x0010, Back = 0x0020, LeftThumb = 0x0040, RightThumb = 0x0080;
    private const ushort LeftShoulder = 0x0100, RightShoulder = 0x0200;
    private const ushort Guide = 0x0400;
    private const ushort A = 0x1000, B = 0x2000, X = 0x4000, Y = 0x8000;

    internal static IReadOnlyList<EmulationControllerState> ReadAll()
    {
        var states = new EmulationControllerState[4];
        for (uint port = 0; port < states.Length; port++)
            states[port] = TryGetState(port, out var state) ? Map(state.Gamepad) : EmulationControllerState.Empty;
        return states;
    }

    internal static IReadOnlyList<GameControllerDevice> GetConnectedDevices()
    {
        var names = GetWindowsGamepadNames();
        var devices = new List<GameControllerDevice>();
        for (uint port = 0; port < 4; port++)
            if (TryGetState(port, out _))
            {
                var xinputName = $"XInput {port + 1}";
                var name = port < names.Count && !string.IsNullOrWhiteSpace(names[(int)port])
                    ? $"{names[(int)port]} · {xinputName}"
                    : xinputName;
                devices.Add(new GameControllerDevice($"xinput:{port}", name));
            }
        return devices;
    }

    private static IReadOnlyList<string> GetWindowsGamepadNames()
    {
        try
        {
            return Gamepad.Gamepads
                .Select(gamepad => RawGameController.FromGameController(gamepad)?.DisplayName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name!)
                .ToArray();
        }
        catch (Exception exception) when (exception is COMException or InvalidOperationException or TypeInitializationException)
        {
            return [];
        }
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
        Set(B, 0); Set(Y, 1); Set(Back, 2); Set(Start, 3);
        Set(DpadUp, 4); Set(DpadDown, 5); Set(DpadLeft, 6); Set(DpadRight, 7);
        Set(A, 8); Set(X, 9); Set(LeftShoulder, 10); Set(RightShoulder, 11);
        if (gamepad.LeftTrigger > 30) buttons |= 1u << 12;
        if (gamepad.RightTrigger > 30) buttons |= 1u << 13;
        Set(LeftThumb, 14); Set(RightThumb, 15);
        Set(Guide, 16);
        return new EmulationControllerState(buttons, gamepad.LeftX, InvertY(gamepad.LeftY),
            gamepad.RightX, InvertY(gamepad.RightY), Trigger(gamepad.LeftTrigger), Trigger(gamepad.RightTrigger));

        void Set(ushort source, int target)
        {
            if ((gamepad.Buttons & source) != 0) buttons |= 1u << target;
        }
    }

    private static short InvertY(short value) => value == short.MinValue ? short.MaxValue : (short)-value;
    private static short Trigger(byte value) => (short)Math.Round(value / 255d * short.MaxValue);

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


internal sealed record GameControllerDevice(string Id, string Name)
{
    public override string ToString() => Name;
}
