using GWGUI.Emulation;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class ControllerDeadZoneStateTests
{
    [Fact]
    public void SixSignedAxesUseTheOriginalIntegerThresholdWithoutLosingMetadata()
    {
        var controls = new EmulationControllerControls(new Dictionary<string, float> { ["auxiliary-axis"] = 0.75f });
        var source = new EmulationControllerState(0xDEADBEEFu, 16383, 16384, -16383, -16384, short.MinValue, 0)
        {
            DeviceId = "gamepad-1",
            Controls = controls
        };

        var result = EmulationSdk.InputProcessing.ApplyDeadZones(source, 50);

        Assert.Equal(new short[] { 0, 16384, 0, -16384, short.MinValue, 0 }, Axes(result));
        Assert.Equal(new short[] { 16383, 16384, -16383, -16384, short.MinValue, 0 }, Axes(source));
        Assert.Equal(source.Buttons, result.Buttons);
        Assert.Equal("gamepad-1", result.DeviceId);
        Assert.Same(controls, result.Controls);
        Assert.Equal(0.75f, result.Controls["auxiliary-axis"]);
        Assert.NotSame(source, result);
    }

    [Fact]
    public void ZeroPercentStillCopiesTheRecordAndPreservesItsValues()
    {
        var source = new EmulationControllerState(1, 1, -1, 2, -2, 3, -3);
        var result = EmulationSdk.InputProcessing.ApplyDeadZones(source, 0);
        Assert.Equal(source, result);
        Assert.NotSame(source, result);
        Assert.Same(source.Controls, result.Controls);
    }

    [Theory]
    [InlineData(100, short.MinValue)]
    [InlineData(101, (short)0)]
    public void SignedExtremesKeepTheExistingScalarBehavior(int percent, short expectedNegative)
    {
        var source = new EmulationControllerState(3, short.MinValue, short.MaxValue,
            short.MinValue, short.MaxValue, short.MinValue, short.MaxValue);
        var result = EmulationSdk.InputProcessing.ApplyDeadZones(source, percent);
        Assert.Equal(new short[] { expectedNegative, 0, expectedNegative, 0, expectedNegative, 0 }, Axes(result));
        Assert.Equal(source.Buttons, result.Buttons);
    }

    private static short[] Axes(EmulationControllerState state) =>
        [state.LeftX, state.LeftY, state.RightX, state.RightY, state.LeftTrigger, state.RightTrigger];
}
