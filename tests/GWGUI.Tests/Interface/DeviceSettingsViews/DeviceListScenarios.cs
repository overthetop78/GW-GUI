using GWGUI.App.Localization.Extensions;
using GWGUI.App.Services.Input.GameInput;
using GWGUI.App.Views.Controls.Options;
using System.Windows;
namespace GWGUI.Tests.Interface.DeviceSettingsViews;

internal static class DeviceListScenarios
{
    internal sealed class Source : IGameInputControllerSource
    {
        public IReadOnlyList<GameInputDeviceDescriptor> Devices = [Device("one"), Device("two")];
        public GameInputLiveState State = GameInputLiveState.Empty("one");
        public Action RefreshAction = () => { };
        public Exception? ReadError;
        public int Started;
        public List<string> Reads = [];
        public List<(string Id, float Low, float High, float Left, float Right)> Rumbles = [];
        public Func<bool> RumbleResult = () => true;
        public void StartMonitoring() => Started++;
        public IReadOnlyList<GameInputDeviceDescriptor> GetConnectedDevices() => Devices;
        public void Refresh() => RefreshAction();
        public GameInputLiveState ReadState(string id) { Reads.Add(id); if (ReadError is { } error) throw error; return State with { DeviceId = id }; }
        public bool SetRumble(string id, float low, float high, float left, float right)
        { Rumbles.Add((id,low,high,left,right)); return RumbleResult(); }
    }
    internal sealed class Profiles : IControllerProfileStore
    {
        public Dictionary<string, ControllerVisualProfile> Visual = [];
        public Dictionary<string, ControllerAnalogDeadZoneProfile> Analog = [];
        public List<string> Saved = [];
        public IReadOnlyDictionary<string, ControllerVisualModel> GetModels() => Visual.ToDictionary(x => x.Key,x => x.Value.Model);
        public ControllerVisualProfile? GetVisual(string id) => Visual.GetValueOrDefault(id);
        public void SetVisual(string id, ControllerVisualModel model, string name) { Visual[id] = new(model,name); Saved.Add(id); }
        public void RemoveVisual(string id) { Visual.Remove(id); Saved.Add(id); }
        public ControllerAnalogDeadZoneProfile GetAnalog(string id) => Analog.GetValueOrDefault(id,ControllerAnalogDeadZoneProfile.Default);
        public void PreviewAnalog(string id, ControllerAnalogDeadZoneProfile profile) => Analog[id] = profile;
        public void SaveAnalog(string id, ControllerAnalogDeadZoneProfile profile) { Analog[id] = profile; Saved.Add(id); }
    }
    internal static GameInputDeviceDescriptor Device(string id) => new(id,"device " + id,"synthetic","virtual-pnp",0xfffe,1,0,default,default,"root",Guid.Empty,default,default,GameInputKind.Gamepad,
        GameInputRumbleMotors.LowFrequency | GameInputRumbleMotors.HighFrequency | GameInputRumbleMotors.LeftTrigger | GameInputRumbleMotors.RightTrigger,
        default,"synthetic",[],[],new(default,0,0,false,false,true,false,false,false,false,0,new Dictionary<GameInputKind,IReadOnlyList<byte>>(),new Dictionary<GameInputKind,IReadOnlyList<byte>>()),[],[],[],false,"",[],ControllerVisualModel.GenericGamepad,false);
    internal static OptionsControllersSection View(Source source, Profiles? profiles = null, List<Exception>? errors = null, Func<int,Task>? delay = null) =>
        new(source,(error,_) => { if(errors is null) Assert.Fail(error.ToString()); else errors.Add(error); },profiles ?? new Profiles(),delay ?? (_ => Task.CompletedTask));

    public static async Task Selection()
    {
        var source = new Source(); var profiles = new Profiles(); profiles.Visual["two"] = new(ControllerVisualModel.XboxOne,"custom two");
        var view = View(source,profiles); Assert.Equal(1,source.Started);
        await view.RefreshDevicesAsync(false);
        Assert.Equal(2,view.DeviceSelector.Items.Count); Assert.Equal("device one",view.ProductNameText.Text);
        Assert.Equal(Visibility.Visible,view.ModelSelectorPanel.Visibility);
        view.DeviceSelector.SelectedIndex = 1; Assert.Equal("custom two",view.ProductNameText.Text);
        source.Devices = [Device("two"),Device("three") with { IsExactVisualModelMatch = true }];
        await view.RefreshDevicesAsync(false);
        Assert.Equal("two",Assert.IsType<GameInputDeviceDescriptor>(view.DeviceSelector.SelectedItem).Id);
        source.Devices = [source.Devices[1]]; await view.RefreshDevicesAsync(false);
        Assert.Equal("three",Assert.IsType<GameInputDeviceDescriptor>(view.DeviceSelector.SelectedItem).Id);
        Assert.Equal(Visibility.Collapsed,view.ModelSelectorPanel.Visibility);
        source.Devices = []; await view.RefreshDevicesAsync(false);
        Assert.Empty(view.DeviceSelector.Items); Assert.Null(view.DeviceSelector.SelectedItem);
        Assert.Equal(LocExtension.Get("Controllers.NoneDetected"),view.ProductNameText.Text);
        Assert.False(view.LowFrequencyRumbleButton.IsEnabled); Assert.Equal(Visibility.Collapsed,view.RumblePanel.Visibility);
        Assert.Empty(profiles.Saved);
    }
    public static async Task Failure()
    {
        var source = new Source(); var errors = new List<Exception>(); var view = View(source,errors:errors);
        await view.RefreshDevicesAsync(false); source.RefreshAction = () => throw new IOException("synthetic detection");
        await view.RefreshDevicesAsync(true);
        Assert.Empty(view.DeviceSelector.Items); Assert.Single(errors); Assert.True(view.DetectButton.IsEnabled);
        Assert.Equal(LocExtension.Get("Controllers.DetectionFailed"),view.DetectionStatus.Text);
        source.RefreshAction = () => { }; await view.RefreshDevicesAsync(true);
        Assert.Equal(2,view.DeviceSelector.Items.Count); Assert.Equal(LocExtension.Get("Controllers.DetectedCount",2),view.DetectionStatus.Text);
    }
}
