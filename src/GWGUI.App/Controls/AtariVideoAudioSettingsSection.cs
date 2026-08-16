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
    internal StackPanel Video { get; } = new();
    internal StackPanel Audio { get; } = new();

    internal void Load(AtariMachineConfiguration configuration)
    {
        var view = AtariVideoAudioSettingsFunctions.Create(configuration);
        _options.Clear();
        Video.Children.Clear();
        Audio.Children.Clear();
        Add(Video, AtariVideoAudioSettingsConstants.VideoStandardResource,
            AtariVideoAudioSettingsConstants.StandardOptionKey, view.Standards, configuration.Options,
            view.Standards.First().Value);
        Add(Video, AtariVideoAudioSettingsConstants.RegionResource,
            AtariVideoAudioSettingsConstants.RegionOptionKey, view.Regions, configuration.Options,
            view.Regions.First().Value);
        Add(Video, AtariVideoAudioSettingsConstants.ResolutionResource,
            AtariVideoAudioSettingsConstants.ResolutionOptionKey, view.Resolutions, configuration.Options,
            AtariVideoAudioSettingsConstants.AutomaticValue);
        Add(Video, AtariVideoAudioSettingsConstants.AspectRatioResource,
            AtariVideoAudioSettingsConstants.AspectRatioOptionKey, view.AspectRatios, configuration.Options,
            AtariVideoAudioSettingsConstants.AutomaticValue);
        Add(Video, AtariVideoAudioSettingsConstants.CropResource,
            AtariVideoAudioSettingsConstants.CropOptionKey, view.Cropping, configuration.Options,
            AtariVideoAudioSettingsConstants.DisabledValue);
        Add(Video, AtariVideoAudioSettingsConstants.FrameSkipResource,
            AtariVideoAudioSettingsConstants.FrameSkipOptionKey, view.FrameSkips, configuration.Options,
            AtariVideoAudioSettingsConstants.MinimumFrameSkip.ToString());
        Configure(_renderer, view.Renderers, view.Renderer.ToString());
        Video.Children.Add(Row(AtariVideoAudioSettingsConstants.RenderingResource, _renderer));

        _audioEnabled.Content = LocExtension.Get(AtariVideoAudioSettingsConstants.AudioEnabledResource);
        _audioEnabled.IsChecked = view.AudioEnabled;
        Audio.Children.Add(_audioEnabled);
        Add(Audio, AtariVideoAudioSettingsConstants.AudioOutputResource,
            AtariVideoAudioSettingsConstants.AudioOutputOptionKey, view.Outputs, configuration.Options,
            AtariVideoAudioSettingsConstants.DefaultOutputValue);
        Add(Audio, AtariVideoAudioSettingsConstants.AudioLatencyResource,
            AtariVideoAudioSettingsConstants.AudioLatencyOptionKey, view.Latencies, configuration.Options,
            AtariVideoAudioSettingsConstants.MinimumLatencyMilliseconds.ToString());
        Add(Audio, AtariVideoAudioSettingsConstants.AudioVolumeResource,
            AtariVideoAudioSettingsConstants.AudioVolumeOptionKey, view.Volumes, configuration.Options,
            AtariVideoAudioSettingsConstants.MaximumVolumePercent.ToString());
        Add(Audio, AtariVideoAudioSettingsConstants.AudioQualityResource,
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
        panel.Children.Add(Row(resource, editor));
    }

    private static void Configure(ComboBox editor, IReadOnlyList<AtariVideoAudioChoice> choices, string selected)
    {
        editor.ItemsSource = choices;
        editor.DisplayMemberPath = nameof(AtariVideoAudioChoice.DisplayName);
        editor.SelectedValuePath = nameof(AtariVideoAudioChoice.Value);
        editor.SelectedValue = selected;
    }

    private static UIElement Row(string resource, UIElement editor)
    {
        var row = new Grid { Margin = new Thickness(0, 4, 0, 4) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
        row.ColumnDefinitions.Add(new ColumnDefinition());
        row.Children.Add(new TextBlock { Text = LocExtension.Get(resource), VerticalAlignment = VerticalAlignment.Center });
        Grid.SetColumn(editor, 1);
        row.Children.Add(editor);
        return row;
    }
}
