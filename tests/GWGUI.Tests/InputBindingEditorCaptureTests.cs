using GWGUI.App.Views.Controls.Emulation.Input;
using GWGUI.App.Services.Input.GameInput;
using GWGUI.Emulation;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace GWGUI.Tests;

public sealed class InputBindingEditorCaptureTests
{
    [Fact]
    public void CaptureExpiresWithoutChangingTheExistingAssignment()
    {
        WpfTestHost.Run(() =>
        {
            var editor = new InputBindingEditor();
            editor.SetRows([new InputBindingDefinition("fire", string.Empty, "Space", "Tir")],
                new Dictionary<string, string> { ["fire"] = "Space" });
            var row = Assert.Single(editor.Rows);
            var button = new Button { Tag = row };
            Invoke(editor, "AssignClicked", button, new RoutedEventArgs());
            SetField(editor, "_captureDeadlineUtc", DateTime.UtcNow.AddSeconds(-1));

            Invoke(editor, "CaptureControllerInput", null, EventArgs.Empty);

            Assert.Equal("Space", row.Binding);
            Assert.Null(GetField(editor, "_captureRow"));
        });
    }

    [Fact]
    public void GenericAxisCanBeCapturedInBothDirections()
    {
        var baseline = EmulationControllerState.Empty with
        {
            DeviceId = "mega-drive",
            Controls = new EmulationControllerControls(new Dictionary<string, float>
            {
                ["Axis0"] = .5f
            })
        };
        var positive = baseline with
        {
            Controls = new EmulationControllerControls(new Dictionary<string, float>
            {
                ["Axis0"] = 1f
            })
        };
        var negative = baseline with
        {
            Controls = new EmulationControllerControls(new Dictionary<string, float>
            {
                ["Axis0"] = 0f
            })
        };

        Assert.Equal("Axis0Positive", InputBindingEditor.NewlyMovedGenericAxis(positive, baseline));
        Assert.Equal("Axis0Negative", InputBindingEditor.NewlyMovedGenericAxis(negative, baseline));
        Assert.True(EmulationInputMappingFunctions.IsControllerSourcePressed(
            "Controller:mega-drive:Axis0Positive", positive));
        Assert.True(EmulationInputMappingFunctions.IsControllerSourcePressed(
            "Controller:mega-drive:Axis0Negative", negative));
        Assert.False(EmulationInputMappingFunctions.IsControllerSourcePressed(
            "Controller:mega-drive:Axis0Positive", negative));
    }

    [Fact]
    public void DetailedRawControllerButtonsUseTheSameIndexAsTheEmulationReader()
    {
        var baseline = GameInputLiveState.Empty("rawgamecontroller:mega");
        var pressed = baseline with
        {
            Controls =
            [
                new GameInputControlValue(GameInputControlType.Button, 0,
                    GameInputLabel.None, 1f)
            ]
        };

        Assert.Equal("Button1", InputBindingEditor.NewlyActivatedDetailedControl(pressed, baseline));
    }

    [Fact]
    public void DetailedRawControllerAxesKeepTheirDirection()
    {
        var baseline = GameInputLiveState.Empty("rawgamecontroller:mega") with
        {
            Controls =
            [
                new GameInputControlValue(GameInputControlType.Axis, 0,
                    GameInputLabel.None, .5f)
            ]
        };
        var pressed = baseline with
        {
            Controls =
            [
                new GameInputControlValue(GameInputControlType.Axis, 0,
                    GameInputLabel.None, 0f)
            ]
        };

        Assert.Equal("Axis0Negative",
            InputBindingEditor.NewlyActivatedDetailedControl(pressed, baseline));
    }

    private static void Invoke(object target, string name, params object?[] arguments) =>
        target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(target, arguments);

    private static void SetField(object target, string name, object value) =>
        target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(target, value);

    private static object? GetField(object target, string name) =>
        target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(target);
}
