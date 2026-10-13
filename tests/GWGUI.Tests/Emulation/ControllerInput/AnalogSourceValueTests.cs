using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation.ControllerInput;

public sealed class AnalogSourceValueTests
{
    [Theory]
    [InlineData("Controller:pad:LeftTrigger")]
    [InlineData("Controller:pad:L2")]
    public void TriggerKeepsIntermediatePressure(string source)
    {
        var state = EmulationControllerState.Empty with { LeftTrigger = 8192, Buttons = 1u << 12 };
        Assert.Equal(8192f / short.MaxValue, EmulationInputMappingFunctions.ControllerSourceValue(source, state));
        Assert.False(EmulationInputMappingFunctions.IsControllerSourcePressed(source, state));
    }

    [Theory]
    [InlineData(.5f, 0f, 0f)]
    [InlineData(.625f, .25f, 0f)]
    [InlineData(.375f, 0f, .25f)]
    [InlineData(1f, 1f, 0f)]
    [InlineData(0f, 0f, 1f)]
    public void GenericAxisKeepsBothHalfCourses(float physical, float positive, float negative)
    {
        var state = EmulationControllerState.Empty with { Controls = new(new Dictionary<string, float> { ["Axis0"] = physical }) };
        Assert.Equal(positive, EmulationInputMappingFunctions.ControllerSourceValue("Controller:pad:Axis0Positive", state));
        Assert.Equal(negative, EmulationInputMappingFunctions.ControllerSourceValue("Controller:pad:Axis0Negative", state));
        Assert.Equal(positive > .5f, EmulationInputMappingFunctions.IsControllerSourcePressed("Controller:pad:Axis0Positive", state));
        Assert.Equal(negative > .5f, EmulationInputMappingFunctions.IsControllerSourcePressed("Controller:pad:Axis0Negative", state));
    }

    [Theory]
    [InlineData("Controller:pad:LeftStickLeft", "Controller:pad:LeftStickRight")]
    [InlineData("Controller:pad:RightStickUp", "Controller:pad:RightStickDown")]
    [InlineData("Controller:raw:device:Axis3Positive", "Controller:raw:device:Axis3Negative")]
    [InlineData("Controller:raw:device:Axis3Negative", "Controller:raw:device:Axis3Positive")]
    [InlineData("Keyboard:A", null)]
    [InlineData("Controller:pad:ButtonA", null)]
    [InlineData("Controller:pad:LeftTrigger", null)]
    public void OppositeAxisKeepsDeviceAndDoesNotPairButtons(string source, string? opposite)
    {
        Assert.Equal(opposite, EmulationInputMappingFunctions.OppositeControllerAxisSource(source));
        if (opposite is not null) Assert.Equal(source, EmulationInputMappingFunctions.OppositeControllerAxisSource(opposite));
    }

    [Fact]
    public void DigitalTriggerRemainsUsable()
    {
        var state = EmulationControllerState.Empty with { Buttons = 1u << 12 };
        Assert.Equal(1f, EmulationInputMappingFunctions.ControllerSourceValue("Controller:pad:LeftTrigger", state));
    }
}
