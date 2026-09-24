using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Functions;

internal static partial class AtariSettingsDescriptionFunctions
{
    private static EmulationSettingsBlock Audio(AtariMachineConfiguration configuration, bool isHatari) =>
        Block(AtariSettingsDescriptionFunctionsConstants.Audio, EmulationMachineTab.Audio, AtariSettingsDescriptionFunctionsConstants.ResourceAudio, AtariSettingsDescriptionFunctionsConstants.Value9, 2,
            Toggle(AtariSettingsConstants.AudioEnabled, EmulationMachineTab.Audio, AtariSettingsDescriptionFunctionsConstants.Audio,
                AtariSettingsDescriptionFunctionsConstants.ResourceAudioEnabled, configuration.AudioEnabled),
            Select(AtariVideoAudioSettingsConstants.AudioOutputOption, EmulationMachineTab.Audio, AtariSettingsDescriptionFunctionsConstants.Audio,
                AtariSettingsDescriptionFunctionsConstants.ResourceAudioOutput, Value(configuration, AtariVideoAudioSettingsConstants.AudioOutputOption,
                    AtariConfigurationOptionConstants.DefaultAudioOutput), DefaultAudioOutput()) with
            { ChoiceSource = EmulationSettingsChoiceSource.AudioOutputDevices },
            Select(AtariVideoAudioSettingsConstants.AudioLatencyOption, EmulationMachineTab.Audio, AtariSettingsDescriptionFunctionsConstants.Audio,
                AtariSettingsDescriptionFunctionsConstants.ResourceAudioLatency, Value(configuration, AtariVideoAudioSettingsConstants.AudioLatencyOption,
                    AtariConfigurationOptionConstants.DefaultAudioLatencyMilliseconds.ToString()),
                AtariVideoAudioSettingsConstants.AudioLatenciesMilliseconds.Select(Milliseconds)),
            Select(AtariVideoAudioSettingsConstants.AudioVolumeOption, EmulationMachineTab.Audio, AtariSettingsDescriptionFunctionsConstants.Audio,
                AtariSettingsDescriptionFunctionsConstants.ExplorerVolume, Value(configuration, AtariVideoAudioSettingsConstants.AudioVolumeOption,
                    AtariConfigurationOptionConstants.DefaultAudioVolumePercent.ToString()), Percentages(
                        AtariVideoAudioSettingsConstants.MinimumVolumePercent,
                        AtariVideoAudioSettingsConstants.MaximumVolumePercent,
                        AtariVideoAudioSettingsConstants.VolumeStepPercent)),
            Toggle(AtariVideoAudioSettingsConstants.FloppySoundOption, EmulationMachineTab.Audio, AtariSettingsDescriptionFunctionsConstants.Audio,
                AtariSettingsDescriptionFunctionsConstants.ResourceAudioFloppyEnabled, Value(configuration,
                    AtariVideoAudioSettingsConstants.FloppySoundOption, AtariSettingsDescriptionFunctionsConstants.True) == AtariSettingsDescriptionFunctionsConstants.True,
                AtariSettingsDescriptionFunctionsConstants.True, AtariSettingsDescriptionFunctionsConstants.False) with
            { IsVisible = isHatari },
            Select(AtariVideoAudioSettingsConstants.FloppySoundVolumeOption, EmulationMachineTab.Audio, AtariSettingsDescriptionFunctionsConstants.Audio,
                AtariSettingsDescriptionFunctionsConstants.ResourceAudioFloppySound, Value(configuration,
                    AtariVideoAudioSettingsConstants.FloppySoundVolumeOption, AtariSettingsDescriptionFunctionsConstants.Value75),
                AtariVideoAudioSettingsConstants.FloppySoundVolumesPercent.Select(Percentage))
                with
            { IsVisible = isHatari },
            Toggle(AtariVideoAudioSettingsConstants.PolarizedFilterOption, EmulationMachineTab.Audio, AtariSettingsDescriptionFunctionsConstants.Audio,
                AtariSettingsDescriptionFunctionsConstants.ResourceAudioPolarizedFilter, Value(configuration,
                    AtariVideoAudioSettingsConstants.PolarizedFilterOption, AtariSettingsDescriptionFunctionsConstants.False) == AtariSettingsDescriptionFunctionsConstants.True,
                AtariSettingsDescriptionFunctionsConstants.True, AtariSettingsDescriptionFunctionsConstants.False) with
            { IsVisible = isHatari });

    private static IReadOnlyList<EmulationSettingsChoice> StStandards(AtariStModelDefinition model)
    {
        var choices = new List<EmulationSettingsChoice>
        {
            new(AtariVideoAudioSettingsConstants.Automatic, AtariSettingsDescriptionFunctionsConstants.VisualAutomatic)
        };
        if (model.Video.Contains(AtariStVideoCapability.Pal)) choices.Add(Invariant(AtariSettingsDescriptionFunctionsConstants.PAL));
        if (model.Video.Contains(AtariStVideoCapability.Ntsc)) choices.Add(Invariant(AtariSettingsDescriptionFunctionsConstants.NTSC));
        if (model.Video.Contains(AtariStVideoCapability.Monochrome)) choices.Add(Invariant(AtariSettingsDescriptionFunctionsConstants.Monochrome));
        return choices;
    }

    private static IReadOnlyList<EmulationSettingsChoice> AutomaticAndNative() =>
    [
        new(AtariVideoAudioSettingsConstants.Automatic, AtariSettingsDescriptionFunctionsConstants.VisualAutomatic),
        Invariant(AtariVideoAudioSettingsConstants.Native)
    ];

    private static IReadOnlyList<EmulationSettingsChoice> AspectRatios() =>
    [
        new(AtariVideoAudioSettingsConstants.Automatic, AtariSettingsDescriptionFunctionsConstants.VisualAutomatic),
        Invariant(AtariVideoAudioSettingsConstants.FourByThree),
        Invariant(AtariVideoAudioSettingsConstants.PixelAspect)
    ];

    private static IReadOnlyList<EmulationSettingsChoice> FrameSkips() =>
        Enumerable.Range(AtariVideoAudioSettingsConstants.MinimumFrameSkip,
                AtariVideoAudioSettingsConstants.MaximumFrameSkip
                - AtariVideoAudioSettingsConstants.MinimumFrameSkip + 1)
            .Select(value => Invariant(value.ToString())).Append(Invariant(AtariSettingsDescriptionFunctionsConstants.Value10)).ToArray();

    private static IReadOnlyList<EmulationSettingsChoice> Resolutions(AtariMachineModel model) =>
        AtariEightBitSettingsCatalog.SupportsComputerOptions(model)
            ? AtariEightBitSettingsCatalog.OriginalComputerResolutions.Select(value =>
                new EmulationSettingsChoice(value, string.Empty,
                    value.Replace(AtariSettingsDescriptionFunctionsConstants.X, AtariSettingsDescriptionFunctionsConstants.Value11, StringComparison.Ordinal))).ToArray()
            : AutomaticAndNative();

    private static string DefaultResolution(AtariMachineModel model) =>
        AtariEightBitSettingsCatalog.SupportsComputerOptions(model)
            ? AtariEightBitSettingsCatalog.OriginalComputerResolutions[0]
            : AtariVideoAudioSettingsConstants.Automatic;

    private static IReadOnlyList<EmulationSettingsChoice> DefaultAudioOutput() =>
    [
        new(AtariConfigurationOptionConstants.DefaultAudioOutput, AtariSettingsDescriptionFunctionsConstants.ResourceAudioDefaultOutput)
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
