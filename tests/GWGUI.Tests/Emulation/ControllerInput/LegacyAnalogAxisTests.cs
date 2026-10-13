using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using AtariInput = GWGUI.Emulation.Atari.Common.Machines.Common.Contracts;
using AtariEnums = GWGUI.Emulation.Atari.Common.Machines.Common.Enums;
using CommodoreInput = GWGUI.Emulation.Commodore.Common.Machines.Common.Contracts;
using CommodoreEnums = GWGUI.Emulation.Commodore.Common.Machines.Common.Enums;
using SegaInput = GWGUI.Emulation.Sega.Common.Machines.Common.Contracts;
using SegaEnums = GWGUI.Emulation.Sega.Common.Machines.Common.Enums;

namespace GWGUI.Tests.Emulation.ControllerInput;

public sealed class LegacyAnalogAxisTests
{
    [Fact]
    public void SingleMissionStickDoesNotSelectTheDualPeripheral()
    {
        var devices = new GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts.ControllerDevice[] { new("Dual Mission Sticks", 1029), new("Mission Stick", 773) };
        Assert.Equal(773u, GWGUI.Emulation.Sega.Emulators.Common.Interop.Services.ExternalCore.ResolveControllerDevice(devices, SegaEnums.ControllerType.SegaSaturnMissionStick));
        Assert.Equal(0u, GWGUI.Emulation.Sega.Emulators.Common.Interop.Services.ExternalCore.ResolveControllerDevice([devices[0]], SegaEnums.ControllerType.SegaSaturnMissionStick));
    }

    [Theory]
    [InlineData(SegaEnums.ControllerType.SegaXe1Ap, "MegaDrive", 8)]
    [InlineData(SegaEnums.ControllerType.SegaDreamcastFishingController, "Dreamcast", 8)]
    [InlineData(SegaEnums.ControllerType.SegaDreamcastMaracas, "Dreamcast", 8)]
    [InlineData(SegaEnums.ControllerType.SegaSportsPad, "MegaDrive", 4)]
    [InlineData(SegaEnums.ControllerType.SegaSaturnMissionStick, "Saturn", 6)]
    [InlineData(SegaEnums.ControllerType.SegaSaturnArcadeRacer, "Saturn", 2)]
    public void SpecialControllerAxesArePairedUnassignedAndPersisted(SegaEnums.ControllerType type, string model, int count)
    {
        var input = new SegaInput.InputConfiguration(ControllerBindings: [new(0, type, "pad", new Dictionary<string, string>())]);
        var configuration = new SegaInput.MachineConfiguration(model, string.Empty, Input: input);
        var settings = GWGUI.Emulation.Sega.Common.Machines.Common.Functions.InputSettingsFunctions.Describe(configuration);
        var port = settings.ControllerPorts[0];
        var axes = port.Bindings.Definitions.Where(item => item.OppositeDirectionId is not null).ToArray();
        Assert.Equal(count, axes.Length);
        foreach (var axis in axes)
        {
            Assert.Empty(axis.DefaultBinding);
            Assert.Equal(axis.Id, Assert.Single(axes, item => item.Id == axis.OppositeDirectionId).OppositeDirectionId);
        }
        var values = new Dictionary<string, string> { ["LeftStickRight"] = "Keyboard:A" };
        var changed = settings with { ControllerPorts = [port with { Bindings = port.Bindings with { Values = values } }] };
        var applied = GWGUI.Emulation.Sega.Common.Machines.Common.Functions.InputSettingsFunctions.Apply(configuration, changed);
        Assert.Equal("Keyboard:A", applied.Input!.ControllerBindings![0].ButtonMappings!["LeftStickRight"]);
    }

