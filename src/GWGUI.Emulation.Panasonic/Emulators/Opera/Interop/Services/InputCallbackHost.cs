using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Panasonic.Emulators.Opera.Interop.Enums;

namespace GWGUI.Emulation.Panasonic.Emulators.Opera.Interop.Services;
internal sealed class InputCallbackHost
{
    private const uint DeviceMask = 255, LeftAnalogIndex = 0, RightAnalogIndex = 1, HorizontalAxis = 0, VerticalAxis = 1;
    private const int TrackballCountsPerFrame = 24, MotionStep = 8;
    private readonly PeripheralCategory[] _peripherals = new PeripheralCategory[ControllerConstants.PortCount];
    private readonly short[] _aimX = new short[ControllerConstants.PortCount], _aimY = new short[ControllerConstants.PortCount];
    private EmulationInputSnapshot _input = EmulationInputSnapshot.Empty;
    private int _width = 320, _height = 240;
    private bool _polled;
    internal InputCallbackHost()
    {
        PollCallback = OnPoll;
        StateCallback = OnState;
    }

    internal ExternalCoreApi.InputPollCallback PollCallback { get; }
    internal ExternalCoreApi.InputStateCallback StateCallback { get; }
    internal Exception? Failure { get; private set; }

    internal void Update(
        EmulationInputSnapshot input,
        int width,
        int height
    )
    {
        _input = input;
        _width = Math.Max(1, width);
        _height = Math.Max(1, height);
        _polled = false;
    }

    internal void SetPeripheral(
        int port,
        PeripheralCategory peripheral
    )
    {
        if (port < 0 || port >= _peripherals.Length || !Enum.IsDefined(peripheral))
            throw new ArgumentOutOfRangeException(nameof(port));
        _peripherals[port] = peripheral;
    }

    internal static NativeDevice Device(PeripheralCategory peripheral) => peripheral switch
    {
        PeripheralCategory.None => NativeDevice.None,
        PeripheralCategory.ThreeDOPad => NativeDevice.Joypad,
        PeripheralCategory.Flightstick => NativeDevice.Flightstick,
        PeripheralCategory.Mouse => NativeDevice.Mouse,
        PeripheralCategory.Lightgun => NativeDevice.Lightgun,
        PeripheralCategory.ArcadeLightgun => NativeDevice.ArcadeLightgun,
        PeripheralCategory.OrbatakTrackball => NativeDevice.OrbatakTrackball,
        _ => throw new ArgumentOutOfRangeException(nameof(peripheral))};
    private float Value(int port, ControllerCommand command) => port < _input.Controllers.Count && _input.Controllers[port].Controls.TryGetValue(command.ToString(), out var value) ? value : 0;
    private short Button(int port, ControllerCommand command) => Value(port, command) > 0.5f ? (short)1 : (short)0;
    private short Axis(int port, ControllerCommand negative, ControllerCommand positive) => (short)Math.Clamp(Math.Round((Value(port, positive) - Value(port, negative)) * short.MaxValue), short.MinValue, short.MaxValue);
    private float Relative(int port, ControllerCommand command) => port < _input.Controllers.Count && _input.Controllers[port].Controls.TryGetValue(command.ToString() + InputMappingConstants.RelativeMotionSuffix, out var value) ? value : 0;
    private short TrackballAxis(int port, ControllerCommand negative, ControllerCommand positive) => (short)Math.Clamp(Math.Round(((Value(port, positive) - Value(port, negative)) + (Relative(port, positive) - Relative(port, negative)) / TrackballCountsPerFrame) * (short.MaxValue + 1f)), short.MinValue, short.MaxValue);
    private float MotionValue(int port, ControllerCommand negative, ControllerCommand positive) => Relative(port, positive) - Relative(port, negative) + (Value(port, positive) - Value(port, negative)) * MotionStep;
    private short Motion(int port, ControllerCommand negative, ControllerCommand positive) => (short)Math.Clamp(Math.Round(MotionValue(port, negative, positive)), short.MinValue, short.MaxValue);
    private void OnPoll()
    {
        if (_polled)
            return;
        _polled = true;
        try
        {
            for (var port = 0; port < _peripherals.Length; port++)
            {
                _aimX[port] = (short)Math.Clamp(Math.Round(_aimX[port] + MotionValue(port, ControllerCommand.AimLeft, ControllerCommand.AimRight) * ushort.MaxValue / _width), short.MinValue, short.MaxValue);
                _aimY[port] = (short)Math.Clamp(Math.Round(_aimY[port] + MotionValue(port, ControllerCommand.AimUp, ControllerCommand.AimDown) * ushort.MaxValue / _height), short.MinValue, short.MaxValue);
            }
        }
        catch (Exception error)
        {
            Failure ??= error;
        }
    }

