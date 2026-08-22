using GWGUI.Emulation;
using GWGUI.Emulation.Atari;
using GWGUI.Emulation.Atari.Cores;
using System.IO;

namespace GWGUI.Tests;

public sealed class AtariControllerTests
{
    [Fact]
    public void EveryModelDefinesPortsAndCompatiblePeripherals()
    {
        foreach (var model in Enum.GetValues<AtariMachineModel>())
        {
            var definition = AtariCompatibilityCatalog.Get(model);
            Assert.InRange(definition.ControllerPortCount, AtariCompatibilityConstants.OneControllerPort,
                AtariCompatibilityConstants.FourControllerPorts);
            var peripherals = AtariControllerFunctions.Peripherals(model);
            // Kept in the core contract for legacy configurations and Stella's API,
            // but the settings UI must never expose it.
            Assert.Contains(AtariPeripheralCategory.Automatic, peripherals);
            Assert.Contains(AtariPeripheralCategory.None, peripherals);
        }
    }

    [Fact]
    public void EightBitAndSpecificConsoleControllersAreDeclared()
    {
        Assert.Equal(AtariCompatibilityConstants.TwoControllerPorts,
            AtariCompatibilityCatalog.Get(AtariMachineModel.Atari800Xl).ControllerPortCount);
        var atari5200 = AtariControllerFunctions.Peripherals(AtariMachineModel.Atari5200);
        Assert.Contains(AtariPeripheralCategory.AnalogJoystick, atari5200);
        Assert.Contains(AtariPeripheralCategory.NumericKeypad, atari5200);
        var jaguar = AtariControllerFunctions.Peripherals(AtariMachineModel.Jaguar);
        Assert.Contains(AtariPeripheralCategory.EnhancedController, jaguar);
        Assert.Equal(AtariCompatibilityConstants.TwoControllerPorts,
            AtariCompatibilityCatalog.Get(AtariMachineModel.Jaguar).ControllerPortCount);
    }

    [Fact]
    public void ResolveDeviceUsesAnnouncedPortsAndRejectsMissingExplicitDevice()
    {
        var ports = new[]
        {
            new AtariControllerPort(new[]
            {
                new AtariControllerDevice(AtariCoreLifecycleConstants.JoypadDeviceName,
                    AtariControllerTestConstants.JoypadDeviceId),
                new AtariControllerDevice(AtariCoreLifecycleConstants.MouseDeviceName,
                    AtariControllerTestConstants.MouseDeviceId)
            })
        };
        Assert.Equal(AtariControllerTestConstants.JoypadDeviceId,
            AtariControllerPortFunctions.ResolveDevice(ports, AtariControllerTestConstants.FirstPort,
                AtariPeripheralCategory.Automatic));
        Assert.Equal(AtariControllerTestConstants.MouseDeviceId,
            AtariControllerPortFunctions.ResolveDevice(ports, AtariControllerTestConstants.FirstPort,
                AtariPeripheralCategory.Mouse));
        Assert.Equal(AtariCoreLifecycleConstants.NoDevice,
            AtariControllerPortFunctions.ResolveDevice(ports, AtariControllerTestConstants.FirstPort,
                AtariPeripheralCategory.None));
        Assert.Equal(AtariControllerTestConstants.JoypadDeviceId,
            AtariControllerPortFunctions.ResolveDevice(ports,
                AtariControllerTestConstants.FirstPort, AtariPeripheralCategory.LightGun));
    }

    [Fact]
    public void StellaControllerProfilesAreSentToTheCoreAsAutomatic()
    {
        const uint automatic = 41;
        IReadOnlyList<AtariControllerPort> ports =
        [
            new AtariControllerPort(
            [
                new AtariControllerDevice("Automatic", automatic),
                new AtariControllerDevice("None", 0)
            ])
        ];

        Assert.Equal(automatic, AtariControllerPortFunctions.ResolveDevice(
            ports, 0, AtariPeripheralCategory.Paddle, AtariEmulator.Stella));
        Assert.Equal(automatic, AtariControllerPortFunctions.ResolveDevice(
            ports, 0, AtariPeripheralCategory.DrivingController, AtariEmulator.Stella));
    }

