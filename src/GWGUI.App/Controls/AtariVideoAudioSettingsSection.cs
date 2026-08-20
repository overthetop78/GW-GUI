using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Localization;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;
using GWGUI.App.Services;

namespace GWGUI.App.Controls;

internal sealed class AtariVideoAudioSettingsSection
{
    private readonly Dictionary<string, ComboBox> _options = new(StringComparer.Ordinal);
    private ComboBox _renderer = new();
    private CheckBox _audioEnabled = new();
    private readonly ContentControl _video = new();
    private readonly ContentControl _rendering = new();
    private readonly ContentControl _audio = new();
    internal UIElement Video { get; }
    internal UIElement Audio { get; }

    internal AtariVideoAudioSettingsSection()
    {
        Video = EmulationSettingsLayout.VideoSettingsPage(_video, _rendering);
        Audio = _audio;
    }

    internal void Load(AtariMachineConfiguration configuration)
    {
        var audioOutputs = new[]
        {
            new AtariVideoAudioChoice(AtariVideoAudioSettingsConstants.DefaultOutputValue,
                LocExtension.Get(AtariVideoAudioSettingsConstants.DefaultAudioOutputResource))
        }.Concat(WasapiAudioOutput.GetOutputDevices().Select(device =>
            new AtariVideoAudioChoice(device.Id, device.Name))).ToArray();
        var view = AtariVideoAudioSettingsFunctions.Create(configuration, audioOutputs);
        _options.Clear();
        _video.Content = null;
        _rendering.Content = null;
        _audio.Content = null;
        _audioEnabled = new CheckBox();
        var videoFields = new List<EmulationVideoSettingsField>();
        AddVideo(videoFields, AtariVideoAudioSettingsConstants.VideoStandardResource,
            AtariVideoAudioSettingsConstants.StandardOptionKey, view.Standards, configuration.Options,
            AtariVideoAudioSettingsFunctions.PreferredVideoStandard(configuration.Model, view.Standards));
        if (AtariCompatibilityCatalog.Get(configuration.Model).Options.Single(option =>
                option.Option == AtariSettingOption.Region).Availability != AtariOptionAvailability.Hidden)
            AddVideo(videoFields, AtariVideoAudioSettingsConstants.RegionResource,
                AtariVideoAudioSettingsConstants.RegionOptionKey, view.Regions, configuration.Options,
                AtariVideoAudioSettingsFunctions.PreferredRegion(configuration.Model, view.Regions));
        AddVideo(videoFields, AtariVideoAudioSettingsConstants.ResolutionResource,
            AtariVideoAudioSettingsConstants.ResolutionOptionKey, view.Resolutions, configuration.Options,
            view.Resolutions[0].Value);
        if (view.ArtifactingModes.Count > 0)
            AddVideo(videoFields, "Emulation.Atari.Video.Artifacting",
                AtariEightBitSettingsConstants.ArtifactingModeOptionKey, view.ArtifactingModes,
                configuration.Options, AtariEightBitSettingsConstants.None);
        if (AtariEightBitSettingsCatalog.SupportsComputerOptions(configuration.Model))
        {
            AddVideo(videoFields, "Emulation.Atari.Video.Hue", AtariEightBitSettingsConstants.ColorHueOptionKey,
                view.ColorHue, configuration.Options, AtariEightBitSettingsConstants.DefaultColorAdjustment);
            AddVideo(videoFields, "Emulation.Atari.Video.Saturation",
                AtariEightBitSettingsConstants.ColorSaturationOptionKey, view.ColorSaturation,
                configuration.Options, AtariEightBitSettingsConstants.DefaultColorAdjustment);
            AddVideo(videoFields, "Emulation.Atari.Video.Contrast",
                AtariEightBitSettingsConstants.ColorContrastOptionKey, view.ColorContrast,
                configuration.Options, AtariEightBitSettingsConstants.DefaultColorAdjustment);
            AddVideo(videoFields, "Emulation.Atari.Video.Brightness",
                AtariEightBitSettingsConstants.ColorBrightnessOptionKey, view.ColorBrightness,
                configuration.Options, AtariEightBitSettingsConstants.DefaultColorAdjustment);
            AddVideo(videoFields, "Emulation.Video.Gamma", AtariEightBitSettingsConstants.ColorGammaOptionKey,
                view.ColorGamma, configuration.Options, AtariEightBitSettingsConstants.DefaultGamma);
            AddVideo(videoFields, "Emulation.Atari.Video.ColorDelay",
                AtariEightBitSettingsConstants.ColorDelayOptionKey, view.ColorDelay,
                configuration.Options, AtariEightBitSettingsConstants.DefaultColorDelay);
            AddVideo(videoFields, "Emulation.Atari.Video.ExternalPalette",
                AtariEightBitSettingsConstants.ExternalPaletteOptionKey, view.ExternalPalettes,
                configuration.Options, AtariEightBitSettingsConstants.None);
        }
        if (configuration.Core == AtariCoreKind.Hatari)
        {
            AddVideo(videoFields, AtariVideoAudioSettingsConstants.AspectRatioResource,
                AtariVideoAudioSettingsConstants.AspectRatioOptionKey, view.AspectRatios, configuration.Options,
                AtariVideoAudioSettingsConstants.AutomaticValue);
            AddVideo(videoFields, AtariVideoAudioSettingsConstants.CropResource,
                AtariVideoAudioSettingsConstants.CropOptionKey, view.Cropping, configuration.Options,
                AtariVideoAudioSettingsConstants.DisabledValue);
            AddVideo(videoFields, AtariVideoAudioSettingsConstants.FrameSkipResource,
                AtariVideoAudioSettingsConstants.FrameSkipOptionKey, view.FrameSkips, configuration.Options,
                AtariVideoAudioSettingsConstants.MinimumFrameSkip.ToString());
        }
        _video.Content = EmulationSettingsLayout.VideoSettingsFields(videoFields.ToArray());
        _renderer = new ComboBox();
        Configure(_renderer, view.Renderers, view.Renderer.ToString());
        _rendering.Content = EmulationSettingsLayout.VideoSettingsFields(
            new EmulationVideoSettingsField(LocExtension.Get("Emulation.Video.Settings.Rendering"), _renderer));

        _audioEnabled.Content = LocExtension.Get(AtariVideoAudioSettingsConstants.AudioEnabledResource);
        AtariAccessibilityFunctions.Configure(_audioEnabled,
            LocExtension.Get(AtariVideoAudioSettingsConstants.AudioEnabledResource));
        _audioEnabled.IsChecked = view.AudioEnabled;
        var outputFields = new List<FrameworkElement>
        {
            EmulationSettingsLayout.AudioCheckBoxField(_audioEnabled)
        };
        AddAudio(outputFields, AtariVideoAudioSettingsConstants.AudioOutputResource,
            AtariVideoAudioSettingsConstants.AudioOutputOptionKey, view.Outputs, configuration.Options,
            AtariVideoAudioSettingsConstants.DefaultOutputValue);
        AddAudio(outputFields, AtariVideoAudioSettingsConstants.AudioLatencyResource,
            AtariVideoAudioSettingsConstants.AudioLatencyOptionKey, view.Latencies, configuration.Options,
            AtariConfigurationOptionConstants.DefaultAudioLatencyMilliseconds.ToString());
        var qualityFields = new List<FrameworkElement>();
        AddAudio(qualityFields, AtariVideoAudioSettingsConstants.AudioVolumeResource,
            AtariVideoAudioSettingsConstants.AudioVolumeOptionKey, view.Volumes, configuration.Options,
            AtariConfigurationOptionConstants.DefaultAudioVolumePercent.ToString());
        if (configuration.Core == AtariCoreKind.Hatari)
        {
            AddAudio(qualityFields, "Emulation.Audio.Floppy.Enabled",
                AtariVideoAudioSettingsConstants.FloppySoundOptionKey, ToggleChoices(), configuration.Options,
                "true");
            AddAudio(qualityFields, "Emulation.Audio.Floppy.Sound",
                AtariVideoAudioSettingsConstants.FloppySoundVolumeOptionKey, PercentageChoices(),
                configuration.Options, "75");
            AddAudio(qualityFields, "Emulation.Audio.PolarizedFilter",
                AtariVideoAudioSettingsConstants.PolarizedFilterOptionKey, ToggleChoices(), configuration.Options,
                "false");
        }
        else if (AtariEightBitSettingsCatalog.SupportsComputerOptions(configuration.Model))
        {
            AddAudio(qualityFields, "Emulation.Atari.Audio.PokeyStereo",
                AtariEightBitSettingsConstants.PokeyStereoOptionKey, AtariToggleChoices(), configuration.Options,
                AtariEightBitSettingsConstants.Disabled);
        }
        _audio.Content = EmulationSettingsLayout.AudioSettingsPage(outputFields, qualityFields);
    }

