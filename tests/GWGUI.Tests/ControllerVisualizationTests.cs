using GWGUI.App.Services.Input.GameInput;
using GWGUI.App.Views.Controls.Options;
using System.IO;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GWGUI.Tests;

[Collection("GameInput hardware")]
public sealed class ControllerVisualizationTests
{
    [Fact]
    public void GenericControllerAxesAreCenteredAndButtonsAndSwitchesAreVisible()
    {
        WpfTestHost.Run(() =>
        {
            var state = new GameInputLiveState(
                "gameinput:raw", 1, GameInputKind.Controller,
                [
                    new(GameInputControlType.Axis, 0, GameInputLabel.None, 0f),
                    new(GameInputControlType.Axis, 1, GameInputLabel.None, 0f),
                    new(GameInputControlType.Axis, 2, GameInputLabel.None, 1f),
                    new(GameInputControlType.Axis, 3, GameInputLabel.None, -1f),
                    new(GameInputControlType.Button, 0, GameInputLabel.None, 1f),
                    new(GameInputControlType.Switch, 0, GameInputLabel.None, 1f, GameInputSwitchPosition.Up)
                ], [], GameInputSystemButtons.None, null, null, null, null);
            var visualizer = new ControllerVisualizer { Model = ControllerVisualModel.GenericGamepad, State = state };

            var snapshot = visualizer.GetSnapshotForTest();

            Assert.Equal(0f, snapshot.LeftX, 3);
            Assert.Equal(0f, snapshot.LeftY, 3);
            Assert.Equal(1f, snapshot.RightX, 3);
            Assert.Equal(-1f, snapshot.RightY, 3);
            Assert.True(snapshot.PrimaryPressed);
            Assert.True(snapshot.DPadUpPressed);
        });
    }

    [Fact]
    public void MissingRawControlsAreReleasedAndCentered()
    {
        WpfTestHost.Run(() =>
        {
            var visualizer = new ControllerVisualizer
            {
                Model = ControllerVisualModel.GenericGamepad,
                State = GameInputLiveState.Empty("gameinput:empty")
            };

            var snapshot = visualizer.GetSnapshotForTest();

            Assert.False(snapshot.PrimaryPressed);
            Assert.False(snapshot.DPadUpPressed);
            Assert.Equal(0f, snapshot.LeftX, 3);
            Assert.Equal(0f, snapshot.LeftY, 3);
            Assert.Equal(0f, snapshot.RightX, 3);
            Assert.Equal(0f, snapshot.RightY, 3);
            Assert.Equal(0f, snapshot.LeftTrigger, 3);
            Assert.Equal(0f, snapshot.RightTrigger, 3);
            Assert.Equal(0f, snapshot.Wheel, 3);
            Assert.Equal(0f, snapshot.Throttle, 3);
            Assert.Equal(0f, snapshot.Brake, 3);
            Assert.Equal(0f, snapshot.Clutch, 3);
        });
    }

    [Fact]
    public void NeutralStandardGamepadDoesNotActivateDpadFromCenteredControllerAxes()
    {
        WpfTestHost.Run(() =>
        {
            var state = new GameInputLiveState(
                "gameinput:neutral-standard", 2,
                GameInputKind.ControllerAxis | GameInputKind.ControllerButton | GameInputKind.Gamepad,
                [
                    new(GameInputControlType.Axis, 0, GameInputLabel.None, 0f),
                    new(GameInputControlType.Axis, 1, GameInputLabel.None, 0f)
                ], [], GameInputSystemButtons.None, null, null,
                new GameInputGamepadState { Buttons = GameInputGamepadButtons.None }, null);
            var visualizer = new ControllerVisualizer { Model = ControllerVisualModel.XboxOne, State = state };

            var snapshot = visualizer.GetSnapshotForTest();

            Assert.False(snapshot.DPadUpPressed);
            Assert.Equal(0f, snapshot.LeftX, 3);
            Assert.Equal(0f, snapshot.LeftY, 3);
        });
    }