    [Theory]
    [InlineData(SegaEnums.ControllerType.SegaXe1Ap, true, true)]
    [InlineData(SegaEnums.ControllerType.SegaDreamcastFishingController, true, true)]
    [InlineData(SegaEnums.ControllerType.SegaDreamcastMaracas, true, true)]
    [InlineData(SegaEnums.ControllerType.SegaSportsPad, false, false)]
    [InlineData(SegaEnums.ControllerType.SegaSaturnMissionStick, false, true)]
    [InlineData(SegaEnums.ControllerType.SegaSaturnArcadeRacer, false, false)]
    public void SpecialControllerAxesKeepTheirOwnExplicitCourse(SegaEnums.ControllerType type, bool rightX, bool rightY)
    {
        var physical = EmulationControllerState.Empty with { DeviceId = "pad", LeftY = short.MaxValue, RightX = short.MaxValue, RightY = short.MaxValue };
        var mappings = new Dictionary<string, string>
        {
            ["LeftStickRight"] = "Controller:pad:Axis0Positive", ["LeftStickLeft"] = "Controller:pad:Axis0Negative",
            ["RightStickDown"] = "Controller:pad:Axis0Positive", ["RightStickUp"] = "Controller:pad:Axis0Negative",
            ["RightStickRight"] = "Controller:pad:Axis0Positive", ["RightStickLeft"] = "Controller:pad:Axis0Negative"
        };
        physical = physical with { Controls = new(new Dictionary<string, float> { ["Axis0"] = .625f }) };
        var snapshot = EmulationInputSnapshot.Empty with { Controllers = [physical] };
        var input = new SegaInput.InputConfiguration(ControllerBindings: [new(0, type, "pad", mappings)]);
        var mapped = GWGUI.Emulation.Sega.Common.Machines.Common.Functions.InputSnapshotFunctions.Apply(snapshot, input, false).Controllers[0];
        Assert.Equal(8192, mapped.LeftX);
        Assert.Equal(0, mapped.LeftY);
        Assert.Equal(rightX ? 8192 : 0, mapped.RightX);
        Assert.Equal(rightY ? 8192 : 0, mapped.RightY);
        Assert.Equal(rightY ? 8192 : 0, GWGUI.Emulation.Sega.Emulators.Common.Interop.Services.ExternalHostCallbacks.ReadAnalogState(mapped, 1, 1));
    }

    [Theory]
    [InlineData(SegaEnums.ControllerType.SegaXe1Ap, "MD XE-1AP", 773u)]
    [InlineData(SegaEnums.ControllerType.SegaDreamcastFishingController, "Fishing Controller", 1537u)]
    [InlineData(SegaEnums.ControllerType.SegaDreamcastMaracas, "Maracas", 1281u)]
    [InlineData(SegaEnums.ControllerType.SegaSaturnMissionStick, "Mission Stick", 773u)]
    [InlineData(SegaEnums.ControllerType.SegaSaturnArcadeRacer, "Arcade Racer", 517u)]
    [InlineData(SegaEnums.ControllerType.SegaSaturnThreeDControlPad, "3D Control Pad", 261u)]
    [InlineData(SegaEnums.ControllerType.SegaSportsPad, "MS Sports Pad", 517u)]
    [InlineData(SegaEnums.ControllerType.SegaPaddleControl, "MS Paddle Control", 261u)]
    public void NativePeripheralSelectionDoesNotSubstituteTheDigitalPad(SegaEnums.ControllerType type, string name, uint id)
    {
        var devices = new GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts.ControllerDevice[] { new("Control Pad", 1), new(name, id) };
        Assert.Equal(id, GWGUI.Emulation.Sega.Emulators.Common.Interop.Services.ExternalCore.ResolveControllerDevice(devices, type));
        Assert.Equal(0u, GWGUI.Emulation.Sega.Emulators.Common.Interop.Services.ExternalCore.ResolveControllerDevice([new("Control Pad", 1)], type));
    }

    [Theory]
    [InlineData(SegaEnums.ControllerType.SegaDreamcastController)]
    [InlineData(SegaEnums.ControllerType.SegaDreamcastFishingController)]
    public void TriggerReassignmentKeepsPressureAndDoesNotInheritAnotherTrigger(SegaEnums.ControllerType type)
    {
        var source = EmulationControllerState.Empty with { DeviceId = "pad", LeftTrigger = short.MaxValue, RightTrigger = 8192 };
        var snapshot = EmulationInputSnapshot.Empty with { Controllers = [source] };
        var mappings = new Dictionary<string, string> { ["L2"] = "Controller:pad:RightTrigger" };
        var configuration = new SegaInput.InputConfiguration(ControllerBindings: [new(0, type, "pad", mappings)]);
        var mapped = GWGUI.Emulation.Sega.Common.Machines.Common.Functions.InputSnapshotFunctions.Apply(snapshot, configuration, false).Controllers[0];
        Assert.Equal(8192, mapped.LeftTrigger);
        Assert.Equal(0, mapped.RightTrigger);
        Assert.Equal(8192, GWGUI.Emulation.Sega.Emulators.Common.Interop.Services.ExternalHostCallbacks.ReadAnalogState(mapped, 2, 12));
    }