    internal AtariMachineConfiguration Apply(AtariMachineConfiguration configuration)
    {
        var displayed = _options
            .Where(value => value.Value.SelectedItem is AtariVideoAudioChoice)
            .Select(value => KeyValuePair.Create(value.Key,
                ((AtariVideoAudioChoice)value.Value.SelectedItem).Value));
        var rendererValue = (_renderer.SelectedItem as AtariVideoAudioChoice)?.Value;
        var renderer = Enum.TryParse<EmulationVideoRenderer>(rendererValue, out var parsed)
            ? parsed : configuration.VideoRenderer;
        return AtariVideoAudioSettingsFunctions.Apply(configuration, displayed,
            _audioEnabled.IsChecked == true, renderer);
    }

    private void AddAudio(ICollection<FrameworkElement> fields, string resource, string key,
        IReadOnlyList<AtariVideoAudioChoice> choices,
        IReadOnlyDictionary<string, string> options, string fallback)
    {
        var editor = new ComboBox();
        Configure(editor, choices, AtariVideoAudioSettingsFunctions.Select(options, key, choices, fallback));
        _options[key] = editor;
        fields.Add(EmulationSettingsLayout.AudioChoiceField(LocExtension.Get(resource), editor));
    }

    private void AddVideo(ICollection<EmulationVideoSettingsField> fields, string resource, string key,
        IReadOnlyList<AtariVideoAudioChoice> choices, IReadOnlyDictionary<string, string> options, string fallback)
    {
        var editor = new ComboBox();
        Configure(editor, choices, AtariVideoAudioSettingsFunctions.Select(options, key, choices, fallback));
        _options[key] = editor;
        fields.Add(new EmulationVideoSettingsField(LocExtension.Get(resource), editor));
    }

