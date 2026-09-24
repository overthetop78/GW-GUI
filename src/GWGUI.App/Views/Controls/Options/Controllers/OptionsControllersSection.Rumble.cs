using GWGUI.App.Localization.Extensions;
using GWGUI.App.Services.Input.GameInput;
using GWGUI.App.Services.Logging;
using GWGUI.App.Views.Controls.Options.ControllerPresentation;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace GWGUI.App.Views.Controls.Options;

public partial class OptionsControllersSection
{
    private async void RumbleMotor_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string value } ||
            !Enum.TryParse<GameInputRumbleMotors>(value, out var motor)) return;
        await TestRumbleAsync(motor);
    }

    private void RumbleStrengthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (RumbleStrengthText is not null)
            RumbleStrengthText.Text = $"{Math.Round(e.NewValue):0} %";
    }

    internal async Task TestRumbleAsync(GameInputRumbleMotors motor)
    {
        const int pulseDurationMilliseconds = 250;

        var device = _selectedDevice;
        var motorBits = (uint)motor;
        if (device is null || motorBits == 0 || (motorBits & (motorBits - 1)) != 0 ||
            !device.RumbleMotors.HasFlag(motor)) return;

        var intensity = (float)(RumbleStrengthSlider.Value / 100d);
        var values = motor switch
        {
            GameInputRumbleMotors.LowFrequency => (intensity, 0f, 0f, 0f),
            GameInputRumbleMotors.HighFrequency => (0f, intensity, 0f, 0f),
            GameInputRumbleMotors.LeftTrigger => (0f, 0f, intensity, 0f),
            GameInputRumbleMotors.RightTrigger => (0f, 0f, 0f, intensity),
            _ => (0f, 0f, 0f, 0f)
        };

        _testingRumble = true;
        UpdateRumbleControls();
        TestStatusText.Text = LocExtension.Get("Controllers.TestRunning");
        var started = false;
        try
        {
            started = _source.SetRumble(device.Id, values.Item1, values.Item2, values.Item3, values.Item4);
            if (!started)
            {
                TestStatusText.Text = LocExtension.Get("Controllers.TestFailed");
                return;
            }
            await _delay(pulseDurationMilliseconds);
            TestStatusText.Text = LocExtension.Get("Controllers.TestCompleted");
        }
        catch (Exception exception)
        {
            LogOnce(exception, "Testing GameInput rumble");
            TestStatusText.Text = LocExtension.Get("Controllers.TestFailed");
        }
        finally
        {
            if (started)
            {
                try { _source.SetRumble(device.Id, 0f, 0f, 0f, 0f); }
                catch (Exception exception) { LogOnce(exception, "Stopping GameInput rumble"); }
            }
            _testingRumble = false;
            UpdateRumbleControls();
        }
    }

    private void StopRumble()
    {
        var device = _selectedDevice;
        if (device is null || device.RumbleMotors == GameInputRumbleMotors.None) return;
        try { _source.SetRumble(device.Id, 0f, 0f, 0f, 0f); }
        catch (Exception exception) { LogOnce(exception, "Stopping GameInput rumble"); }
    }

    private void UpdateRumbleControls()
    {
        var motors = _selectedDevice?.RumbleMotors ?? GameInputRumbleMotors.None;
        RumblePanel.Visibility = motors == GameInputRumbleMotors.None
            ? Visibility.Collapsed
            : Visibility.Visible;
        var enabled = !_testingRumble;
        LowFrequencyRumbleButton.IsEnabled = enabled && motors.HasFlag(GameInputRumbleMotors.LowFrequency);
        HighFrequencyRumbleButton.IsEnabled = enabled && motors.HasFlag(GameInputRumbleMotors.HighFrequency);
        LeftTriggerRumbleButton.IsEnabled = enabled && motors.HasFlag(GameInputRumbleMotors.LeftTrigger);
        RightTriggerRumbleButton.IsEnabled = enabled && motors.HasFlag(GameInputRumbleMotors.RightTrigger);
        RumbleStrengthSlider.IsEnabled = enabled && motors != GameInputRumbleMotors.None;
    }

    private void LogOnce(Exception exception, string context)
    {
        var signature = $"{context}|{exception.GetType().FullName}|{exception.HResult:X8}";
        if (string.Equals(signature, _lastLoggedFailure, StringComparison.Ordinal)) return;
        _lastLoggedFailure = signature;
        _errorLogger(exception, context);
    }

}
