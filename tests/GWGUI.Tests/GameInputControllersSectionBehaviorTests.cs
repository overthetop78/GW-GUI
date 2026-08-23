using GWGUI.App.Dictionaries.Localization;
using GWGUI.App.Constants.Localization;
using GWGUI.App.Functions.Localization;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Services.Input.GameInput;
using GWGUI.App.Views.Controls.Options;
using GWGUI.App.Views.Controls.Options.ControllerPresentation;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Localization.Sources;
using System.Globalization;

namespace GWGUI.Tests;

[Collection("GameInput hardware")]
public sealed class GameInputControllersSectionBehaviorTests
{
    [Fact]
    public void LiveRowsAreUpdatedInPlaceRawBytesStayHiddenAndAnalogValuesAreShown()
    {
        WpfTestHost.RunAsync(async () =>
        {
            var descriptor = Descriptor();
            var source = new MutableSource(descriptor, State(descriptor.Id, pressed: true));
            var section = new OptionsControllersSection(source);
            await section.RefreshDevicesAsync(force: false);
            typeof(OptionsControllersSection).GetMethod("RefreshLiveState", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(section, null);

            var grid = Assert.IsType<DataGrid>(section.FindName("ControlsGrid"));
            var firstRows = grid.ItemsSource.Cast<ControllerInputRow>().ToArray();
            Assert.Single(firstRows);
            Assert.True(firstRows[0].Active);
            Assert.DoesNotContain(firstRows, row => row.Key.Type == GameInputControlType.RawByte);
            Assert.Equal(6, Assert.IsType<ItemsControl>(section.FindName("AnalogValuesList")).Items.Count);
            Assert.Equal(GameInputDeviceModelCatalog.AllVisualModels.Count + 1,
                Assert.IsType<ComboBox>(section.FindName("ModelSelector")).Items.Count);
            var visualizer = Assert.IsType<ControllerVisualizer>(section.FindName("Visualizer"));
            var activeVisual = visualizer.GetSnapshotForTest();
            Assert.True(activeVisual.PrimaryPressed);
            Assert.Equal(.1f, activeVisual.LeftX, 3);
            Assert.Equal(-.2f, activeVisual.LeftY, 3);

            source.CurrentState = State(descriptor.Id, pressed: false);
            typeof(OptionsControllersSection).GetMethod("RefreshLiveState", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(section, null);
            var secondRows = grid.ItemsSource.Cast<ControllerInputRow>().ToArray();

            Assert.Same(firstRows[0], Assert.Single(secondRows));
            Assert.False(secondRows[0].Active);
            Assert.Equal("0", secondRows[0].Value);
            var releasedVisual = visualizer.GetSnapshotForTest();
            Assert.False(releasedVisual.PrimaryPressed);
            Assert.Equal(.1f, releasedVisual.LeftX, 3);
            Assert.Equal(-.2f, releasedVisual.LeftY, 3);
            section.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
        });
    }

    [Fact]
    public void CachedConnectionsAppearAndDisappearWithoutForcedDetection()
    {
        WpfTestHost.RunAsync(async () =>
        {
            var source = new HotPlugSource();
            var section = new OptionsControllersSection(source);
            await section.RefreshDevicesAsync(force: false);
            var selector = Assert.IsType<ComboBox>(section.FindName("DeviceSelector"));
            Assert.Empty(selector.Items);

            source.Devices = [Descriptor()];
            typeof(OptionsControllersSection).GetMethod(
                    "RefreshDevicesFromCache", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(section, null);
            Assert.Single(selector.Items);

            source.Devices = [];
            typeof(OptionsControllersSection).GetMethod(
                    "RefreshDevicesFromCache", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(section, null);
            Assert.Empty(selector.Items);
            Assert.Equal(0, source.RefreshCount);
            section.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
        });
    }

    [Fact]
    public void DetectionAndReadingFailuresAreContainedAndTranslated()
    {
        WpfTestHost.RunAsync(async () =>
        {
            var logged = new List<(Exception Exception, string Context)>();
            var section = new OptionsControllersSection(new FailingSource(),
                (exception, context) => logged.Add((exception, context)));
            var exception = await Record.ExceptionAsync(() =>
                section.RefreshDevicesAsync(force: false));

            Assert.Null(exception);
            Assert.Equal(LocExtension.Get("Controllers.DetectionFailed"),
                Assert.IsType<TextBlock>(section.FindName("DetectionStatus")).Text);
            var entry = Assert.Single(logged);
            Assert.IsType<COMException>(entry.Exception);
            Assert.Equal("Detecting GameInput controllers", entry.Context);
            section.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
        });
    }

    [Fact]
    public void EveryDisplayedEnumValueHasReadableText()
    {
        Assert.All(Enum.GetValues<GameInputLabel>().Where(label => label is not GameInputLabel.None and not GameInputLabel.Unknown),
            label =>
            {
                var text = GameInputDisplayFormatter.Label(label);
                Assert.False(string.IsNullOrWhiteSpace(text));
                Assert.False(text.StartsWith("#", StringComparison.Ordinal),
                    $"Missing readable mapping for {label} ({(int)label}).");
            });
        Assert.All(Enum.GetValues<GameInputSwitchPosition>(),
            value => Assert.False(string.IsNullOrWhiteSpace(GameInputDisplayFormatter.SwitchPosition(value))));
        Assert.All(Enum.GetValues<GameInputDeviceFamily>(),
            value => Assert.False(string.IsNullOrWhiteSpace(GameInputDisplayFormatter.Family(value))));
        var kinds = GameInputDisplayFormatter.Flags((GameInputKind)0x01040007);
        Assert.Contains(LocExtension.Get("Controllers.Enum.Gamepad"), kinds, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(LocExtension.Get("Controllers.Enum.UiNavigation"), kinds, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("17039367", kinds, StringComparison.Ordinal);
    }


    [Fact]
    public void EveryLanguageFormatsGameInputEnumsWithoutFallingBackToEnglish()
    {
        var previousCulture = LocalizationSource.Instance.Culture;
        var previousUiCulture = LocalizationSource.Instance.UiCulture;
        try
        {
            foreach (var language in UiLanguageCatalog.Available)
            {
                var culture = UiLanguageResolver.GetUiCulture(language.Code);
                LocalizationSource.Instance.SetCultures(culture, culture);
                var kinds = GameInputDisplayFormatter.Flags(
                    GameInputKind.RawDeviceReport | GameInputKind.Gamepad | GameInputKind.UiNavigation);
                Assert.Contains(LocExtension.Get("Controllers.RawReports"), kinds);
                Assert.Contains(LocExtension.Get("Controllers.Enum.Gamepad"), kinds);
                Assert.Contains(LocExtension.Get("Controllers.Enum.UiNavigation"), kinds);
                Assert.DoesNotContain("Raw Device Report", kinds, StringComparison.Ordinal);

                var motors = GameInputDisplayFormatter.Flags(
                    GameInputRumbleMotors.LowFrequency | GameInputRumbleMotors.HighFrequency);
                Assert.Contains(LocExtension.Get("Controllers.Enum.LowFrequencyMotor"), motors);
                Assert.Contains(LocExtension.Get("Controllers.Enum.HighFrequencyMotor"), motors);

                var status = GameInputDisplayFormatter.Flags(
                    GameInputDeviceStatus.Connected | GameInputDeviceStatus.HapticInfoReady);
                Assert.Contains(LocExtension.Get("Controllers.Device"), status);
                Assert.Contains(LocExtension.Get("Controllers.Haptics"), status);
                Assert.DoesNotContain("Connected", status, StringComparison.Ordinal);
            }
        }
        finally
        {
            LocalizationSource.Instance.SetCultures(previousCulture, previousUiCulture);
        }
    }

    [Fact]
    public void EveryDisplayedStandardEnumIsReadableInEveryLanguage()
    {
        var previousCulture = LocalizationSource.Instance.Culture;
        var previousUiCulture = LocalizationSource.Instance.UiCulture;
        try
        {
            foreach (var language in UiLanguageCatalog.Available)
            {
                var culture = UiLanguageResolver.GetUiCulture(language.Code);
                LocalizationSource.Instance.SetCultures(culture, culture);

                Assert.Equal(LocExtension.Get("Controllers.LeftStickX"),
                    GameInputDisplayFormatter.EnumValue(GameInputGamepadAxes.LeftThumbstickX));
                Assert.Equal(LocExtension.Get("Controllers.Steering"),
                    GameInputDisplayFormatter.EnumValue(GameInputRacingWheelAxes.Steering));
                Assert.Equal(LocExtension.Get("Controllers.Roll"),
                    GameInputDisplayFormatter.EnumValue(GameInputFlightStickAxes.Roll));
                Assert.Contains(LocExtension.Get("Controllers.Control"),
                    GameInputDisplayFormatter.EnumValue(GameInputArcadeStickButtons.Action1));
                Assert.Contains(LocExtension.Get("Controllers.Gear"),
                    GameInputDisplayFormatter.EnumValue(GameInputRacingWheelButtons.PreviousGear));

                var values = new List<string>();
                values.AddRange(Enum.GetValues<GameInputArcadeStickButtons>()
                    .Where(value => value != 0).Select(GameInputDisplayFormatter.EnumValue));
                values.AddRange(Enum.GetValues<GameInputFlightStickButtons>()
                    .Where(value => value != 0).Select(GameInputDisplayFormatter.EnumValue));
                values.AddRange(Enum.GetValues<GameInputGamepadButtons>()
                    .Where(value => value != 0).Select(GameInputDisplayFormatter.EnumValue));
                values.AddRange(Enum.GetValues<GameInputRacingWheelButtons>()
                    .Where(value => value != 0).Select(GameInputDisplayFormatter.EnumValue));
                values.AddRange(Enum.GetValues<GameInputFlightStickAxes>()
                    .Where(value => value != 0).Select(GameInputDisplayFormatter.EnumValue));
                values.AddRange(Enum.GetValues<GameInputGamepadAxes>()
                    .Where(value => value != 0).Select(GameInputDisplayFormatter.EnumValue));
                values.AddRange(Enum.GetValues<GameInputRacingWheelAxes>()
                    .Where(value => value != 0).Select(GameInputDisplayFormatter.EnumValue));
                values.AddRange(Enum.GetValues<GameInputRawDeviceReportKind>()
                    .Select(GameInputDisplayFormatter.EnumValue));
                values.AddRange(Enum.GetValues<GameInputFeedbackEffectState>()
                    .Select(GameInputDisplayFormatter.EnumValue));
                values.AddRange(Enum.GetValues<GameInputElementKind>()
                    .Where(value => value != GameInputElementKind.None)
                    .Select(GameInputDisplayFormatter.EnumValue));
                values.AddRange(Enum.GetValues<GameInputSwitchKind>()
                    .Select(GameInputDisplayFormatter.SwitchKind));

                Assert.All(values, value =>
                {
                    Assert.False(string.IsNullOrWhiteSpace(value));
                    Assert.DoesNotContain("Controllers.", value, StringComparison.Ordinal);
                    Assert.False(value.StartsWith("#", StringComparison.Ordinal));
                });
            }
        }
        finally
        {
            LocalizationSource.Instance.SetCultures(previousCulture, previousUiCulture);
        }
    }

    [Fact]
    public void LanguageChangeReplacesEveryControllersSectionTextImmediately()
    {
        WpfTestHost.Run(() =>
        {
            var previousCulture = LocalizationSource.Instance.Culture;
            var previousUiCulture = LocalizationSource.Instance.UiCulture;
            try
            {
                LocalizationSource.Instance.SetCultures(
                    CultureInfo.GetCultureInfo("it-IT"), CultureInfo.GetCultureInfo("it-IT"));
                var descriptor = Descriptor();
                var section = new OptionsControllersSection(new MutableSource(descriptor, State(descriptor.Id, true)));
                section.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
                Assert.Equal("Rileva controller", Assert.IsType<Button>(section.FindName("DetectButton")).Content);
                Assert.Equal("Valori analogici", LocExtension.Get("Controllers.AnalogValues"));

                LocalizationSource.Instance.SetCultures(
                    CultureInfo.GetCultureInfo("ja-JP"), CultureInfo.GetCultureInfo("ja-JP"));
                section.RefreshLocalizedContent();

                Assert.Equal("コントローラーを検出", Assert.IsType<Button>(section.FindName("DetectButton")).Content);
                Assert.Equal("アナログ値", LocExtension.Get("Controllers.AnalogValues"));
                Assert.DoesNotContain("Rileva controller", VisibleText(section));
                Assert.DoesNotContain("Valori analogici", VisibleText(section));
                section.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
            }
            finally
            {
                LocalizationSource.Instance.SetCultures(previousCulture, previousUiCulture);
            }
        });
    }

    [Fact]
    public void XboxWirelessReceiverKeepsAutoAndManualModelSelectorAvailable()
    {
        WpfTestHost.RunAsync(async () =>
        {
            var descriptor = Descriptor() with { ProductName = "Xbox Series X Controller" };
            var section = new OptionsControllersSection(
                new MutableSource(descriptor, State(descriptor.Id, pressed: false)));
            var window = new Window
            {
                Content = section,
                Width = 1500,
                Height = 820,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false
            };
            try
            {
                window.Show();
                await section.RefreshDevicesAsync(force: false);
                section.UpdateLayout();
                Assert.Equal(Visibility.Visible,
                    Assert.IsType<Grid>(section.FindName("ModelSelectorPanel")).Visibility);
                Assert.Equal(0, Assert.IsType<ComboBox>(section.FindName("ModelSelector")).SelectedIndex);
                var bitmap = new RenderTargetBitmap(1500, 820, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(window);
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
                    "..", "..", "..", "..", "..", "tmp", "captures",
                    "options-controllers-xbox-series-reference-20260823.png"));
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                using var stream = File.Create(path);
                encoder.Save(stream);
                Assert.True(new FileInfo(path).Length > 10_000);
            }
            finally
            {
                window.Close();
                section.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
            }
        });
    }

    private static IReadOnlyList<string> VisibleText(DependencyObject root)
    {
        var values = new List<string>();
        for (var index = 0; index < System.Windows.Media.VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(root, index);
            if (child is TextBlock text && !string.IsNullOrWhiteSpace(text.Text)) values.Add(text.Text);
            if (child is ContentControl { Content: string content } && !string.IsNullOrWhiteSpace(content)) values.Add(content);
            values.AddRange(VisibleText(child));
        }
        return values;
    }

    private static GameInputLiveState State(string id, bool pressed) => new(
        id, 10, GameInputKind.Gamepad,
        [
            new GameInputControlValue(GameInputControlType.Button, 0, GameInputLabel.XboxA, pressed ? 1f : 0f),
            new GameInputControlValue(GameInputControlType.RawByte, 0, GameInputLabel.None, 255f)
        ], [255], GameInputSystemButtons.None, null, null,
        new GameInputGamepadState
        {
            Buttons = pressed ? GameInputGamepadButtons.A : GameInputGamepadButtons.None,
            LeftThumbstickX = .1f, LeftThumbstickY = .2f,
            RightThumbstickX = .3f, RightThumbstickY = .4f,
            LeftTrigger = .5f, RightTrigger = .6f
        }, null);

    private static GameInputDeviceDescriptor Descriptor() => new(
        "gameinput:behavior", "Test controller", "GameInput device", @"\?HID#TEST",
        0x045E, 0x0B12, 1, new(), new(), "root", Guid.Empty,
        GameInputDeviceFamily.XboxOne, new GameInputUsage { Page = 1, Id = 5 },
        GameInputKind.Controller | GameInputKind.Gamepad,
        GameInputRumbleMotors.LowFrequency | GameInputRumbleMotors.HighFrequency,
        GameInputSystemButtons.Guide, "Microsoft", [],
        [new(GameInputControlType.Button, 0, GameInputLabel.XboxA)],
        new(GameInputGamepadButtons.A, 0, 0, false, false, true, false,
            false, false, false, 0,
            new Dictionary<GameInputKind, IReadOnlyList<byte>>(),
            new Dictionary<GameInputKind, IReadOnlyList<byte>>()),
        [], [new(GameInputRawDeviceReportKind.Input, 0, 18)], [], false, string.Empty, [],
        ControllerVisualModel.XboxSeries, false);

    private sealed class HotPlugSource : IGameInputControllerSource
    {
        internal IReadOnlyList<GameInputDeviceDescriptor> Devices { get; set; } = [];
        internal int RefreshCount { get; private set; }
        public IReadOnlyList<GameInputDeviceDescriptor> GetConnectedDevices() => Devices;
        public GameInputLiveState ReadState(string deviceId) => GameInputLiveState.Empty(deviceId);
        public void Refresh() => RefreshCount++;
        public bool SetRumble(string deviceId, float lowFrequency, float highFrequency,
            float leftTrigger, float rightTrigger) => false;
    }

    private sealed class MutableSource(GameInputDeviceDescriptor descriptor, GameInputLiveState state)
        : IGameInputControllerSource
    {
        internal GameInputLiveState CurrentState { get; set; } = state;
        public IReadOnlyList<GameInputDeviceDescriptor> GetConnectedDevices() => [descriptor];
        public GameInputLiveState ReadState(string deviceId) => CurrentState;
        public void Refresh() { }
        public bool SetRumble(string deviceId, float lowFrequency, float highFrequency,
            float leftTrigger, float rightTrigger) => true;
    }

    private sealed class FailingSource : IGameInputControllerSource
    {
        public IReadOnlyList<GameInputDeviceDescriptor> GetConnectedDevices() => throw new COMException("failure", unchecked((int)0x80004002));
        public GameInputLiveState ReadState(string deviceId) => throw new COMException("failure", unchecked((int)0x80004002));
        public void Refresh() => throw new COMException("failure", unchecked((int)0x80004002));
        public bool SetRumble(string deviceId, float lowFrequency, float highFrequency,
            float leftTrigger, float rightTrigger) => throw new COMException("failure", unchecked((int)0x80004002));
    }
}