    [Fact]
    public void StandardGamepadSignalsRemainSignedAndAnalog()
    {
        WpfTestHost.Run(() =>
        {
            var state = new GameInputLiveState(
                "gameinput:standard", 2, GameInputKind.Gamepad, [], [], GameInputSystemButtons.None,
                null, null,
                new GameInputGamepadState
                {
                    Buttons = GameInputGamepadButtons.A | GameInputGamepadButtons.DPadUp,
                    LeftThumbstickX = -.75f,
                    LeftThumbstickY = .25f,
                    RightThumbstickX = .5f,
                    RightThumbstickY = -1f,
                    LeftTrigger = .33f,
                    RightTrigger = .66f
                }, null);
            var visualizer = new ControllerVisualizer { Model = ControllerVisualModel.XboxSeries, State = state };

            var snapshot = visualizer.GetSnapshotForTest();

            Assert.Equal(-.75f, snapshot.LeftX, 3);
            Assert.Equal(-.25f, snapshot.LeftY, 3);
            Assert.Equal(.5f, snapshot.RightX, 3);
            Assert.Equal(1f, snapshot.RightY, 3);
            Assert.Equal(.33f, snapshot.LeftTrigger, 3);
            Assert.Equal(.66f, snapshot.RightTrigger, 3);
            Assert.True(snapshot.PrimaryPressed);
            Assert.True(snapshot.DPadUpPressed);
        });
    }


    [Fact]
    public void RawHidAxesDriveTheDpadAndKeepScreenDirection()
    {
        WpfTestHost.Run(() =>
        {
            var state = new GameInputLiveState(
                "gameinput:hid", 4, GameInputKind.RawDeviceReport,
                [
                    new(GameInputControlType.Axis, 0, GameInputLabel.None, .5f),
                    new(GameInputControlType.Axis, 1, GameInputLabel.None, 0f)
                ], [], GameInputSystemButtons.None, null, null, null, null,
                ControlsUseNormalizedAxes: true);
            var visualizer = new ControllerVisualizer { Model = ControllerVisualModel.MegaDrive6, State = state };

            var snapshot = visualizer.GetSnapshotForTest();

            Assert.Equal(0f, snapshot.LeftX, 3);
            Assert.Equal(-1f, snapshot.LeftY, 3);
            Assert.True(snapshot.DPadUpPressed);
        });
    }

    [Fact]
    public void HidAxesRemainNormalizedWhenTheReadingAlsoAdvertisesControllerAxes()
    {
        WpfTestHost.Run(() =>
        {
            var centered = new GameInputLiveState(
                "gameinput:hid-controller-flags", 5,
                GameInputKind.RawDeviceReport | GameInputKind.ControllerAxis,
                [
                    new(GameInputControlType.Axis, 0, GameInputLabel.None, .5f),
                    new(GameInputControlType.Axis, 1, GameInputLabel.None, .5f)
                ], [], GameInputSystemButtons.None, null, null, null, null,
                ControlsUseNormalizedAxes: true);
            var visualizer = new ControllerVisualizer { Model = ControllerVisualModel.Nintendo64, State = centered };

            var snapshot = visualizer.GetSnapshotForTest();

            Assert.Equal(0f, snapshot.LeftX, 3);
            Assert.Equal(0f, snapshot.LeftY, 3);
        });
    }

    [Fact]
    public void SuperNintendoSwappedHidAxesRenderAllFourDpadDirections()
    {
        WpfTestHost.Run(() =>
        {
            string Render(float vertical, float horizontal)
            {
                var controls = new[]
                {
                    new GameInputControlValue(GameInputControlType.Axis, 0, GameInputLabel.None, vertical),
                    new GameInputControlValue(GameInputControlType.Axis, 1, GameInputLabel.None, horizontal)
                };
                var state = new GameInputLiveState(
                    "gameinput:snes-directions", 6,
                    GameInputKind.RawDeviceReport | GameInputKind.ControllerAxis,
                    controls, [], GameInputSystemButtons.None, null, null, null, null,
                    ControlsUseNormalizedAxes: true);
                return RenderHash(new ControllerVisualizer
                {
                    Model = ControllerVisualModel.SuperNintendo,
                    State = state
                });
            }

            var idle = Render(.5f, .5f);
            var directions = new[]
            {
                Render(0f, .5f),
                Render(1f, .5f),
                Render(.5f, 0f),
                Render(.5f, 1f)
            };

            Assert.All(directions, hash => Assert.NotEqual(idle, hash));
            Assert.Equal(4, directions.Distinct().Count());
        });
    }