    [Fact]
    public void InputMapsBitmaskExtremeAxesTriggersAndTwoIndependentControllers()
    {
        var first = new EmulationControllerState(AtariControllerTestConstants.FirstButtons,
            short.MinValue, short.MaxValue, AtariControllerTestConstants.NeutralAxis,
            AtariControllerTestConstants.NeutralAxis, AtariControllerTestConstants.LeftTrigger,
            AtariControllerTestConstants.NeutralAxis);
        var second = new EmulationControllerState(AtariControllerTestConstants.SecondButtons,
            AtariControllerTestConstants.SecondLeftX, AtariControllerTestConstants.NeutralAxis,
            AtariControllerTestConstants.NeutralAxis, AtariControllerTestConstants.NeutralAxis,
            AtariControllerTestConstants.NeutralAxis, AtariControllerTestConstants.RightTrigger);
        var snapshot = new EmulationInputSnapshot(new HashSet<EmulationKey>(), EmulationInputSnapshot.Empty.Pointer,
            [first, second]);

        Assert.Equal(unchecked((short)AtariControllerTestConstants.FirstButtons), State(snapshot,
            AtariControllerTestConstants.FirstPort, AtariInputConstants.JoypadDevice,
            AtariInputConstants.LeftAnalogIndex, AtariInputConstants.JoypadMaskId));
        Assert.Equal(short.MinValue, State(snapshot, AtariControllerTestConstants.FirstPort,
            AtariInputConstants.AnalogDevice, AtariInputConstants.LeftAnalogIndex, AtariInputConstants.AnalogXId));
        Assert.Equal(short.MaxValue, State(snapshot, AtariControllerTestConstants.FirstPort,
            AtariInputConstants.AnalogDevice, AtariInputConstants.LeftAnalogIndex, AtariInputConstants.AnalogYId));
        Assert.Equal(AtariControllerTestConstants.LeftTrigger, State(snapshot, AtariControllerTestConstants.FirstPort,
            AtariInputConstants.AnalogDevice, AtariControllerConstants.TriggerAnalogIndex,
            AtariControllerConstants.LeftTriggerId));
        Assert.Equal(AtariControllerTestConstants.SecondLeftX, State(snapshot, AtariControllerTestConstants.SecondPort,
            AtariInputConstants.AnalogDevice, AtariInputConstants.LeftAnalogIndex, AtariInputConstants.AnalogXId));
        Assert.Equal(AtariControllerTestConstants.RightTrigger, State(snapshot, AtariControllerTestConstants.SecondPort,
            AtariInputConstants.AnalogDevice, AtariControllerConstants.TriggerAnalogIndex,
            AtariControllerConstants.RightTriggerId));
        Assert.Equal(AtariInputConstants.InactiveState, State(snapshot, AtariControllerTestConstants.AbsentPort,
            AtariInputConstants.JoypadDevice, AtariInputConstants.LeftAnalogIndex,
            AtariControllerTestConstants.FirstButtonId));
    }

    [Fact]
    public void InputExposesPressedKeyboardKeysToDirectCorePolling()
    {
        var snapshot = new EmulationInputSnapshot(new HashSet<EmulationKey> { EmulationKey.A },
            EmulationInputSnapshot.Empty.Pointer, []);

        Assert.Equal(AtariInputConstants.ActiveState, State(snapshot, AtariInputConstants.PrimaryPort,
            AtariInputConstants.KeyboardDevice, AtariInputConstants.LeftAnalogIndex, 'a'));
        Assert.Equal(AtariInputConstants.InactiveState, State(snapshot, AtariInputConstants.PrimaryPort,
            AtariInputConstants.KeyboardDevice, AtariInputConstants.LeftAnalogIndex, 'b'));
    }

