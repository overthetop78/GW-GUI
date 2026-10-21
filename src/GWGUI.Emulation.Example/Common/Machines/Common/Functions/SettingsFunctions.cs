using System.Globalization;

namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;

/// <summary>Projection des réglages de matériel, d’audio et de cœur vers les éditeurs communs.</summary>
internal static class SettingsDescriptionFunctions
{
    /// <summary>Décrit le profil et les options annoncées par l’adaptateur sélectionné.</summary>
    /// <param name="configuration">Configuration dont les valeurs courantes sont présentées.</param>
    /// <param name="coreOptions">Options fournies par le cœur ou son catalogue, ou null sans options natives.</param>
    /// <returns>Blocs de données descriptives ; leur création ne charge ni cœur ni sortie audio.</returns>
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration, IReadOnlyList<CoreOption>? coreOptions = null)
    {
        var hardware = HardwareModelCatalog.Get(configuration.Model);
        var blocks = new List<EmulationSettingsBlock>
        {
            new("hardware", EmulationMachineTab.General, "Emulation.Tab.General",
            [
                new(SettingsConstants.Cpu, EmulationMachineTab.General, "hardware", "Emulation.Cpu.Model", EmulationSettingsEditor.Information, hardware.DefaultCpu.ToString()),
                new(SettingsConstants.CpuFrequency, EmulationMachineTab.General, "hardware", "Emulation.Cpu.SpeedOriginal", EmulationSettingsEditor.Information, hardware.DefaultCpuFrequencyHz.ToString(CultureInfo.InvariantCulture) + " Hz"),
                new(SettingsConstants.MainMemory, EmulationMachineTab.General, "hardware", "Emulation.Memory.Main", EmulationSettingsEditor.Information, hardware.MainMemoryBytes.ToString(CultureInfo.InvariantCulture) + " B")
            ]),
            new("audio", EmulationMachineTab.Audio, "Emulation.Audio",
            [
                new(SettingsConstants.AudioEnabled, EmulationMachineTab.Audio, "audio", "Emulation.Audio.Enabled", EmulationSettingsEditor.Toggle, configuration.AudioEnabled.ToString(), EnabledValue: bool.TrueString, DisabledValue: bool.FalseString),
                new(SettingsConstants.AudioOutput, EmulationMachineTab.Audio, "audio", "Emulation.Audio.Output", EmulationSettingsEditor.Selection, configuration.Options.GetValueOrDefault(SettingsConstants.AudioOutput, SettingsConstants.DefaultOutput), [new(SettingsConstants.DefaultOutput, "Emulation.Audio.DefaultOutput")]),
                new(SettingsConstants.AudioLatency, EmulationMachineTab.Audio, "audio", "Emulation.Audio.Latency", EmulationSettingsEditor.Selection, configuration.Options.GetValueOrDefault(SettingsConstants.AudioLatency, AudioConstants.DefaultLatencyMilliseconds.ToString(CultureInfo.InvariantCulture)), AudioConstants.LatencyChoicesMilliseconds.Select(value => new EmulationSettingsChoice(value.ToString(CultureInfo.InvariantCulture), string.Empty, value.ToString(CultureInfo.InvariantCulture) + " ms")).ToArray()),
                new(VideoAudioSettingsConstants.AudioVolumeOption, EmulationMachineTab.Audio, "audio", "Explorer.Volume", EmulationSettingsEditor.Percentage, configuration.Options.GetValueOrDefault(VideoAudioSettingsConstants.AudioVolumeOption, AudioConstants.DefaultVolumePercent.ToString(CultureInfo.InvariantCulture)))
            ])
        };
        foreach (var group in (coreOptions ?? []).Where(option => option.IsVisible).GroupBy(option => option.Tab))
        {
            var blockId = "core-" + group.Key;
            var fields = group.Select(option => new EmulationSettingsField(option.Key, option.Tab, blockId, option.ResourceKey ?? option.Name,
                option.NumericRange is null ? EmulationSettingsEditor.Selection : EmulationSettingsEditor.Number,
                configuration.Options.GetValueOrDefault(option.Key, option.CurrentValue),
                option.Values.Select(value => new EmulationSettingsChoice(value.Value, value.ResourceKey ?? string.Empty, value.ResourceKey is null || value.FrameSkipRatio is not null ? ChoiceLabel(value) : null)).ToArray(),
                IsEnabled: option.IsEnabled, ExplanationResourceKey: option.ExplanationResourceKey, RequiresRestart: option.RequiresRestart, NumericRange: option.NumericRange)).ToArray();
            blocks.Add(new(blockId, group.Key, "Emulation.Tab." + group.Key, fields));
        }
        return blocks;
    }

    /// <summary>Conserve le libellé de choix fourni par le cœur ou représente son rapport de saut d’images.</summary>
    /// <param name="value">Choix natif et ses métadonnées de présentation.</param>
    /// <returns>Libellé technique invariant ou rapport numérique explicite.</returns>
    internal static string ChoiceLabel(CoreOptionValue value) => value.FrameSkipRatio switch
    {
        FrameSkipRatio.OneOfTwo => "1/2",
        FrameSkipRatio.TwoOfThree => "2/3",
        FrameSkipRatio.ThreeOfFour => "3/4",
        FrameSkipRatio.FourOfFive => "4/5",
        FrameSkipRatio.FiveOfSix => "5/6",
        _ => value.Label
    };
}

/// <summary>Application des valeurs d’éditeurs à une nouvelle configuration du profil.</summary>
internal static class SettingsApplicationFunctions
{
    /// <summary>Conserve les valeurs techniques du cœur et traite séparément l’autorisation audio.</summary>
    /// <param name="configuration">Configuration initiale à conserver immuable.</param>
    /// <param name="values">Valeurs proposées par l’hôte ; null retire une option.</param>
    /// <returns>Configuration validée par les mêmes règles que les montages et le magasin.</returns>
    internal static MachineConfiguration Apply(MachineConfiguration configuration, IReadOnlyDictionary<string, string?> values)
    {
        var options = new Dictionary<string, string>(configuration.Options, StringComparer.Ordinal);
        var audioEnabled = configuration.AudioEnabled;
        foreach (var value in values)
        {
            if (value.Key == SettingsConstants.AudioEnabled)
            {
                if (!bool.TryParse(value.Value, out audioEnabled)) throw new ArgumentException(nameof(values));
                continue;
            }
            // Les trois informations matérielles sont descriptives : les valeurs renvoyées par l’éditeur ne deviennent pas des options natives.
            if (value.Key == SettingsConstants.Cpu || value.Key == SettingsConstants.CpuFrequency || value.Key == SettingsConstants.MainMemory) continue;
            if (value.Value is null) options.Remove(value.Key); else options[value.Key] = value.Value;
        }
        var result = configuration with { Options = options, AudioEnabled = audioEnabled };
        ConfigurationFunctions.Validate(result);
        return result;
    }
}
