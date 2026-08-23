using GWGUI.App.Services.Input.GameInput;
using GWGUI.App.Views.Controls.Options;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GWGUI.Tests;

[Collection("GameInput hardware")]
public sealed class ControllerSignalVisualizationTests
{
    [Fact]
    public void EveryPhysicalXboxSeriesButtonChangesTheXboxSeriesVisual()
    {
        WpfTestHost.Run(() =>
        {
            var idle = Hash(ControllerVisualModel.XboxSeries, State(gamepad: new()));
            foreach (var button in SingleBits<GameInputGamepadButtons>()
                         .Where(button => button is not GameInputGamepadButtons.C and not GameInputGamepadButtons.Z))
            {
                var active = Hash(ControllerVisualModel.XboxSeries,
                    State(gamepad: new GameInputGamepadState { Buttons = button }));
                Assert.False(string.Equals(idle, active, StringComparison.Ordinal),
                    $"Gamepad signal {button} did not change the Xbox Series visual.");
            }
        });
    }

    [Fact]
    public void EverySystemButtonChangesTheXboxSeriesVisual()
    {
        WpfTestHost.Run(() =>
        {
            var idle = Hash(ControllerVisualModel.XboxSeries, State(gamepad: new()));
            foreach (var button in SingleBits<GameInputSystemButtons>())
                Assert.NotEqual(idle, Hash(ControllerVisualModel.XboxSeries,
                    State(systemButtons: button, gamepad: new())));
        });
    }

    [Fact]
    public void EveryRacingWheelButtonChangesTheWheelVisual()
    {
        WpfTestHost.Run(() =>
        {
            var idle = Hash(ControllerVisualModel.RacingWheel, State(wheel: new()));
            foreach (var button in SingleBits<GameInputRacingWheelButtons>())
                Assert.NotEqual(idle, Hash(ControllerVisualModel.RacingWheel,
                    State(wheel: new GameInputRacingWheelState { Buttons = button })));
        });
    }

    [Fact]
    public void EveryFlightStickButtonChangesTheFlightStickVisual()
    {
        WpfTestHost.Run(() =>
        {
            var idle = Hash(ControllerVisualModel.FlightStick, State(flight: new()));
            foreach (var button in SingleBits<GameInputFlightStickButtons>())
                Assert.NotEqual(idle, Hash(ControllerVisualModel.FlightStick,
                    State(flight: new GameInputFlightStickState { Buttons = button })));
        });
    }

    [Fact]
    public void EveryArcadeStickButtonChangesTheArcadeStickVisual()
    {
        WpfTestHost.Run(() =>
        {
            var idle = Hash(ControllerVisualModel.ArcadeStick, State(arcade: new()));
            foreach (var button in SingleBits<GameInputArcadeStickButtons>())
                Assert.NotEqual(idle, Hash(ControllerVisualModel.ArcadeStick,
                    State(arcade: new GameInputArcadeStickState { Buttons = button })));
        });
    }

    [Fact]
    public void EveryGamepadAnalogValueChangesTheImage()
    {
        WpfTestHost.Run(() =>
        {
            var idle = Hash(ControllerVisualModel.XboxSeries, State(gamepad: new()));
            var values = new[]
            {
                ("LeftTrigger", new GameInputGamepadState { LeftTrigger = .75f }),
                ("RightTrigger", new GameInputGamepadState { RightTrigger = .75f }),
                ("LeftThumbstickX", new GameInputGamepadState { LeftThumbstickX = .75f }),
                ("LeftThumbstickY", new GameInputGamepadState { LeftThumbstickY = .75f }),
                ("RightThumbstickX", new GameInputGamepadState { RightThumbstickX = .75f }),
                ("RightThumbstickY", new GameInputGamepadState { RightThumbstickY = .75f })
            };
            foreach (var (name, value) in values)
                Assert.False(string.Equals(idle,
                    Hash(ControllerVisualModel.XboxSeries, State(gamepad: value)),
                    StringComparison.Ordinal), $"Gamepad axis {name} did not change the visual.");
        });
    }

    [Fact]
    public void EveryWheelAnalogValueAndGearChangesTheImage()
    {
        WpfTestHost.Run(() =>
        {
            var idle = Hash(ControllerVisualModel.RacingWheel, State(wheel: new()));
            var values = new[]
            {
                new GameInputRacingWheelState { Wheel = .75f },
                new GameInputRacingWheelState { Throttle = .75f },
                new GameInputRacingWheelState { Brake = .75f },
                new GameInputRacingWheelState { Clutch = .75f },
                new GameInputRacingWheelState { Handbrake = .75f },
                new GameInputRacingWheelState { PatternShifterGear = 3 }
            };
            Assert.All(values, value =>
                Assert.NotEqual(idle, Hash(ControllerVisualModel.RacingWheel, State(wheel: value))));
        });
    }

