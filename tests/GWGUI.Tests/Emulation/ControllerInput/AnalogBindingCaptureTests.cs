using GWGUI.App.Enums.Input;
using GWGUI.App.Views.Controls.Emulation.Input;
using GWGUI.Emulation.Contracts;
using GWGUI.Tests.Application.TestInfrastructure;

namespace GWGUI.Tests.Emulation.ControllerInput;

[Collection("WPF")]
public sealed class AnalogBindingCaptureTests(StaExecutionScenarios sta)
{
    [Fact]
    public void GenericAxisCaptureUsesItsStartingPosition()
    {
        var baseline = EmulationControllerState.Empty with { Controls = new(new Dictionary<string, float> { ["Axis0"] = .5f }) };
        foreach (var position in new[] { .55f, .65f, .75f })
        {
            var current = baseline with { Controls = new(new Dictionary<string, float> { ["Axis0"] = position }) };
            Assert.Null(InputBindingEditor.NewlyMovedGenericAxis(current, baseline));
        }
        var moved = baseline with { Controls = new(new Dictionary<string, float> { ["Axis0"] = .9f }) };
        Assert.Equal("Axis0Positive", InputBindingEditor.NewlyMovedGenericAxis(moved, baseline));
    }

    [Theory]
    [InlineData("Controller:pad:LeftStickLeft", "Controller:pad:LeftStickRight")]
    [InlineData("Controller:pad:Axis0Negative", "Controller:pad:Axis0Positive")]
    [InlineData("Keyboard:A", "Keyboard:B")]
    [InlineData("Controller:pad:ButtonA", "Keyboard:B")]
    public Task CapturedAxisPairsOppositeWhileKeysAndButtonsRemainIndependent(string captured, string expected) => sta.Run(() =>
    {
        var editor = new InputBindingEditor();
        try
        {
            editor.ConfigureCaptureSources(InputCaptureSources.Keyboard | InputCaptureSources.Controller, true);
            editor.SetRows([
                new("Left", "Left", string.Empty) { OppositeDirectionId = "Right" },
                new("Right", "Right", string.Empty) { OppositeDirectionId = "Left" }
            ], new Dictionary<string, string> { ["Left"] = captured, ["Right"] = "Keyboard:B" });
            editor.ApplyOppositeAxisBinding(editor.Rows[0]);
            Assert.Equal(captured, editor.Rows[0].Binding);
            Assert.Equal(expected, editor.Rows[1].Binding);
        }
        finally
        {
            editor.SetRows([], null);
        }

    });
}