    [Fact]
    public void DeadZoneIsAppliedPerPortAndValidated()
    {
        Assert.Equal(AtariControllerConstants.NeutralAxis,
            AtariControllerFunctions.ApplyDeadZone(AtariControllerTestConstants.InsideDeadZone,
                AtariControllerConstants.DefaultDeadZonePercent));
        Assert.Equal(AtariControllerTestConstants.OutsideDeadZone,
            AtariControllerFunctions.ApplyDeadZone(AtariControllerTestConstants.OutsideDeadZone,
                AtariControllerConstants.DefaultDeadZonePercent));

        var snapshot = new EmulationInputSnapshot(new HashSet<EmulationKey>(), EmulationInputSnapshot.Empty.Pointer,
        [
            new EmulationControllerState(AtariControllerTestConstants.FirstButtons,
                AtariControllerTestConstants.InsideDeadZone, AtariControllerTestConstants.OutsideDeadZone,
                AtariControllerTestConstants.NeutralAxis, AtariControllerTestConstants.NeutralAxis,
                AtariControllerTestConstants.InsideDeadZone, AtariControllerTestConstants.OutsideDeadZone),
            new EmulationControllerState(AtariControllerTestConstants.SecondButtons,
                AtariControllerTestConstants.InsideDeadZone, AtariControllerTestConstants.InsideDeadZone,
                AtariControllerTestConstants.NeutralAxis, AtariControllerTestConstants.NeutralAxis,
                AtariControllerTestConstants.NeutralAxis, AtariControllerTestConstants.NeutralAxis)
        ]);
        var filtered = AtariControllerFunctions.ApplyDeadZones(snapshot,
        [
            new AtariControllerBinding(AtariControllerTestConstants.FirstPort, AtariPeripheralCategory.Joystick,
                DeadZonePercent: AtariControllerConstants.DefaultDeadZonePercent)
        ]);
        Assert.Equal(AtariControllerConstants.NeutralAxis,
            filtered.Controllers[AtariControllerTestConstants.FirstPort].LeftX);
        Assert.Equal(AtariControllerTestConstants.OutsideDeadZone,
            filtered.Controllers[AtariControllerTestConstants.FirstPort].LeftY);
        Assert.Equal(AtariControllerConstants.NeutralAxis,
            filtered.Controllers[AtariControllerTestConstants.FirstPort].LeftTrigger);
        Assert.Equal(AtariControllerTestConstants.InsideDeadZone,
            filtered.Controllers[AtariControllerTestConstants.SecondPortIndex].LeftX);

        Assert.Throws<ArgumentOutOfRangeException>(() => new AtariMachineConfiguration(
            AtariMachineModel.Atari800Xl, input: new AtariInputConfiguration(Controllers:
            [new AtariControllerBinding(AtariControllerTestConstants.FirstPort, AtariPeripheralCategory.Joystick,
                DeadZonePercent: AtariControllerConstants.MaximumDeadZonePercent +
                                 AtariControllerTestConstants.OnePercent)])));
        Assert.Throws<ArgumentException>(() => new AtariMachineConfiguration(AtariMachineModel.Lynx,
            input: new AtariInputConfiguration(Controllers:
            [new AtariControllerBinding(AtariControllerTestConstants.FirstPort, AtariPeripheralCategory.Paddle)])));
    }

    private static short State(EmulationInputSnapshot snapshot, uint port, uint device, uint index, uint id) =>
        AtariInputFunctions.State(snapshot, port, device, index, id);
}

internal static class AtariControllerTestConstants
{
    internal const int FirstPort = 0;
    internal const uint SecondPort = 1;
    internal const int SecondPortIndex = 1;
    internal const uint AbsentPort = 3;
    internal const uint JoypadDeviceId = 71;
    internal const uint MouseDeviceId = 72;
    internal const uint FirstButtonId = 0;
    internal const uint SecondButtonId = 15;
    internal const uint FirstButtons = 1u << (int)FirstButtonId;
    internal const uint SecondButtons = 1u << (int)SecondButtonId;
    internal const short NeutralAxis = 0;
    internal const short LeftTrigger = 12345;
    internal const short RightTrigger = 23456;
    internal const short SecondLeftX = 30000;
    internal const short InsideDeadZone = 4000;
    internal const short OutsideDeadZone = 12000;
    internal const int OnePercent = 1;
}
