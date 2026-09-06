using GWGUI.App.Localization.Extensions;
using GWGUI.App.Services.Input.GameInput;
namespace GWGUI.Tests.Interface.DeviceSettingsViews;
internal static class DeviceFeedbackScenarios
{
    public static async Task Pulse(int bit)
    {
        var source = new DeviceListScenarios.Source(); var pulse = new TaskCompletionSource();
        var view = DeviceListScenarios.View(source,delay: milliseconds => { Assert.Equal(250,milliseconds); return pulse.Task; });
        await view.RefreshDevicesAsync(false); view.DeviceSelector.SelectedIndex = 1; view.RumbleStrengthSlider.Value = 37;
        var running = view.TestRumbleAsync((GameInputRumbleMotors)(1 << bit));
        Assert.False(running.IsCompleted); Assert.False(view.LowFrequencyRumbleButton.IsEnabled); Assert.False(view.RumbleStrengthSlider.IsEnabled);
        Assert.Equal(LocExtension.Get("Controllers.TestRunning"),view.TestStatusText.Text);
        var call = Assert.Single(source.Rumbles); Assert.Equal("two",call.Id);
        var expected = new float[4]; expected[bit] = .37f;
        Assert.Equal(expected,new[] {call.Low,call.High,call.Left,call.Right});
        pulse.SetResult(); await running;
        Assert.Equal(("two",0f,0f,0f,0f),source.Rumbles[1]); Assert.True(view.LowFrequencyRumbleButton.IsEnabled);
        Assert.Equal(LocExtension.Get("Controllers.TestCompleted"),view.TestStatusText.Text);
    }
    public static async Task Failure(bool throws)
    {
        var source = new DeviceListScenarios.Source(); var errors = new List<Exception>();
        source.RumbleResult = () => throws ? throw new IOException("synthetic rumble") : false;
        var view = DeviceListScenarios.View(source,errors:errors); await view.RefreshDevicesAsync(false);
        await view.TestRumbleAsync(GameInputRumbleMotors.LowFrequency);
        Assert.Single(source.Rumbles); Assert.Equal(throws?1:0,errors.Count); Assert.True(view.LowFrequencyRumbleButton.IsEnabled);
        Assert.Equal(LocExtension.Get("Controllers.TestFailed"),view.TestStatusText.Text);
        await view.TestRumbleAsync(GameInputRumbleMotors.None);
        await view.TestRumbleAsync(GameInputRumbleMotors.LowFrequency | GameInputRumbleMotors.HighFrequency);
        source.Devices = []; await view.RefreshDevicesAsync(false); await view.TestRumbleAsync(GameInputRumbleMotors.LowFrequency);
        Assert.Single(source.Rumbles);
    }
}