    [Fact]
    public void RacingWheelVisualReactsToWheelPedalsGearDpadAndButtons()
    {
        WpfTestHost.Run(() =>
        {
            var visualizer = new ControllerVisualizer { Model = ControllerVisualModel.RacingWheel };
            var idle = RenderHash(visualizer);
            visualizer.State = new GameInputLiveState(
                "gameinput:wheel", 5, GameInputKind.RacingWheel, [], [],
                GameInputSystemButtons.None, null, null, null,
                new GameInputRacingWheelState
                {
                    Buttons = GameInputRacingWheelButtons.A |
                        GameInputRacingWheelButtons.DPadUp |
                        GameInputRacingWheelButtons.NextGear,
                    PatternShifterGear = 3,
                    Wheel = .75f,
                    Throttle = .8f,
                    Brake = .4f,
                    Clutch = .2f,
                    Handbrake = .6f
                });

            Assert.NotEqual(idle, RenderHash(visualizer));
        });
    }

    [Fact]
    public void FlightStickVisualReactsToRollPitchYawThrottleHatAndFire()
    {
        WpfTestHost.Run(() =>
        {
            var visualizer = new ControllerVisualizer { Model = ControllerVisualModel.FlightStick };
            var idle = RenderHash(visualizer);
            visualizer.State = new GameInputLiveState(
                "gameinput:flight", 6, GameInputKind.FlightStick, [], [],
                GameInputSystemButtons.None, null,
                new GameInputFlightStickState
                {
                    Buttons = GameInputFlightStickButtons.FirePrimary |
                        GameInputFlightStickButtons.FireSecondary,
                    HatSwitch = GameInputSwitchPosition.UpRight,
                    Roll = .6f,
                    Pitch = -.4f,
                    Yaw = .8f,
                    Throttle = .7f
                }, null, null);

            Assert.NotEqual(idle, RenderHash(visualizer));
        });
    }

    [Fact]
    public void ArcadeStickVisualReactsToStandardizedDirectionsAndButtons()
    {
        WpfTestHost.Run(() =>
        {
            var visualizer = new ControllerVisualizer { Model = ControllerVisualModel.ArcadeStick };
            var idle = RenderHash(visualizer);
            visualizer.State = new GameInputLiveState(
                "gameinput:arcade", 7, GameInputKind.ArcadeStick, [], [],
                GameInputSystemButtons.None,
                new GameInputArcadeStickState
                {
                    Buttons = GameInputArcadeStickButtons.Up |
                        GameInputArcadeStickButtons.Right |
                        GameInputArcadeStickButtons.Action1 |
                        GameInputArcadeStickButtons.Special2
                }, null, null, null);

            Assert.NotEqual(idle, RenderHash(visualizer));
        });
    }

    [Fact]
    public void EverySelectableVisualModelRendersASeparateImage()
    {
        WpfTestHost.Run(() =>
        {
            var hashes = new Dictionary<ControllerVisualModel, string>();
            foreach (var model in GameInputDeviceModelCatalog.AllVisualModels)
            {
                var visualizer = new ControllerVisualizer { Model = model };
                hashes.Add(model, RenderHash(visualizer));
            }

            Assert.Equal(GameInputDeviceModelCatalog.AllVisualModels.Count, hashes.Values.Distinct().Count());
        });
    }

    [Fact]
    public void RenderedImageChangesWhenPhysicalSignalsChange()
    {
        WpfTestHost.Run(() =>
        {
            var visualizer = new ControllerVisualizer { Model = ControllerVisualModel.XboxSeries };
            var idle = RenderHash(visualizer);
            visualizer.State = new GameInputLiveState(
                "gameinput:test", 3, GameInputKind.Gamepad, [], [], GameInputSystemButtons.None,
                null, null,
                new GameInputGamepadState
                {
                    Buttons = GameInputGamepadButtons.A,
                    LeftThumbstickX = 1f,
                    LeftTrigger = 1f
                }, null);
            var active = RenderHash(visualizer);

            Assert.NotEqual(idle, active);
        });
    }

