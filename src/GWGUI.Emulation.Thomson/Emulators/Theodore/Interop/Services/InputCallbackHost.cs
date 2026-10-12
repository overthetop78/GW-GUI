using GWGUI.Emulation.Interop;

namespace GWGUI.Emulation.Thomson.Emulators.Theodore.Interop.Services;

internal sealed class InputCallbackHost
{
    private const uint JoypadDevice = 1;
    private const uint PointerDevice = 6;
    private const uint DeviceMask = 255;
    private const uint FireButton = 0;
    private const uint UpButton = 4;
    private const uint DownButton = 5;
    private const uint LeftButton = 6;
    private const uint RightButton = 7;
    private const uint PointerHorizontal = 0;
    private const uint PointerVertical = 1;
    private const uint PointerPressed = 2;
    private const float PointerRange = ushort.MaxValue;
    private const int PointerMinimum = -short.MaxValue;
    private const int PointerMaximum = short.MaxValue;
    private readonly EnvironmentCallbackHost _environment;
    private readonly VideoCallbackHost _video;
    private readonly KeyboardState _keyboard;
    private EmulationInputSnapshot _input = EmulationInputSnapshot.Empty;
    private int _x, _y;
    internal InputCallbackHost(MachineModel model, EnvironmentCallbackHost environment, VideoCallbackHost video) { _environment = environment; _video = video; _keyboard = new KeyboardState(model); PollCallback = OnPoll; StateCallback = OnState; }
    internal ExternalCoreApi.InputPollCallback PollCallback { get; }
    internal ExternalCoreApi.InputStateCallback StateCallback { get; }
    internal Exception? Failure { get; private set; }
    internal void Update(EmulationInputSnapshot input) => _input = input;

    private void OnPoll()
    {
        try
        {
            _keyboard.Publish(_input.Keys, _environment.Keyboard);
            if (_video.LatestFrame is { } frame)
            {
                _x = (int)Math.Clamp(_x + Math.Round((double)_input.Pointer.DeltaX * PointerRange / frame.Width), PointerMinimum, PointerMaximum);
                _y = (int)Math.Clamp(_y + Math.Round((double)_input.Pointer.DeltaY * PointerRange / frame.Height), PointerMinimum, PointerMaximum);
            }
            _input = _input with { Pointer = _input.Pointer with { DeltaX = 0, DeltaY = 0 } };
        }
        catch (Exception error) { Failure ??= error; }
    }

    private short OnState(uint port, uint device, uint index, uint id)
    {
        device &= DeviceMask;
        if (device == PointerDevice && port == ControllerConstants.PortCount) return id switch { PointerHorizontal => (short)_x, PointerVertical => (short)_y, PointerPressed => _input.Pointer.Left ? (short)1 : (short)0, _ => 0 };
        if (device != JoypadDevice || port >= _input.Controllers.Count) return 0;
        var controller = _input.Controllers[(int)port];
        var pressed = id switch { FireButton => (controller.Buttons & ControllerConstants.FireButtonMask) != 0, UpButton => controller.LeftY < 0, DownButton => controller.LeftY > 0, LeftButton => controller.LeftX < 0, RightButton => controller.LeftX > 0, _ => false };
        return pressed ? (short)1 : (short)0;
    }

    internal void Release()
    {
        _keyboard.Release(_environment.Keyboard);
        _input = EmulationInputSnapshot.Empty;
        Failure = null;
    }
}
