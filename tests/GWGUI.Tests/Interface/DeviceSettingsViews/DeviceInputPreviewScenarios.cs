using GWGUI.App.Localization.Extensions;
using GWGUI.App.Services.Input.GameInput;
using GWGUI.App.Views.Controls.Options.ControllerPresentation;
namespace GWGUI.Tests.Interface.DeviceSettingsViews;
internal static class DeviceInputPreviewScenarios
{
    public static async Task Values()
    {
        var source = new DeviceListScenarios.Source(); var profiles = new DeviceListScenarios.Profiles();
        profiles.Analog["two"] = new(17,8,4);
        var view = DeviceListScenarios.View(source,profiles); await view.RefreshDevicesAsync(false);
        source.State = source.State with { Controls = [new(GameInputControlType.Axis,0,default,.25f),new(GameInputControlType.Button,1,default,1),new(GameInputControlType.RawByte,0,default,42)] };
        await view.RefreshLiveStateAsync();
        Assert.Same(source.State.Controls,view.Visualizer.State!.Controls);
        var rows = view.ControlsGrid.Items.Cast<ControllerInputRow>().ToArray(); Assert.Equal(2,rows.Length);
        Assert.True(rows.Single(x => x.Key.Type == GameInputControlType.Button).Active);
        source.State = source.State with { Controls = [new(GameInputControlType.Button,1,default,0)] };
        await view.RefreshLiveStateAsync(); Assert.Single(view.ControlsGrid.Items);
        Assert.False(Assert.IsType<ControllerInputRow>(view.ControlsGrid.Items[0]).Active);
        view.DeviceSelector.SelectedIndex = 1; Assert.Empty(view.ControlsGrid.Items);
        Assert.Equal(17,view.StickDeadZoneSlider.Value); Assert.Equal(8,view.TriggerDeadZoneSlider.Value);
        await view.RefreshLiveStateAsync(); Assert.Equal("two",source.Reads[^1]); Assert.Equal("two",view.Visualizer.State!.DeviceId);
        view.StickDeadZoneSlider.Value = 23; Assert.Equal(23,profiles.Analog["two"].StickPercent);
        Assert.False(profiles.Analog.ContainsKey("one"));
        view.DeviceSelector.SelectedIndex = 0; Assert.Equal(0,view.StickDeadZoneSlider.Value);
        view.DeviceSelector.SelectedIndex = 1; Assert.Equal(23,view.StickDeadZoneSlider.Value);
    }
    public static async Task Failure()
    {
        var source = new DeviceListScenarios.Source(); var errors = new List<Exception>();
        var view = DeviceListScenarios.View(source,errors:errors); await view.RefreshDevicesAsync(false);
        source.ReadError = new IOException("synthetic state"); await view.RefreshLiveStateAsync(); await view.RefreshLiveStateAsync();
        Assert.Single(errors); Assert.Equal(LocExtension.Get("Controllers.ReadFailed"),view.DetectionStatus.Text);
        source.ReadError = null; await view.RefreshLiveStateAsync();
        Assert.Equal(LocExtension.Get("Controllers.DetectedCount",2),view.DetectionStatus.Text);
    }
}
