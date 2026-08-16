using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Localization;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;

namespace GWGUI.App.Controls;

internal sealed class AtariVideoAudioSettingsSection
{
    private readonly Dictionary<string, ComboBox> _options = new(StringComparer.Ordinal);
    private readonly ComboBox _renderer = new();
    private readonly CheckBox _audioEnabled = new();
    private readonly StackPanel _video = new();
    private readonly StackPanel _audioOutput = new();
    private readonly StackPanel _audioQuality = new();
    internal UIElement Video { get; }
    internal UIElement Audio { get; }

    internal AtariVideoAudioSettingsSection()
    {
        var rendering = new StackPanel();
        rendering.Children.Add(AtariAccessibilityFunctions.LabeledRow(
            LocExtension.Get("Emulation.RenderingSettings"), _renderer));
        Video = EmulationSettingsLayout.ScrollPage(EmulationSettingsLayout.TwoColumnPage(
            EmulationSettingsLayout.ActionCard(_video, LocExtension.Get("Emulation.DisplaySettings")),
            EmulationSettingsLayout.ActionCard(rendering, LocExtension.Get("Emulation.RenderingSettings"))));
        Audio = EmulationSettingsLayout.ScrollPage(EmulationSettingsLayout.TwoColumnPage(
            EmulationSettingsLayout.ActionCard(_audioOutput, LocExtension.Get("Emulation.AudioOutput")),
            EmulationSettingsLayout.ActionCard(_audioQuality, LocExtension.Get("Emulation.AudioQuality"))));
    }

    internal void Load(AtariMachineConfiguration configuration)
    {
        var view = AtariVideoAudioSettingsFunctions.Create(configuration);
        _options.Clear();
        _video.Children.Clear();
        _audioOutput.Children.Clear();
        _audioQuality.Children.Clear();
        Add(_video, AtariVideoAudioSettingsConstants.VideoStandardResource,
            AtariVideoAudioSettingsConstants.StandardOptionKey, view.Standards, configuration.Options,
            view.Standards.First().Value);
        Add(_video, AtariVideoAudioSettingsConstants.RegionResource,
            AtariVideoAudioSettingsConstants.RegionOptionKey, view.Regions, configuration.Options,
            AtariVideoAudioSettingsFunctions.PreferredRegion(configuration.Model, view.Regions));
        Add(_video, AtariVideoAudioSettingsConstants.ResolutionResource,
            AtariVideoAudioSettingsConstants.ResolutionOptionKey, view.Resolutions, configuration.Options,
            AtariVideoAudioSettingsConstants.AutomaticValue);
        Add(_video, AtariVideoAudioSettingsConstants.AspectRatioResource,
            AtariVideoAudioSettingsConstants.AspectRatioOptionKey, view.AspectRatios, configuration.Options,
            AtariVideoAudioSettingsConstants.AutomaticValue);
        Add(_video, AtariVideoAudioSettingsConstants.CropResource,
            AtariVideoAudioSettingsConstants.CropOptionKey, view.Cropping, configuration.Options,
            AtariVideoAudioSettingsConstants.DisabledValue);
        Add(_video, AtariVideoAudioSettingsConstants.FrameSkipResource,
            AtariVideoAudioSettingsConstants.FrameSkipOptionKey, view.FrameSkips, configuration.Options,
            AtariVideoAudioSettingsConstants.MinimumFrameSkip.ToString());
        Configure(_renderer, view.Renderers, view.Renderer.ToString());

        _audioEnabled.Content = LocExtension.Get(AtariVideoAudioSettingsConstants.AudioEnabledResource);
        AtariAccessibilityFunctions.Configure(_audioEnabled,
            LocExtension.Get(AtariVideoAudioSettingsConstants.AudioEnabledResource));
        _audioEnabled.IsChecked = view.AudioEnabled;
        _audioOutput.Children.Add(_audioEnabled);
        Add(_audioOutput, AtariVideoAudioSettingsConstants.AudioOutputResource,
            AtariVideoAudioSettingsConstants.AudioOutputOptionKey, view.Outputs, configuration.Options,
            AtariVideoAudioSettingsConstants.DefaultOutputValue);
        Add(_audioOutput, AtariVideoAudioSettingsConstants.AudioLatencyResource,
            AtariVideoAudioSettingsConstants.AudioLatencyOptionKey, view.Latencies, configuration.Options,
            AtariVideoAudioSettingsConstants.MinimumLatencyMilliseconds.ToString());
        Add(_audioQuality, AtariVideoAudioSettingsConstants.AudioVolumeResource,
            AtariVideoAudioSettingsConstants.AudioVolumeOptionKey, view.Volumes, configuration.Options,
            AtariVideoAudioSettingsConstants.MaximumVolumePercent.ToString());
        Add(_audioQuality, AtariVideoAudioSettingsConstants.AudioQualityResource,
            AtariVideoAudioSettingsConstants.AudioQualityOptionKey, view.Qualities, configuration.Options,
            AtariVideoAudioSettingsConstants.NormalQualityValue);
    }

    internal AtariMachineConfiguration Apply(AtariMachineConfiguration configuration)
    {
        var displayed = _options.Where(value => value.Value.SelectedValue is string)
            .Select(value => KeyValuePair.Create(value.Key, (string)value.Value.SelectedValue));
        var renderer = Enum.TryParse<EmulationVideoRenderer>(_renderer.SelectedValue as string, out var parsed)
            ? parsed : configuration.VideoRenderer;
        return AtariVideoAudioSettingsFunctions.Apply(configuration, displayed,
            _audioEnabled.IsChecked == true, renderer);
    }

    private void Add(Panel panel, string resource, string key, IReadOnlyList<AtariVideoAudioChoice> choices,
        IReadOnlyDictionary<string, string> options, string fallback)
    {
        var editor = new ComboBox();
        Configure(editor, choices, AtariVideoAudioSettingsFunctions.Select(options, key, choices, fallback));
        _options[key] = editor;
        panel.Children.Add(AtariAccessibilityFunctions.LabeledRow(LocExtension.Get(resource), editor));
    }

    private static void Configure(ComboBox editor, IReadOnlyList<AtariVideoAudioChoice> choices, string selected)
    {
        editor.ItemsSource = choices;
        editor.DisplayMemberPath = nameof(AtariVideoAudioChoice.DisplayName);
        editor.SelectedValuePath = nameof(AtariVideoAudioChoice.Value);
        editor.SelectedValue = selected;
    }

}
