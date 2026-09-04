using GWGUI.App.Functions.Input.Controllers;
using GWGUI.App.Services.Input.GameInput;
using GWGUI.Emulation.Contracts;

namespace GWGUI.Tests;

public sealed class ControllerAnalogDeadZoneFunctionsTests
{
    [Fact]
    public void OneProfileProcessesBothSticksAndBothTriggers()
    {
        var state = new EmulationControllerState(
            (1u << 12) | (1u << 13),
            Axis(.10f), Axis(.60f), Axis(-.10f), Axis(-.60f),
            Axis(.10f), Axis(.55f))
        {
            DeviceId = "controller"
        };

        var result = ControllerAnalogDeadZoneFunctions.Apply(
            state, new ControllerAnalogDeadZoneProfile(20, 10, 0));

        Assert.InRange(result.LeftX / (float)short.MaxValue, .08f, .09f);
        Assert.InRange(result.LeftY / (float)short.MaxValue, .49f, .51f);
        Assert.InRange(result.RightX / (float)short.MaxValue, -.09f, -.08f);
        Assert.InRange(result.RightY / (float)short.MaxValue, -.51f, -.49f);
        Assert.Equal(0, result.LeftTrigger);
        Assert.InRange(result.RightTrigger / (float)short.MaxValue, .49f, .51f);
        Assert.Equal(0u, result.Buttons & (1u << 12));
        Assert.NotEqual(0u, result.Buttons & (1u << 13));
    }

    [Fact]
    public void StickInsideTheRadialDeadZoneReturnsToCenter()
    {
        var state = new EmulationControllerState(0, Axis(.10f), Axis(.10f), 0, 0, 0, 0);

        var result = ControllerAnalogDeadZoneFunctions.Apply(
            state, new ControllerAnalogDeadZoneProfile(20, 0, 0));

        Assert.Equal(0, result.LeftX);
        Assert.Equal(0, result.LeftY);
    }

    [Fact]
    public void OuterDeadZoneMakesTheEndOfTheStickReachFullScale()
    {
        var state = new EmulationControllerState(0, Axis(.85f), 0, 0, 0, 0, 0);

        var result = ControllerAnalogDeadZoneFunctions.Apply(
            state, new ControllerAnalogDeadZoneProfile(0, 0, 20));

        Assert.Equal(short.MaxValue, result.LeftX);
    }

    private static short Axis(float value) =>
        (short)Math.Round(Math.Clamp(value, -1f, 1f) * short.MaxValue);
}
