using GWGUI.App.Localization.Extensions;
using GWGUI.App.Services.Input.GameInput;
using GWGUI.App.Views.Controls.Options;
using GWGUI.App.Views.Controls.Options.ControllerPresentation;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace GWGUI.Tests;

[Collection("GameInput hardware")]
public sealed class GameInputControllersSectionTests
{
    [Fact]
    public void InteropLayoutMatchesGameInputThreePointFive()
    {
        Assert.Equal(256, Marshal.SizeOf<GameInputDeviceInfo>());
        Assert.Equal(48, Marshal.SizeOf<GameInputControllerInfo>());
        Assert.Equal(76, Marshal.SizeOf<GameInputGamepadInfo>());
        Assert.Equal(12, Marshal.SizeOf<GameInputRawDeviceReportInfo>());
        Assert.Equal(0x00000040u, (uint)GameInputKind.Sensors);
        Assert.Equal(0x01000000u, (uint)GameInputKind.UiNavigation);
        Assert.Contains(nameof(GameInputKind.UiNavigation), ((GameInputKind)0x01040007).ToString());
        Assert.Equal(0x00200000u, (uint)GameInputDeviceStatus.HapticInfoReady);
        Assert.Equal(124, (int)GameInputLabel.PaddleRight2);
    }

    [Fact]
    public void InjectedControllerPopulatesSelectorLiveControlsAndDetection()
    {
        WpfTestHost.RunAsync(async () =>
        {
            var descriptor = CreateDescriptor();
            var state = new GameInputLiveState(
                descriptor.Id,
                42,
                GameInputKind.Gamepad,
                [
                    new GameInputControlValue(
                        GameInputControlType.Button,
                        0,
                        GameInputLabel.XboxA,
                        1f)
                ],
                [],
                GameInputSystemButtons.None,
                null,
                null,
                new GameInputGamepadState
                {
                    Buttons = GameInputGamepadButtons.A,
                    LeftThumbstickX = .75f,
                    LeftThumbstickY = -.5f
                },
                null);
            var source = new FakeControllerSource(descriptor, state);
            var section = new OptionsControllersSection(source);

            await section.RefreshDevicesAsync(force: false);
            typeof(OptionsControllersSection).GetMethod("RefreshLiveState", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(section, null);

            var selector = Assert.IsType<ComboBox>(section.FindName("DeviceSelector"));
            Assert.Single(selector.Items);
            Assert.Same(descriptor, selector.SelectedItem);
            Assert.Equal(
                "Injected Xbox Series",
                Assert.IsType<TextBlock>(section.FindName("ProductNameText")).Text);

            var controls = Assert.IsType<DataGrid>(section.FindName("ControlsGrid"));
            Assert.Single(controls.ItemsSource.Cast<object>());
            var visualizer = Assert.IsType<ControllerVisualizer>(section.FindName("Visualizer"));
            Assert.Equal(GameInputGamepadButtons.A, visualizer.State?.Gamepad?.Buttons);
            Assert.Equal(Visibility.Collapsed,
                Assert.IsType<Grid>(section.FindName("ModelSelectorPanel")).Visibility);

            Assert.IsType<Button>(section.FindName("DetectButton"));
            await section.RefreshDevicesAsync(force: true);
            Assert.Equal(1, source.RefreshCount);

            section.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
        });
    }

    [Fact]
    public void DetectRefreshesTheDeviceListAndPreservesTheSelectedDevice()
    {
        WpfTestHost.RunAsync(async () =>
        {
            var first = CreateDescriptor();
            var second = first with
            {
                Id = "gameinput:second",
                ProductName = "Second controller",
                VendorId = 0xFFFF,
                ProductId = 0x0002,
                SuggestedVisualModel = ControllerVisualModel.GenericGamepad,
                IsExactVisualModelMatch = false
            };
            var source = new RefreshingControllerSource(first, second);
            var section = new OptionsControllersSection(source);
            await section.RefreshDevicesAsync(force: false);
            var selector = Assert.IsType<ComboBox>(section.FindName("DeviceSelector"));
            Assert.Single(selector.Items);
            Assert.Same(first, selector.SelectedItem);

            Assert.IsType<Button>(section.FindName("DetectButton"));
            await section.RefreshDevicesAsync(force: true);

            Assert.Equal(2, selector.Items.Count);
            Assert.Same(first, selector.SelectedItem);
            Assert.Equal(1, source.RefreshCount);
            section.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
        });
    }

    [Fact]
    public void EveryRequiredVisualModelIsSelectableAndTranslated()
    {
        WpfTestHost.RunAsync(async () =>
        {
            var descriptor = CreateDescriptor() with
            {
                SuggestedVisualModel = ControllerVisualModel.GenericGamepad,
                IsExactVisualModelMatch = false
            };
            var section = new OptionsControllersSection(
                new FakeControllerSource(descriptor, GameInputLiveState.Empty(descriptor.Id)));
            await section.RefreshDevicesAsync(force: false);

            var selector = Assert.IsType<ComboBox>(section.FindName("ModelSelector"));
            var choices = selector.Items.Cast<object>().ToArray();
            Assert.Equal(Enum.GetValues<ControllerVisualModel>().Length + 1, choices.Length);
            var models = choices.Select(choice =>
                (ControllerVisualModel?)choice.GetType().GetProperty("Model")!.GetValue(choice)).ToArray();
            Assert.Null(models[0]);
            Assert.Equal(Enum.GetValues<ControllerVisualModel>().Order(),
                models.Skip(1).Select(model => Assert.IsType<ControllerVisualModel>(model)).Order());
            Assert.Equal(Visibility.Visible,
                Assert.IsType<Grid>(section.FindName("ModelSelectorPanel")).Visibility);
            Assert.All(choices, choice =>
            {
                var text = Assert.IsType<string>(
                    choice.GetType().GetProperty("DisplayName")!.GetValue(choice));
                Assert.False(string.IsNullOrWhiteSpace(text));
                Assert.DoesNotContain("Controllers.Model.", text, StringComparison.Ordinal);
            });

            section.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
        });
    }

    [Fact]
    public void CapabilitiesExposeRumbleAndForceFeedbackMotorCountsAndPower()
    {
        var descriptor = CreateDescriptor() with
        {
            RumbleMotors = GameInputRumbleMotors.LowFrequency |
                GameInputRumbleMotors.HighFrequency |
                GameInputRumbleMotors.LeftTrigger |
                GameInputRumbleMotors.RightTrigger,
            ForceFeedbackMotors =
            [
                new GameInputForceFeedbackMotorDescriptor(
                    0, GameInputFeedbackAxes.LinearX,
                    [GameInputForceFeedbackEffectKind.Constant], true),
                new GameInputForceFeedbackMotorDescriptor(
                    1, GameInputFeedbackAxes.AngularZ,
                    [GameInputForceFeedbackEffectKind.Spring], false)
            ]
        };

        var rows = GameInputDescriptorPresenter.Capabilities(descriptor);
        var rumble = Assert.Single(rows, row =>
            row.Label == GWGUI.App.Localization.Extensions.LocExtension.Get("Controllers.Rumble"));
        var feedback = Assert.Single(rows, row =>
            row.Label == GWGUI.App.Localization.Extensions.LocExtension.Get("Controllers.ForceFeedback"));

        Assert.StartsWith("4 · ", rumble.Value, StringComparison.Ordinal);
        Assert.StartsWith("2 · ", feedback.Value, StringComparison.Ordinal);
        Assert.Contains("#1: ⏻", feedback.Value, StringComparison.Ordinal);
        Assert.Contains("#2: ⏻", feedback.Value, StringComparison.Ordinal);
    }

    [Fact]
    public void RumbleTestActivatesOnlySupportedMotorsThenStopsEveryMotor()
    {
        var descriptor = CreateDescriptor() with
        {
            RumbleMotors = GameInputRumbleMotors.LowFrequency |
                GameInputRumbleMotors.RightTrigger
        };
        var source = new RecordingRumbleSource(descriptor);

        WpfTestHost.RunAsync(async () =>
        {
            var section = new OptionsControllersSection(source);
            await section.RefreshDevicesAsync(force: false);

            await section.TestRumbleAsync(GameInputRumbleMotors.RightTrigger);

            Assert.Collection(source.Calls,
                started =>
                {
                    Assert.Equal(descriptor.Id, started.DeviceId);
                    Assert.Equal((0f, 0f, 0f, .2f), started.Values);
                },
                stopped =>
                {
                    Assert.Equal(descriptor.Id, stopped.DeviceId);
                    Assert.Equal((0f, 0f, 0f, 0f), stopped.Values);
                });
            section.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
        });
    }

    [Fact]
    public void TechnicalDetailsPreserveControlsRumbleFeedbackHapticsReportsAndWheelCapabilities()
    {
        var location = Guid.Parse("d7010c42-25c4-4f03-a3d5-f87c5b0f7a21");
        var baseDescriptor = CreateDescriptor();
        var descriptor = baseDescriptor with
        {
            Status = GameInputDeviceStatus.Connected | GameInputDeviceStatus.HapticInfoReady,
            Controls =
            [
                new(GameInputControlType.Axis, 0, GameInputLabel.XboxLeftTrigger),
                new(GameInputControlType.Button, 0, GameInputLabel.XboxA),
                new(GameInputControlType.Switch, 0, GameInputLabel.None,
                    GameInputSwitchKind.EightWay,
                    [GameInputLabel.Up, GameInputLabel.ArrowUpRight, GameInputLabel.Right,
                     GameInputLabel.ArrowDownRight, GameInputLabel.Down, GameInputLabel.ArrowDownLeft,
                     GameInputLabel.Left, GameInputLabel.ArrowUpLeft])
            ],
            RumbleMotors = GameInputRumbleMotors.LowFrequency |
                GameInputRumbleMotors.HighFrequency |
                GameInputRumbleMotors.LeftTrigger |
                GameInputRumbleMotors.RightTrigger,
            StandardCapabilities = baseDescriptor.StandardCapabilities with
            {
                HasRacingWheel = true,
                RacingWheelHasClutch = true,
                RacingWheelHasHandbrake = true,
                RacingWheelHasPatternShifter = true,
                RacingWheelMaxAngle = 900
            },
            ForceFeedbackMotors =
            [
                new(0, GameInputFeedbackAxes.LinearX | GameInputFeedbackAxes.AngularZ,
                    [GameInputForceFeedbackEffectKind.Constant,
                     GameInputForceFeedbackEffectKind.Spring], true)
            ],
            InputReports = [new(GameInputRawDeviceReportKind.Input, 1, 64)],
            OutputReports = [new(GameInputRawDeviceReportKind.Output, 2, 32)],
            HasHaptics = true,
            HapticAudioEndpointId = "haptic-endpoint",
            HapticLocations = [location]
        };

        var rows = GameInputDescriptorPresenter.Capabilities(descriptor)
            .ToDictionary(row => row.Label, row => row.Value);

        Assert.Contains(LocExtension.Get("Controllers.ControlType.Axis"),
            rows[LocExtension.Get("Controllers.Controls")]);
        Assert.Contains(LocExtension.Get("Controllers.ControlType.Button"),
            rows[LocExtension.Get("Controllers.Controls")]);
        Assert.Contains("✣ · 8", rows[LocExtension.Get("Controllers.Controls")]);
        Assert.StartsWith("4 · ", rows[LocExtension.Get("Controllers.Rumble")]);
        Assert.StartsWith("1 · ", rows[LocExtension.Get("Controllers.ForceFeedback")]);
        Assert.Equal(LocExtension.Get("Controllers.Yes"), rows[LocExtension.Get("Controllers.Haptics")]);
        Assert.Contains(LocExtension.Get("Controllers.Enum.InputReport"),
            rows[LocExtension.Get("Controllers.RawReports")]);
        Assert.Contains(LocExtension.Get("Controllers.Enum.OutputReport"),
            rows[LocExtension.Get("Controllers.RawReports")]);
        Assert.Contains("900", rows[LocExtension.Get("Controllers.WheelCapabilities")]);
        Assert.Equal("haptic-endpoint", rows[LocExtension.Get("Controllers.HapticEndpoint")]);
        Assert.Contains(location.ToString(), rows[LocExtension.Get("Controllers.HapticLocations")]);
    }

    private static GameInputDeviceDescriptor CreateDescriptor() =>
        new(
            "gameinput:injected",
            "Injected Xbox Series",
            "Injected GameInput device",
            @"\\?\HID#INJECTED",
            0x045E,
            0x0B12,
            1,
            new GameInputVersion(),
            new GameInputVersion(),
            "root",
            Guid.Empty,
            GameInputDeviceFamily.XboxOne,
            new GameInputUsage { Page = 1, Id = 5 },
            GameInputKind.Controller | GameInputKind.Gamepad,
            GameInputRumbleMotors.LowFrequency | GameInputRumbleMotors.HighFrequency,
            GameInputSystemButtons.Guide,
            "Microsoft",
            [],
            [new GameInputControlDescriptor(GameInputControlType.Button, 0, GameInputLabel.XboxA)],
            new GameInputStandardCapabilities(
                GameInputGamepadButtons.A,
                0,
                0,
                false,
                false,
                true,
                false,
                false,
                false,
                false,
                0,
                new Dictionary<GameInputKind, IReadOnlyList<byte>>(),
                new Dictionary<GameInputKind, IReadOnlyList<byte>>()),
            [],
            [new GameInputRawReportDescriptor(GameInputRawDeviceReportKind.Input, 0, 18)],
            [],
            false,
            string.Empty,
            [],
            ControllerVisualModel.XboxSeries,
            true);

    private sealed class RefreshingControllerSource(
        GameInputDeviceDescriptor first,
        GameInputDeviceDescriptor second) : IGameInputControllerSource
    {
        private IReadOnlyList<GameInputDeviceDescriptor> _devices = [first];
        internal int RefreshCount { get; private set; }
        public IReadOnlyList<GameInputDeviceDescriptor> GetConnectedDevices() => _devices;
        public GameInputLiveState ReadState(string deviceId) => GameInputLiveState.Empty(deviceId);
        public void Refresh()
        {
            RefreshCount++;
            _devices = [first, second];
        }
        public bool SetRumble(string deviceId, float lowFrequency, float highFrequency,
            float leftTrigger, float rightTrigger) => false;
    }

    private sealed class FakeControllerSource(
        GameInputDeviceDescriptor descriptor,
        GameInputLiveState state) : IGameInputControllerSource
    {
        internal int RefreshCount { get; private set; }

        public IReadOnlyList<GameInputDeviceDescriptor> GetConnectedDevices() => [descriptor];
        public GameInputLiveState ReadState(string deviceId) => state;
        public void Refresh() => RefreshCount++;
        public bool SetRumble(
            string deviceId,
            float lowFrequency,
            float highFrequency,
            float leftTrigger,
            float rightTrigger) => true;
    }

    private sealed class RecordingRumbleSource(GameInputDeviceDescriptor descriptor)
        : IGameInputControllerSource
    {
        internal List<(string DeviceId, (float Low, float High, float Left, float Right) Values)> Calls { get; } = [];

        public IReadOnlyList<GameInputDeviceDescriptor> GetConnectedDevices() => [descriptor];
        public GameInputLiveState ReadState(string deviceId) => GameInputLiveState.Empty(deviceId);
        public void Refresh() { }
        public bool SetRumble(string deviceId, float lowFrequency, float highFrequency,
            float leftTrigger, float rightTrigger)
        {
            Calls.Add((deviceId, (lowFrequency, highFrequency, leftTrigger, rightTrigger)));
            return true;
        }
    }
}