    private short OnState(
        uint port,
        uint device,
        uint index,
        uint id
    )
    {
        try
        {
            if (port >= ControllerConstants.PortCount || _peripherals[port] == PeripheralCategory.None)
                return 0;
            var p = checked((int)port);
            switch ((NativeDevice)(device & DeviceMask))
            {
                case NativeDevice.Joypad:
                    if (index != 0)
                        return 0;
                    var command = (NativeButton)id switch
                    {
                        NativeButton.Y => ControllerCommand.A,
                        NativeButton.B => ControllerCommand.B,
                        NativeButton.A => ControllerCommand.C,
                        NativeButton.X or NativeButton.Start => _peripherals[p] == PeripheralCategory.OrbatakTrackball ? ControllerCommand.StartPlayer2 : ControllerCommand.P,
                        NativeButton.Select => _peripherals[p] == PeripheralCategory.OrbatakTrackball ? ControllerCommand.StartPlayer1 : ControllerCommand.X,
                        NativeButton.Up => ControllerCommand.Up,
                        NativeButton.Down => ControllerCommand.Down,
                        NativeButton.Left => ControllerCommand.Left,
                        NativeButton.Right => ControllerCommand.Right,
                        NativeButton.L => _peripherals[p] == PeripheralCategory.OrbatakTrackball ? ControllerCommand.CoinPlayer1 : ControllerCommand.L,
                        NativeButton.R => _peripherals[p] == PeripheralCategory.OrbatakTrackball ? ControllerCommand.CoinPlayer2 : ControllerCommand.R,
                        NativeButton.R2 => _peripherals[p] == PeripheralCategory.OrbatakTrackball ? ControllerCommand.Service : ControllerCommand.Fire,
                        _ => (ControllerCommand? )null
                    };
                    return command is { } action ? Button(p, action) : (short)0;
                case NativeDevice.Analog:
                    if (_peripherals[p] == PeripheralCategory.OrbatakTrackball && index == LeftAnalogIndex)
                        return id == HorizontalAxis 
                            ? TrackballAxis(p, ControllerCommand.MoveLeft, ControllerCommand.MoveRight) 
                            : id == VerticalAxis ? TrackballAxis(p, ControllerCommand.MoveUp, ControllerCommand.MoveDown) : (short)0;
                    if (index == LeftAnalogIndex)
                        return id == HorizontalAxis ? Axis(p, ControllerCommand.YawLeft, ControllerCommand.YawRight) : id == VerticalAxis ? Axis(p, ControllerCommand.PitchUp, ControllerCommand.PitchDown) : (short)0;
                    return index == RightAnalogIndex && id == VerticalAxis ? Axis(p, ControllerCommand.ThrottleUp, ControllerCommand.ThrottleDown) : (short)0;
                case NativeDevice.Mouse:
                    if (index != 0)
                        return 0;
                    return (NativeMouseControl)id switch
                    {
                        NativeMouseControl.X => Motion(p, ControllerCommand.MoveLeft, ControllerCommand.MoveRight),
                        NativeMouseControl.Y => Motion(p, ControllerCommand.MoveUp, ControllerCommand.MoveDown),
                        NativeMouseControl.Left => Button(p, ControllerCommand.LeftButton),
                        NativeMouseControl.Right => Button(p, ControllerCommand.RightButton),
                        NativeMouseControl.Middle => Button(p, ControllerCommand.MiddleButton),
                        NativeMouseControl.Button4 => Button(p, ControllerCommand.Shift),
                        _ => 0
                    };
                case NativeDevice.Lightgun:
                    if (index != 0)
                        return 0;
                    return (NativeLightgunControl)id switch
                    {
                        NativeLightgunControl.ScreenX => _aimX[p],
                        NativeLightgunControl.ScreenY => _aimY[p],
                        NativeLightgunControl.Trigger => Button(p, ControllerCommand.Trigger),
                        NativeLightgunControl.Reload => Button(p, ControllerCommand.Reload),
                        NativeLightgunControl.Select => Button(p, _peripherals[p] == PeripheralCategory.ArcadeLightgun ? ControllerCommand.Coin : ControllerCommand.Option),
                        NativeLightgunControl.AuxiliaryA => Button(p, ControllerCommand.Service),
                        NativeLightgunControl.Start => Button(p, ControllerCommand.Start),
                        NativeLightgunControl.Offscreen => Button(p, ControllerCommand.Reload),
                        _ => 0
                    };
                default:
                    return 0;
            }
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
        _polled = false;
        Array.Clear(_aimX);
        Array.Clear(_aimY);
        Failure = null;
    }
}