    [Theory]
    [InlineData(1537u, 0u, 0u, 1)]
    [InlineData(1281u, 0u, 256u, 1)]
    [InlineData(773u, 1u, 1u, 8192)]
    [InlineData(5u, 1u, 1u, 8192)]
    [InlineData(0u, 1u, 1u, 0)]
    public void NativeSubtypeKeepsItsBaseInputChannels(uint device, uint index, uint id, int expected)
    {
        var state = EmulationControllerState.Empty with { Buttons = 1, RightY = 8192 };
        Assert.Equal(expected, GWGUI.Emulation.Sega.Emulators.Common.Interop.Services.ExternalHostCallbacks.ReadControllerState(state, device, index, id));
    }

    [Theory]
    [InlineData(2u, 12u, 8192)]
    [InlineData(2u, 13u, 24575)]
    [InlineData(2u, 0u, 32767)]
    [InlineData(2u, 32u, 0)]
    [InlineData(0u, 0u, -12000)]
    [InlineData(1u, 1u, 9000)]
    public void NativeAnalogReaderKeepsSupportedChannelsAndBounds(uint index, uint id, int expected)
    {
        var state = new EmulationControllerState(1, -12000, 0, 0, 9000, 8192, 24575);
        Assert.Equal(expected, GWGUI.Emulation.Sega.Emulators.Common.Interop.Services.ExternalHostCallbacks.ReadAnalogState(state, index, id));
    }

    [Theory]
    [InlineData(.625f, 8192)]
    [InlineData(.375f, -8192)]
    [InlineData(.5f, 0)]
    public void PhysicalAxisPreservesItsCourseThroughEveryComposer(float position, int expected)
    {
        var state = EmulationControllerState.Empty with { DeviceId = "pad", LeftY = short.MaxValue, Controls = new(new Dictionary<string, float> { ["Axis0"] = position }) };
        var snapshot = EmulationInputSnapshot.Empty with { Controllers = [state] };
        AssertComposed(snapshot, "Controller:pad:Axis0Negative", "Controller:pad:Axis0Positive", expected);
    }

    [Theory]
    [InlineData(false, -32768)]
    [InlineData(true, 0)]
    public void KeyboardDirectionsAndTheirOppositionReachTheAnalogAxis(bool both, int expected)
    {
        var keys = new HashSet<EmulationKey> { EmulationKey.A };
        if (both) keys.Add(EmulationKey.B);
        AssertComposed(EmulationInputSnapshot.Empty with { Keys = keys }, "Keyboard:A", "Keyboard:B", expected);
    }

    private static void AssertComposed(EmulationInputSnapshot snapshot, string negative, string positive, int expected)
    {
        var mappings = new Dictionary<string, string> { ["Left"] = negative, ["Right"] = positive };
        var atari = new AtariInput.InputConfiguration(Controllers: [new(0, AtariEnums.PeripheralCategory.AnalogJoystick, "pad", mappings)]);
        var atariState = GWGUI.Emulation.Atari.Common.Machines.Common.Functions.InputSnapshotFunctions.Apply(snapshot, atari, AtariEnums.MachineModel.Atari5200).Controllers[0];
        var commodore = new CommodoreInput.InputConfiguration(ControllerBindings: [new(0, CommodoreEnums.ControllerType.AnalogJoystick, "pad", mappings)]);
        var commodoreState = GWGUI.Emulation.Commodore.Common.Machines.Common.Functions.InputSnapshotFunctions.Apply(snapshot, commodore, false).Controllers[0];
        var segaMappings = new Dictionary<string, string> { ["LeftStickLeft"] = negative, ["LeftStickRight"] = positive };
        var sega = new SegaInput.InputConfiguration(ControllerBindings: [new(0, SegaEnums.ControllerType.SegaDreamcastController, "pad", segaMappings)]);
        var segaState = GWGUI.Emulation.Sega.Common.Machines.Common.Functions.InputSnapshotFunctions.Apply(snapshot, sega, false).Controllers[0];
        foreach (var state in new[] { atariState, commodoreState, segaState })
        {
            Assert.Equal(expected, state.LeftX);
            Assert.Equal(0, state.LeftY);
        }
    }
}
