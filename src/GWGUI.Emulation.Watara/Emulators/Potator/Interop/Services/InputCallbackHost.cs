using GWGUI.Emulation.Interop;

namespace GWGUI.Emulation.Watara.Emulators.Potator.Interop.Services;
internal sealed class InputCallbackHost
{
    private enum NativeButton : uint
    {
        B = 0,
        Select = 2,
        Start = 3,
        Up = 4,
        Down = 5,
        Left = 6,
        Right = 7,
        A = 8
    }

    private const uint JoypadDevice = 1;
    private const uint DeviceMask = 255;
    private EmulationInputSnapshot _input = EmulationInputSnapshot.Empty;
    internal InputCallbackHost()
    {
        PollCallback = OnPoll;
        StateCallback = OnState;
    }

    internal ExternalCoreApi.InputPollCallback PollCallback { get; }
    internal ExternalCoreApi.InputStateCallback StateCallback { get; }
    internal Exception? Failure { get; private set; }

    internal void Update(EmulationInputSnapshot input) => _input = input;
    private void OnPoll()
    {
    }

    private short OnState(uint port, uint device, uint index, uint id)
    {
        try
        {
            if (port != ControllerConstants.FirstPort || (device & DeviceMask) != JoypadDevice || index != 0 || _input.Controllers.Count == 0)
                return 0;
            var controller = _input.Controllers[ControllerConstants.FirstPort];
            var buttons = (ControllerButtons)controller.Buttons;
            var pressed = (NativeButton)id switch
            {
                NativeButton.Up => controller.LeftY < 0,
                NativeButton.Down => controller.LeftY > 0,
                NativeButton.Left => controller.LeftX < 0,
                NativeButton.Right => controller.LeftX > 0,
                NativeButton.A => buttons.HasFlag(ControllerButtons.A),
                NativeButton.B => buttons.HasFlag(ControllerButtons.B),
                NativeButton.Start => buttons.HasFlag(ControllerButtons.Start),
                NativeButton.Select => buttons.HasFlag(ControllerButtons.Select),
                _ => false
            };
            return pressed ? (short)1 : (short)0;
        }
        catch (Exception error)
        {
            Failure ??= error;
            return 0;
        }
    }

    internal void Release()
    {
        _input = EmulationInputSnapshot.Empty;
        Failure = null;
    }
}