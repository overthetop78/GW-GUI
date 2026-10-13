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
    public void TriggerReassignmentKeepsPressureAndDoesNotInheritAnotherTrigger()
    {
        var source = EmulationControllerState.Empty with { DeviceId = "pad", LeftTrigger = short.MaxValue, RightTrigger = 8192 };
        var snapshot = EmulationInputSnapshot.Empty with { Controllers = [source] };
        var mappings = new Dictionary<string, string> { ["L2"] = "Controller:pad:RightTrigger" };
        var configuration = new SegaInput.InputConfiguration(ControllerBindings: [new(0, SegaEnums.ControllerType.SegaDreamcastController, "pad", mappings)]);
        var mapped = GWGUI.Emulation.Sega.Common.Machines.Common.Functions.InputSnapshotFunctions.Apply(snapshot, configuration, false).Controllers[0];
        Assert.Equal(8192, mapped.LeftTrigger);
        Assert.Equal(0, mapped.RightTrigger);
        Assert.Equal(8192, GWGUI.Emulation.Sega.Emulators.Common.Interop.Services.ExternalHostCallbacks.ReadAnalogState(mapped, 2, 12));
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
