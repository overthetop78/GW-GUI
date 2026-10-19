using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class PhysicalInputReaderTests
{
    [Theory]
    [InlineData("Keyboard:A", true)]
    [InlineData("keyboard:A", true)]
    [InlineData("Keyboard:a", false)]
    [InlineData("Keyboard:B", false)]
    [InlineData("Keyboard:", false)]
    [InlineData("A", false)]
    [InlineData("LeftStickRight", false)]
    [InlineData("Controller:pad:LeftStickRight", true)]
    public void StrictReadersPreservePrefixAndKeyCasePolicies(string mapping, bool expected)
    {
        var controller = EmulationControllerState.Empty with { DeviceId = "pad", LeftX = short.MaxValue };
        var snapshot = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.A }, Controllers = [controller] };
        Assert.Equal(expected, EmulationInputMappingFunctions.IsMappedInputPressed(mapping, controller, snapshot));
    }

    [Theory]
    [InlineData("Left", true)]
    [InlineData("Right", true)]
    [InlineData("Middle", true)]
    [InlineData("XButton1", true)]
    [InlineData("XButton2", true)]
    [InlineData("WheelUp", true)]
    [InlineData("WheelDown", false)]
    [InlineData("WheelLeft", true)]
    [InlineData("WheelRight", false)]
    [InlineData("left", false)]
    [InlineData("Unknown", false)]
    public void MouseNamesAndWheelDirectionsKeepTheirOriginalMeaning(string control, bool expected)
    {
        var snapshot = EmulationInputSnapshot.Empty with { Pointer = new(0, 0, 1, true, true, true, true, true, -1) };
        Assert.Equal(expected, EmulationInputMappingFunctions.IsMappedInputPressed("mOuSe:" + control, EmulationControllerState.Empty, snapshot));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void CombinedSourcesRequireAllParts(bool secondKeyPressed, bool expected)
    {
        var keys = new HashSet<EmulationKey> { EmulationKey.A };
        if (secondKeyPressed) keys.Add(EmulationKey.B);
        var snapshot = EmulationInputSnapshot.Empty with { Keys = keys };
        Assert.Equal(expected, EmulationInputMappingFunctions.IsMappedInputPressed("Keyboard:A+Keyboard:B", EmulationControllerState.Empty, snapshot));
        Assert.Equal(expected ? 1f : 0f, EmulationInputMappingFunctions.MappedInputMagnitude("Keyboard:A+Keyboard:B", EmulationControllerState.Empty, snapshot, 20));
    }

    [Theory]
    [InlineData(0f, 20, 0f)]
    [InlineData(.2f, 20, 0f)]
    [InlineData(.21f, 20, .21f)]
    [InlineData(.8f, 20, .8f)]
    [InlineData(1.5f, 20, 0f)]
    [InlineData(-.5f, 20, 0f)]
    [InlineData(1f, 100, 0f)]
    public void AnalogMagnitudeKeepsTheInclusiveDeadZoneAndDoesNotRescale(float value, int deadZone, float expected)
    {
        var controller = EmulationControllerState.Empty with
        {
            DeviceId = "pad",
            Controls = new(new Dictionary<string, float> { ["LeftStickRight"] = value })
        };
        var snapshot = EmulationInputSnapshot.Empty with { Controllers = [controller] };
        Assert.Equal(expected, EmulationInputMappingFunctions.MappedInputMagnitude("Controller:pad:LeftStickRight", controller, snapshot, deadZone), 6);
    }

    [Fact]
    public void DigitalSourcesKeepFullMagnitudeRegardlessOfAnalogDeadZone()
    {
        var snapshot = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.A } };
        Assert.Equal(1f, EmulationInputMappingFunctions.MappedInputMagnitude("Keyboard:A", EmulationControllerState.Empty, snapshot, 100));
    }

    [Theory]
    [InlineData(-2f, -32767)]
    [InlineData(-1f, -32767)]
    [InlineData(-.5f, -16383)]
    [InlineData(0f, 0)]
    [InlineData(.5f, 16383)]
    [InlineData(1f, 32767)]
    [InlineData(2f, 32767)]
    public void NormalizedAxesKeepTruncationRatherThanRounding(float value, int expected)
        => Assert.Equal((short)expected, EmulationInputMappingFunctions.NormalizedAxis(value));
}