    [Fact]
    public void EveryFlightAnalogValueAndHatDirectionChangesTheImage()
    {
        WpfTestHost.Run(() =>
        {
            var idle = Hash(ControllerVisualModel.FlightStick, State(flight: new()));
            var values = new[]
            {
                new GameInputFlightStickState { Roll = .75f },
                new GameInputFlightStickState { Pitch = .75f },
                new GameInputFlightStickState { Yaw = .75f },
                new GameInputFlightStickState { Throttle = .75f }
            };
            Assert.All(values, value =>
                Assert.NotEqual(idle, Hash(ControllerVisualModel.FlightStick, State(flight: value))));
            foreach (var direction in Enum.GetValues<GameInputSwitchPosition>()
                         .Where(value => value != GameInputSwitchPosition.Center))
                Assert.NotEqual(idle, Hash(ControllerVisualModel.FlightStick,
                    State(flight: new GameInputFlightStickState { HatSwitch = direction })));
        });
    }

    [Fact]
    public void EveryRawRetroButtonChangesItsModelImage()
    {
        WpfTestHost.Run(() =>
        {
            var cases = new[]
            {
                (ControllerVisualModel.MasterSystem, new[] { 0, 1 }),
                (ControllerVisualModel.NintendoEntertainmentSystem, new[] { 0, 1, 8, 9 }),
                (ControllerVisualModel.Nintendo64, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }),
                (ControllerVisualModel.SuperNintendo, new[] { 0, 1, 2, 3, 4, 5, 8, 9 }),
                (ControllerVisualModel.MegaDrive3, new[] { 0, 1, 2, 3 }),
                (ControllerVisualModel.MegaDrive6, new[] { 0, 1, 2, 3, 4, 5, 8, 9 }),
                (ControllerVisualModel.Saturn, Enumerable.Range(0, 9).ToArray())
            };
            foreach (var (model, indices) in cases)
            {
                var idle = Hash(model, State());
                foreach (var index in indices)
                {
                    var controls = new[]
                    {
                        new GameInputControlValue(
                            GameInputControlType.Button, index, GameInputLabel.None, 1f)
                    };
                    Assert.NotEqual(idle, Hash(model, State(controls: controls)));
                }
            }
        });
    }

    [Fact]
    public void EveryGamepadShapedModelRespondsToAStandardButton()
    {
        WpfTestHost.Run(() =>
        {
            var models = new[]
            {
                ControllerVisualModel.GenericGamepad,
                ControllerVisualModel.XboxSeries,
                ControllerVisualModel.XboxOne,
                ControllerVisualModel.XboxRematchCore,
                ControllerVisualModel.PlayStation4,
                ControllerVisualModel.PlayStation5,
                ControllerVisualModel.PlayStation1,
                ControllerVisualModel.PlayStation2,
                ControllerVisualModel.Dreamcast
            };
            foreach (var model in models)
            {
                var idle = Hash(model, State(gamepad: new()));
                var active = Hash(model, State(gamepad: new GameInputGamepadState
                {
                    Buttons = GameInputGamepadButtons.A
                }));
                Assert.NotEqual(idle, active);
            }
        });
    }

    private static IEnumerable<T> SingleBits<T>() where T : struct, Enum =>
        Enum.GetValues<T>().Where(value =>
        {
            var bits = Convert.ToUInt64(value);
            return bits != 0 && (bits & (bits - 1)) == 0;
        });

    private static GameInputLiveState State(
        IReadOnlyList<GameInputControlValue>? controls = null,
        GameInputSystemButtons systemButtons = GameInputSystemButtons.None,
        GameInputArcadeStickState? arcade = null,
        GameInputFlightStickState? flight = null,
        GameInputGamepadState? gamepad = null,
        GameInputRacingWheelState? wheel = null) =>
        new("gameinput:signals", 1,
            arcade is not null ? GameInputKind.ArcadeStick :
            flight is not null ? GameInputKind.FlightStick :
            wheel is not null ? GameInputKind.RacingWheel :
            gamepad is not null ? GameInputKind.Gamepad :
            GameInputKind.Controller,
            controls ?? [], [], systemButtons, arcade, flight, gamepad, wheel);

    private static string Hash(ControllerVisualModel model, GameInputLiveState state)
    {
        var visualizer = new ControllerVisualizer { Model = model, State = state };
        visualizer.Measure(new Size(620, 320));
        visualizer.Arrange(new Rect(0, 0, 620, 320));
        visualizer.UpdateLayout();
        var bitmap = new RenderTargetBitmap(620, 320, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visualizer);
        var bytes = new byte[620 * 320 * 4];
        bitmap.CopyPixels(bytes, 620 * 4, 0);
        return Convert.ToHexString(SHA256.HashData(bytes));
    }
}
