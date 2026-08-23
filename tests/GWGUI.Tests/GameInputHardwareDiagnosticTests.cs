using GWGUI.App.Services.Input.GameInput;
using GWGUI.App.Views.Controls.Options;
using System.Windows;
using System.Windows.Controls;
using Xunit.Abstractions;

namespace GWGUI.Tests;
[Collection("GameInput hardware")]
public sealed class GameInputHardwareDiagnosticTests(ITestOutputHelper output)
{
    [Fact]
    public void ListsEveryConnectedControllerInterface()
    {
        var devices = GameInputControllerReader.GetConnectedControllerDetails();
        foreach (var device in devices) output.WriteLine($"{device.Id} | {device.ProductName} | VID:PID={device.VidPid} | rumble={device.RumbleMotors} (0x{(uint)device.RumbleMotors:X8}) | status={device.Status}");
        output.WriteLine(GameInputControllerReader.LastEnumerationDiagnostic);
    }

    [Fact]
    [Trait("Category", "InteractiveHardware")]
    public void TestsOfficialXboxSeriesLowFrequencyRumbleWithoutDisconnecting()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("GWGUI_INTERACTIVE_HARDWARE"), "1", StringComparison.Ordinal))
        {
            output.WriteLine("Interactive hardware test not requested. Set GWGUI_INTERACTIVE_HARDWARE=1 to run it.");
            return;
        }

        var before = GameInputControllerReader.GetConnectedControllerDetails();
        var device = Assert.Single(before, item =>
            item.VendorId == 0x045E && item.ProductId == 0x0B12);
        output.WriteLine($"BEFORE id={device.Id} rumble={device.RumbleMotors} status={device.Status}");
        Assert.True(device.RumbleMotors.HasFlag(GameInputRumbleMotors.LowFrequency));

        var started = GameInputControllerReader.SetRumble(device.Id, 0.10f, 0f, 0f, 0f);
        output.WriteLine($"START low=0.10 high=0 leftTrigger=0 rightTrigger=0 result={started}");
        Assert.True(started);
        Thread.Sleep(80);
        var stopped = GameInputControllerReader.SetRumble(device.Id, 0f, 0f, 0f, 0f);
        output.WriteLine($"STOP result={stopped}");
        Assert.True(stopped);

        Thread.Sleep(1000);
        GameInputControllerReader.RefreshConnectedDevices();
        var after = GameInputControllerReader.GetConnectedControllerDetails();
        var reconnected = Assert.Single(after, item =>
            string.Equals(item.Id, device.Id, StringComparison.OrdinalIgnoreCase));
        output.WriteLine($"AFTER id={reconnected.Id} rumble={reconnected.RumbleMotors} status={reconnected.Status}");
        Assert.True(reconnected.Status.HasFlag(GameInputDeviceStatus.Connected));
    }

    [Fact]
    [Trait("Category", "InteractiveHardware")]
    public void TestsEachOfficialXboxSeriesRumbleMotorIndependently()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("GWGUI_INTERACTIVE_HARDWARE"), "1", StringComparison.Ordinal))
        {
            output.WriteLine("Interactive hardware test not requested. Set GWGUI_INTERACTIVE_HARDWARE=1 to run it.");
            return;
        }

        WpfTestHost.RunAsync(async () =>
        {
            var section = new OptionsControllersSection(GameInputControllerSource.Instance);
            await section.RefreshDevicesAsync(force: false);
            var selector = Assert.IsType<ComboBox>(section.FindName("DeviceSelector"));
            var device = Assert.Single(selector.Items.Cast<GameInputDeviceDescriptor>(), item =>
                item.VendorId == 0x045E && item.ProductId == 0x0B12);
            selector.SelectedItem = device;

            foreach (var motor in new[]
            {
                GameInputRumbleMotors.LowFrequency,
                GameInputRumbleMotors.HighFrequency,
                GameInputRumbleMotors.LeftTrigger,
                GameInputRumbleMotors.RightTrigger
            })
            {
                output.WriteLine($"START motor={motor} strength=20% duration=250ms");
                await section.TestRumbleAsync(motor);
                await Task.Delay(350);
                var connected = GameInputControllerReader.GetConnectedControllerDetails();
                var current = Assert.Single(connected, item =>
                    string.Equals(item.Id, device.Id, StringComparison.OrdinalIgnoreCase));
                output.WriteLine($"AFTER motor={motor} id={current.Id} status={current.Status}");
                Assert.True(current.Status.HasFlag(GameInputDeviceStatus.Connected));
            }
            section.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
        });
    }

    [Fact]
    public void DumpsRawGameInputDeviceCallbacks()
    {
        GameInputControllerReader.RefreshConnectedDevices();
        var devices = GameInputControllerReader.GetConnectedDevices();
        output.WriteLine($"Runtime={GameInputNative.SelectedRuntimePath}");
        output.WriteLine(GameInputControllerReader.DeviceCallbackTrace);
        output.WriteLine(GameInputControllerReader.RawEnumerationDiagnostic);
        Assert.NotEmpty(devices);
    }


    [Fact]
    public void ListsRawGameControllerFallbackDevices()
    {
        WpfTestHost.Run(() =>
        {
            RawGameControllerFallback.Refresh();
            var devices = RawGameControllerFallback.MergeDescriptors([]);
            foreach (var device in devices)
            {
                var state = Assert.IsType<GameInputLiveState>(
                    RawGameControllerFallback.TryReadDetailed(device.Id, out var live) ? live : null);
                output.WriteLine($"{device.Id} | {device.ProductName} | VID:PID={device.VidPid} | axes={device.Controls.Count(control => control.Type == GameInputControlType.Axis)} | buttons={device.Controls.Count(control => control.Type == GameInputControlType.Button)} | switches={device.Controls.Count(control => control.Type == GameInputControlType.Switch)} | reading={state.Timestamp}");
            }
            Assert.NotEmpty(devices);
        });
    }

    [Fact]
    public void ReadsEveryDetailedControllerStateFromWpfThread()
    {
        WpfTestHost.Run(() =>
        {
            GameInputControllerReader.StartMonitoring();
            RawGameControllerFallback.Refresh();
            var devices = GameInputControllerReader.GetConnectedControllerDetails();
            Assert.NotEmpty(devices);
            foreach (var device in devices)
            {
                var state = GameInputControllerReader.ReadDetailedState(device.Id);
                Assert.Equal(device.Id, state.DeviceId);
                output.WriteLine($"DEVICE {device.ProductName} | {device.VidPid} | {state.InputKind} | controls={state.Controls.Count} | raw={state.RawReport.Count}");
                if (state.Gamepad is { } gamepad)
                    output.WriteLine($"GAMEPAD buttons={gamepad.Buttons} | leftX={gamepad.LeftThumbstickX:0.000} | leftY={gamepad.LeftThumbstickY:0.000} | rightX={gamepad.RightThumbstickX:0.000} | rightY={gamepad.RightThumbstickY:0.000} | leftTrigger={gamepad.LeftTrigger:0.000} | rightTrigger={gamepad.RightTrigger:0.000}");
                foreach (var control in state.Controls)
                    output.WriteLine($"CONTROL type={control.Type} | index={control.Index} | label={control.Label} | value={control.Value:0.000} | switch={control.SwitchPosition}");
            }
            var assignmentStates = GameInputControllerReader.ReadAll();
            output.WriteLine($"ASSIGNMENT STATES: {string.Join(", ", assignmentStates.Select(state => state.DeviceId))}");
            Assert.Contains(assignmentStates, state => state.DeviceId.StartsWith("rawgamecontroller:", StringComparison.Ordinal));

        });
    }

    [Fact]
    public void RepeatedRefreshAndDetailedReadsRemainStable()
    {
        for (var iteration = 0; iteration < 10; iteration++)
        {
            GameInputControllerReader.RefreshConnectedDevices();
            var devices = GameInputControllerReader.GetConnectedControllerDetails();
            Assert.NotEmpty(devices);
            foreach (var device in devices)
            {
                var state = GameInputControllerReader.ReadDetailedState(device.Id);
                Assert.Equal(device.Id, state.DeviceId);
                var active = state.Controls.Where(control => control.IsPressed ||
                    (control.Type == GameInputControlType.Axis && Math.Abs(control.Value) > .001f)).ToArray();
                output.WriteLine($"REFRESH {iteration + 1} | {device.ProductName} | kind={state.InputKind} | gamepad={(state.Gamepad is null ? "null" : state.Gamepad.Value.Buttons.ToString())} | active={string.Join(",", active.Select(control => $"{control.Type}[{control.Index}]={control.Value:0.000}"))}");
            }
        }
    }

    [Fact]
    [Trait("Category", "InteractiveHardware")]
    public void DetectsARealConnectionOrDisconnectionAfterTheWatchStarts()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("GWGUI_INTERACTIVE_HARDWARE"), "1", StringComparison.Ordinal))
        {
            output.WriteLine("Interactive hardware test not requested. Set GWGUI_INTERACTIVE_HARDWARE=1 to run it.");
            return;
        }
        GameInputControllerReader.RefreshConnectedDevices();
        var baseline = GameInputControllerReader.GetConnectedControllerDetails()
            .ToDictionary(device => device.Id, StringComparer.OrdinalIgnoreCase);
        output.WriteLine($"WATCH START | {DateTime.Now:HH:mm:ss.fff} | {string.Join(", ", baseline.Values.Select(device => $"{device.ProductName} ({device.VidPid})"))}");

        var deadline = DateTime.UtcNow.AddSeconds(30);
        while (DateTime.UtcNow < deadline)
        {
            GameInputControllerReader.RefreshConnectedDevices();
            var current = GameInputControllerReader.GetConnectedControllerDetails()
                .ToDictionary(device => device.Id, StringComparer.OrdinalIgnoreCase);
            var connected = current.Values.FirstOrDefault(device => !baseline.ContainsKey(device.Id));
            if (connected is not null)
            {
                var state = GameInputControllerReader.ReadDetailedState(connected.Id);
                output.WriteLine($"CONNECTED | {DateTime.Now:HH:mm:ss.fff} | {connected.ProductName} | {connected.VidPid} | {state.InputKind} | controls={state.Controls.Count}");
                return;
            }

            var disconnected = baseline.Values.FirstOrDefault(device => !current.ContainsKey(device.Id));
            if (disconnected is not null)
            {
                output.WriteLine($"DISCONNECTED | {DateTime.Now:HH:mm:ss.fff} | {disconnected.ProductName} | {disconnected.VidPid}");
                return;
            }

            Thread.Sleep(100);
        }

        Assert.Fail($"No connection change received during 30 seconds. Devices: {string.Join(", ", baseline.Values.Select(device => $"{device.ProductName} ({device.VidPid})"))}");
    }

    [Fact]
    [Trait("Category", "InteractiveHardware")]
    public void ReceivesRealControllerActivity()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("GWGUI_INTERACTIVE_HARDWARE"), "1", StringComparison.Ordinal))
        {
            output.WriteLine("Interactive hardware test not requested. Set GWGUI_INTERACTIVE_HARDWARE=1 to run it.");
            return;
        }
        WpfTestHost.Run(() =>
        {
            var status = new TextBlock
            {
                FontSize = 24,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(32)
            };
            var window = new Window
            {
                Title = "Test GameInput en cours",
                Width = 680,
                Height = 240,
                Topmost = true,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = status
            };

            GameInputControllerReader.SetFocusPolicyForDiagnostics(GameInputFocusPolicy.EnableBackgroundInput);
            try
            {
                window.Show();
                window.Activate();
                var devices = GameInputControllerReader.GetConnectedControllerDetails();
                Assert.NotEmpty(devices);
                var baseline = devices.ToDictionary(
                    device => device.Id,
                    device => GameInputControllerReader.ReadDetailedState(device.Id));
                foreach (var device in devices)
                {
                    var state = baseline[device.Id];
                    output.WriteLine($"BASELINE {device.ProductName} | {device.VidPid} | kind={state.InputKind} | controls={state.Controls.Count} | raw={state.RawReport.Count}");
                }

                var deadline = DateTime.UtcNow.AddSeconds(15);
                while (DateTime.UtcNow < deadline)
                {
                    status.Text = $"Test GameInput actif — {Math.Ceiling((deadline - DateTime.UtcNow).TotalSeconds)} s\n\nAppuyez sur les boutons, les gâchettes et bougez les deux sticks.";
                    window.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Background);
                    foreach (var device in devices)
                    {
                        var original = baseline[device.Id];
                        var state = GameInputControllerReader.ReadDetailedState(device.Id);
                        var originalControls = original.Controls.ToDictionary(
                            control => (control.Type, control.Index), control => control.Value);
                        var changes = state.Controls
                            .Where(control => !originalControls.TryGetValue((control.Type, control.Index), out var oldValue) ||
                                Math.Abs(control.Value - oldValue) > .001f)
                            .ToArray();
                        if (changes.Length != 0)
                        {
                            output.WriteLine($"SIGNAL {device.ProductName} | {device.VidPid}: {string.Join(", ", changes.Select(control =>
                                $"{control.Type}[{control.Index}] {control.Label}={control.Value:0.000}"))}");
                            return;
                        }
                        if (state.SystemButtons != original.SystemButtons)
                        {
                            output.WriteLine($"SIGNAL {device.ProductName} | {device.VidPid}: SystemButtons={state.SystemButtons}");
                            return;
                        }
                        if (state.Gamepad is { } gamepad && original.Gamepad is { } originalGamepad &&
                            (gamepad.Buttons != originalGamepad.Buttons ||
                             Math.Abs(gamepad.LeftThumbstickX - originalGamepad.LeftThumbstickX) > .001f ||
                             Math.Abs(gamepad.LeftThumbstickY - originalGamepad.LeftThumbstickY) > .001f ||
                             Math.Abs(gamepad.RightThumbstickX - originalGamepad.RightThumbstickX) > .001f ||
                             Math.Abs(gamepad.RightThumbstickY - originalGamepad.RightThumbstickY) > .001f ||
                             Math.Abs(gamepad.LeftTrigger - originalGamepad.LeftTrigger) > .001f ||
                             Math.Abs(gamepad.RightTrigger - originalGamepad.RightTrigger) > .001f))
                        {
                            output.WriteLine($"SIGNAL {device.ProductName} | {device.VidPid}: " +
                                $"buttons={gamepad.Buttons}, " +
                                $"left=({gamepad.LeftThumbstickX:0.000},{gamepad.LeftThumbstickY:0.000}), " +
                                $"right=({gamepad.RightThumbstickX:0.000},{gamepad.RightThumbstickY:0.000}), " +
                                $"triggers=({gamepad.LeftTrigger:0.000},{gamepad.RightTrigger:0.000})");
                            return;
                        }
                        if (state.RawReport.Count != 0 && !state.RawReport.SequenceEqual(original.RawReport))
                        {
                            output.WriteLine($"SIGNAL {device.ProductName} | {device.VidPid}: RawReport={Convert.ToHexString(state.RawReport.ToArray())}");
                            return;
                        }
                    }
                    Thread.Sleep(20);
                }
                Assert.Fail($"No controller activity received during 15 seconds. Devices: {string.Join(", ", devices.Select(device => $"{device.ProductName} ({device.VidPid})"))}. Last detailed read: {GameInputControllerReader.LastDetailedReadDiagnostic}");
            }
            finally
            {
                GameInputControllerReader.SetFocusPolicyForDiagnostics(GameInputFocusPolicy.Default);
                window.Close();
            }
        });
    }

    [Fact]
    public void RealOptionsControllersSectionListsTheResolvedXboxSeriesName()
    {
        WpfTestHost.RunAsync(async () =>
        {
            var section = new OptionsControllersSection(GameInputControllerSource.Instance);
            var window = new Window { Content = section, Width = 1120, Height = 650 };
            try
            {
                window.Show();
                section.UpdateLayout();
                await section.RefreshDevicesAsync(force: false);
                var selector = Assert.IsType<ComboBox>(section.FindName("DeviceSelector"));
                var devices = Assert.IsAssignableFrom<IEnumerable<GameInputDeviceDescriptor>>(selector.ItemsSource).ToArray();
                output.WriteLine(string.Join(Environment.NewLine,
                    devices.Select(device => $"{device.ProductName} | {device.VidPid} | {device.Id}")));
                var xbox = devices.SingleOrDefault(device =>
                    device.VendorId == 0x045E && device.ProductId == 0x0B12);
                if (xbox is null)
                {
                    output.WriteLine("Xbox Series 045E:0B12 was not returned in this run; live name assertion was not executed.");
                    return;
                }
                Assert.Equal("Xbox Series X Controller", xbox.ProductName);
            }
            finally
            {
                window.Close();
            }
        });
    }

}