    [Fact]
    public void AllModelsCanBeCapturedAsReferenceSheet()
    {
        WpfTestHost.Run(() =>
        {
            const int columns = 2;
            const int cellWidth = 620;
            const int cellHeight = 350;
            var models = GameInputDeviceModelCatalog.AllVisualModels;
            var rows = (int)Math.Ceiling(models.Count / (double)columns);
            var grid = new System.Windows.Controls.Grid
            {
                Width = columns * cellWidth,
                Height = rows * cellHeight,
                Background = new SolidColorBrush(Color.FromRgb(18, 20, 23))
            };
            for (var column = 0; column < columns; column++)
                grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new GridLength(cellWidth) });
            for (var row = 0; row < rows; row++)
                grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(cellHeight) });

            for (var index = 0; index < models.Count; index++)
            {
                var panel = new System.Windows.Controls.Grid();
                panel.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(30) });
                panel.RowDefinitions.Add(new System.Windows.Controls.RowDefinition());
                var label = new System.Windows.Controls.TextBlock
                {
                    Text = models[index].ToString(),
                    Foreground = Brushes.White,
                    FontSize = 16,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                var visualizer = new ControllerVisualizer { Model = models[index] };
                if (models[index] == ControllerVisualModel.Nintendo64)
                {
                    visualizer.State = new GameInputLiveState(
                        "raw:n64-preview", 1, GameInputKind.Controller,
                        [
                            new(GameInputControlType.Axis, 0, GameInputLabel.None, .82f),
                            new(GameInputControlType.Axis, 1, GameInputLabel.None, .18f),
                            new(GameInputControlType.Switch, 0, GameInputLabel.None, 1f, GameInputSwitchPosition.UpRight),
                            .. new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }
                                .Select(button => new GameInputControlValue(GameInputControlType.Button, button, GameInputLabel.None, 1f))
                        ], [], GameInputSystemButtons.None, null, null, null, null, true);
                }
                else if (models[index] == ControllerVisualModel.SuperNintendo)
                {
                    visualizer.State = new GameInputLiveState(
                        "raw:snes-preview", 1, GameInputKind.Controller,
                        [
                            new(GameInputControlType.Axis, 0, GameInputLabel.None, .15f),
                            new(GameInputControlType.Axis, 1, GameInputLabel.None, .85f),
                            .. new[] { 0, 1, 2, 3, 4, 5, 8, 9 }
                                .Select(button => new GameInputControlValue(GameInputControlType.Button, button, GameInputLabel.None, 1f))
                        ], [], GameInputSystemButtons.None, null, null, null, null, true);
                }
                System.Windows.Controls.Grid.SetRow(visualizer, 1);
                panel.Children.Add(label);
                panel.Children.Add(visualizer);
                System.Windows.Controls.Grid.SetColumn(panel, index % columns);
                System.Windows.Controls.Grid.SetRow(panel, index / columns);
                grid.Children.Add(panel);
            }

            grid.Measure(new Size(grid.Width, grid.Height));
            grid.Arrange(new Rect(0, 0, grid.Width, grid.Height));
            grid.UpdateLayout();
            var bitmap = new RenderTargetBitmap((int)grid.Width, (int)grid.Height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(grid);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            var path = Path.Combine(RepositoryRoot(), "tmp", "captures",
                "controller-models-reference-20260823.png");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using var stream = File.Create(path);
            encoder.Save(stream);
            Assert.True(new FileInfo(path).Length > 10_000);
        });
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "src")) &&
                Directory.Exists(Path.Combine(directory.FullName, "tests")))
                return directory.FullName;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("GW GUI repository root was not found.");
    }

    private static string RenderHash(ControllerVisualizer visualizer)
    {
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
