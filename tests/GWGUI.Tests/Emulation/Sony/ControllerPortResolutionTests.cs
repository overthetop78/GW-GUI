using GWGUI.Emulation.Sony.Common.Machines.Common.Enums;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;
using Native = GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants.ControllerPortConstants;
using Ports = GWGUI.Emulation.Sony.Common.Constants.ControllerPortConstants;

namespace GWGUI.Tests.Emulation.Sony;

public sealed class ControllerPortResolutionTests
{
    private const uint DualShock2Device = 257;
    private const uint AlternativeJoypadDevice = 513;
    private const string DualShock2Name = "PS2 DualShock2";
    private const string DigitalName = "Digital Controller";
    private const string AlternativeName = "Alternative Joypad";

    [Fact]
    public void UsesTheOnlyAdvertisedJoypadSubclass()
    {
        IReadOnlyList<IReadOnlyList<ControllerDevice>> devices =
            [[new ControllerDevice(DualShock2Name, DualShock2Device)]];
        Assert.Equal(DualShock2Device,
            ControllerPortFunctions.Resolve(ControllerType.Joystick, Ports.MinimumControllerPort, devices));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ControllerPortFunctions.Resolve(ControllerType.Joystick, Ports.OneControllerPort, devices));
    }

    [Fact]
    public void PrefersTheGenericJoypadWhenAdvertised()
    {
        IReadOnlyList<IReadOnlyList<ControllerDevice>> devices =
            [[new ControllerDevice(DualShock2Name, DualShock2Device),
              new ControllerDevice(DigitalName, Native.JoypadDevice)]];
        Assert.Equal(Native.JoypadDevice,
            ControllerPortFunctions.Resolve(ControllerType.Joystick, Ports.MinimumControllerPort, devices));
    }

    [Fact]
    public void DoesNotChooseAnAmbiguousSubclass()
    {
        IReadOnlyList<IReadOnlyList<ControllerDevice>> devices =
            [[new ControllerDevice(DualShock2Name, DualShock2Device),
              new ControllerDevice(AlternativeName, AlternativeJoypadDevice)]];
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ControllerPortFunctions.Resolve(ControllerType.Joystick, Ports.MinimumControllerPort, devices));
    }

    [Fact]
    public void UnspecifiedControllerInformationUsesTheGenericJoypad()
    {
        Assert.Equal(Native.JoypadDevice,
            ControllerPortFunctions.Resolve(ControllerType.Joystick, Ports.MinimumControllerPort, []));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ControllerPortFunctions.Resolve(ControllerType.Joystick, Ports.MaximumControllerPortCount, []));
    }
}