    private static void Configure(ComboBox editor, IReadOnlyList<AtariVideoAudioChoice> choices, string selected)
    {
        editor.ItemsSource = choices;
        editor.DisplayMemberPath = nameof(AtariVideoAudioChoice.DisplayName);
        editor.SelectedValuePath = nameof(AtariVideoAudioChoice.Value);
        editor.SelectedValue = selected;
    }

    private static IReadOnlyList<AtariVideoAudioChoice> ToggleChoices() =>
    new AtariVideoAudioChoice[]
    {
        new("true", LocExtension.Get(AtariVideoAudioSettingsConstants.EnabledResource)),
        new("false", LocExtension.Get(AtariVideoAudioSettingsConstants.DisabledResource))
    };

    private static IReadOnlyList<AtariVideoAudioChoice> PercentageChoices() =>
        new[] { 25, 50, 75, 100 }.Select(value =>
            new AtariVideoAudioChoice(value.ToString(), $"{value} %")).ToArray();

    private static IReadOnlyList<AtariVideoAudioChoice> AtariToggleChoices() =>
    [
        new(AtariEightBitSettingsConstants.Enabled,
            LocExtension.Get(AtariVideoAudioSettingsConstants.EnabledResource)),
        new(AtariEightBitSettingsConstants.Disabled,
            LocExtension.Get(AtariVideoAudioSettingsConstants.DisabledResource))
    ];

}
