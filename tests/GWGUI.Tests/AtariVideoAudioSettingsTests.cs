using System.Windows;
using System.Windows.Threading;
using GWGUI.App.Controls;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;

namespace GWGUI.Tests;

[Collection(AtariNativeCoreTestConstants.CollectionName)]
public sealed class AtariVideoAudioSettingsTests
{
    public static TheoryData<AtariMachineModel> EveryModel => new(Enum.GetValues<AtariMachineModel>());

    [Theory]
    [MemberData(nameof(EveryModel))]
    public void EveryModelBuildsVideoAndAudioChoicesFromItsCatalog(AtariMachineModel model)
    {
        var configuration = new AtariMachineConfiguration(model);
        var view = AtariVideoAudioSettingsFunctions.Create(configuration);

        Assert.NotEmpty(view.Standards);
        Assert.NotEmpty(view.Regions);
        Assert.NotEmpty(view.Resolutions);
        Assert.NotEmpty(view.AspectRatios);
        Assert.NotEmpty(view.Cropping);
        Assert.NotEmpty(view.FrameSkips);
        Assert.NotEmpty(view.Renderers);
        Assert.NotEmpty(view.Outputs);
        Assert.NotEmpty(view.Latencies);
        Assert.NotEmpty(view.Volumes);
        Assert.NotEmpty(view.Qualities);
        Assert.Equal(configuration.VideoRenderer, view.Renderer);
        Assert.Equal(Enum.GetValues<EmulationVideoRenderer>().Order(),
            view.Renderers.Select(value => Enum.Parse<EmulationVideoRenderer>(value.Value)).Order());
    }

    [Fact]
    public void ApplyPersistsFrontendValuesAndPreservesUnknownOptions()
    {
        var source = new AtariMachineConfiguration(AtariMachineModel.St,
            options: new Dictionary<string, string>
            {
                [AtariVideoAudioSettingsTestConstants.UnknownKey] = AtariVideoAudioSettingsTestConstants.UnknownValue
            });
        var displayed = new Dictionary<string, string>
        {
            [AtariVideoAudioSettingsConstants.FrameSkipOptionKey] = AtariVideoAudioSettingsTestConstants.FrameSkip
        };

        var result = AtariVideoAudioSettingsFunctions.Apply(source, displayed, false, EmulationVideoRenderer.Wpf);

        Assert.False(result.AudioEnabled);
        Assert.Equal(EmulationVideoRenderer.Wpf, result.VideoRenderer);
        Assert.Equal(AtariVideoAudioSettingsTestConstants.FrameSkip,
            result.Options[AtariVideoAudioSettingsConstants.FrameSkipOptionKey]);
        Assert.Equal(AtariVideoAudioSettingsTestConstants.UnknownValue,
            result.Options[AtariVideoAudioSettingsTestConstants.UnknownKey]);
    }

    [Fact]
    public void InvalidPersistedValueFallsBackWithoutDeletingStoredValue()
    {
        var choices = new[] { new AtariVideoAudioChoice(AtariVideoAudioSettingsTestConstants.ValidValue,
            AtariVideoAudioSettingsTestConstants.ValidValue) };
        var options = new Dictionary<string, string>
        {
            [AtariVideoAudioSettingsTestConstants.OptionKey] = AtariVideoAudioSettingsTestConstants.UnknownValue
        };

        var selected = AtariVideoAudioSettingsFunctions.Select(options,
            AtariVideoAudioSettingsTestConstants.OptionKey, choices, AtariVideoAudioSettingsTestConstants.ValidValue);

        Assert.Equal(AtariVideoAudioSettingsTestConstants.ValidValue, selected);
        Assert.Equal(AtariVideoAudioSettingsTestConstants.UnknownValue,
            options[AtariVideoAudioSettingsTestConstants.OptionKey]);
    }

    [Fact]
    public void ControlsCanLoadTheSameConfigurationRepeatedly()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                var app = Application.Current as GWGUI.App.App ?? new GWGUI.App.App();
                app.InitializeComponent();
                var section = new AtariVideoAudioSettingsSection();
                var configuration = new AtariMachineConfiguration(AtariMachineModel.St);
                section.Load(configuration);
                section.Load(configuration);
            }
            catch (Exception error) { failure = error; }
            finally { Dispatcher.CurrentDispatcher.InvokeShutdown(); }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(10000));
        if (failure is not null) throw failure;
    }
}

internal static class AtariVideoAudioSettingsTestConstants
{
    internal const string UnknownKey = "future_video_option";
    internal const string UnknownValue = "preserved";
    internal const string FrameSkip = "2";
    internal const string OptionKey = "option";
    internal const string ValidValue = "valid";
}
