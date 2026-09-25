using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Functions;

internal static partial class SettingsDescriptionFunctions
{
    private static EmulationSettingsBlock Audio(MachineConfiguration configuration, bool isHatari) =>
        Block(SettingsDescriptionFunctionsConstants.Audio, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.ResourceAudio, SettingsDescriptionFunctionsConstants.Value9, 2,
            Toggle(SettingsConstants.AudioEnabled, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.Audio,
                SettingsDescriptionFunctionsConstants.ResourceAudioEnabled, configuration.AudioEnabled),
            Select(VideoAudioSettingsConstants.AudioOutputOption, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.Audio,
                SettingsDescriptionFunctionsConstants.ResourceAudioOutput, Value(configuration, VideoAudioSettingsConstants.AudioOutputOption,
                    ConfigurationOptionConstants.DefaultAudioOutput), DefaultAudioOutput()) with
            { ChoiceSource = EmulationSettingsChoiceSource.AudioOutputDevices },
            Select(VideoAudioSettingsConstants.AudioLatencyOption, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.Audio,
                SettingsDescriptionFunctionsConstants.ResourceAudioLatency, Value(configuration, VideoAudioSettingsConstants.AudioLatencyOption,
                    ConfigurationOptionConstants.DefaultAudioLatencyMilliseconds.ToString()),
                VideoAudioSettingsConstants.AudioLatenciesMilliseconds.Select(Milliseconds)),
            Select(VideoAudioSettingsConstants.AudioVolumeOption, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.Audio,
                SettingsDescriptionFunctionsConstants.ExplorerVolume, Value(configuration, VideoAudioSettingsConstants.AudioVolumeOption,
                    ConfigurationOptionConstants.DefaultAudioVolumePercent.ToString()), Percentages(
                        VideoAudioSettingsConstants.MinimumVolumePercent,
                        VideoAudioSettingsConstants.MaximumVolumePercent,
                        VideoAudioSettingsConstants.VolumeStepPercent)),
            Toggle(VideoAudioSettingsConstants.FloppySoundOption, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.Audio,
                SettingsDescriptionFunctionsConstants.ResourceAudioFloppyEnabled, Value(configuration,
                    VideoAudioSettingsConstants.FloppySoundOption, SettingsDescriptionFunctionsConstants.True) == SettingsDescriptionFunctionsConstants.True,
                SettingsDescriptionFunctionsConstants.True, SettingsDescriptionFunctionsConstants.False) with
            { IsVisible = isHatari },
            Select(VideoAudioSettingsConstants.FloppySoundVolumeOption, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.Audio,
                SettingsDescriptionFunctionsConstants.ResourceAudioFloppySound, Value(configuration,
                    VideoAudioSettingsConstants.FloppySoundVolumeOption, SettingsDescriptionFunctionsConstants.Value75),
                VideoAudioSettingsConstants.FloppySoundVolumesPercent.Select(Percentage))
                with
            { IsVisible = isHatari },
            Toggle(VideoAudioSettingsConstants.PolarizedFilterOption, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.Audio,
                SettingsDescriptionFunctionsConstants.ResourceAudioPolarizedFilter, Value(configuration,
                    VideoAudioSettingsConstants.PolarizedFilterOption, SettingsDescriptionFunctionsConstants.False) == SettingsDescriptionFunctionsConstants.True,
                SettingsDescriptionFunctionsConstants.True, SettingsDescriptionFunctionsConstants.False) with
            { IsVisible = isHatari });

    private static IReadOnlyList<EmulationSettingsChoice> StStandards(StModelDefinition model)
    {
        var choices = new List<EmulationSettingsChoice>
        {
            new(VideoAudioSettingsConstants.Automatic, SettingsDescriptionFunctionsConstants.VisualAutomatic)
        };
        if (model.Video.Contains(StVideoCapability.Pal)) choices.Add(Invariant(SettingsDescriptionFunctionsConstants.PAL));
        if (model.Video.Contains(StVideoCapability.Ntsc)) choices.Add(Invariant(SettingsDescriptionFunctionsConstants.NTSC));
        if (model.Video.Contains(StVideoCapability.Monochrome)) choices.Add(Invariant(SettingsDescriptionFunctionsConstants.Monochrome));
        return choices;
    }

    private static IReadOnlyList<EmulationSettingsChoice> AutomaticAndNative() =>
    [
        new(VideoAudioSettingsConstants.Automatic, SettingsDescriptionFunctionsConstants.VisualAutomatic),
        Invariant(VideoAudioSettingsConstants.Native)
    ];

    private static IReadOnlyList<EmulationSettingsChoice> AspectRatios() =>
    [
        new(VideoAudioSettingsConstants.Automatic, SettingsDescriptionFunctionsConstants.VisualAutomatic),
        Invariant(VideoAudioSettingsConstants.FourByThree),
        Invariant(VideoAudioSettingsConstants.PixelAspect)
    ];

    private static IReadOnlyList<EmulationSettingsChoice> FrameSkips() =>
        Enumerable.Range(VideoAudioSettingsConstants.MinimumFrameSkip,
                VideoAudioSettingsConstants.MaximumFrameSkip
                - VideoAudioSettingsConstants.MinimumFrameSkip + 1)
            .Select(value => Invariant(value.ToString())).Append(Invariant(SettingsDescriptionFunctionsConstants.Value10)).ToArray();

    private static IReadOnlyList<EmulationSettingsChoice> Resolutions(MachineModel model) =>
        EightBitSettingsCatalog.SupportsComputerOptions(model)
            ? EightBitSettingsCatalog.OriginalComputerResolutions.Select(value =>
                new EmulationSettingsChoice(value, string.Empty,
                    value.Replace(SettingsDescriptionFunctionsConstants.X, SettingsDescriptionFunctionsConstants.Value11, StringComparison.Ordinal))).ToArray()
            : AutomaticAndNative();

    private static string DefaultResolution(MachineModel model) =>
        EightBitSettingsCatalog.SupportsComputerOptions(model)
            ? EightBitSettingsCatalog.OriginalComputerResolutions[0]
            : VideoAudioSettingsConstants.Automatic;

    private static IReadOnlyList<EmulationSettingsChoice> DefaultAudioOutput() =>
    [
        new(ConfigurationOptionConstants.DefaultAudioOutput, SettingsDescriptionFunctionsConstants.ResourceAudioDefaultOutput)
    ];

    private static EmulationSettingsChoice Milliseconds(int value) =>
        new(value.ToString(), string.Empty, $"{value} ms", value);

    private static EmulationSettingsChoice Percentage(int value) =>
        new(value.ToString(), string.Empty, $"{value} %", value);

    private static IReadOnlyList<EmulationSettingsChoice> Percentages(int minimum, int maximum, int step) =>
        Enumerable.Range(0, (maximum - minimum) / step + 1)
            .Select(index => Percentage(minimum + index * step)).ToArray();

    private static EmulationSettingsChoice Invariant(string value) => new(value, string.Empty, value);

}
