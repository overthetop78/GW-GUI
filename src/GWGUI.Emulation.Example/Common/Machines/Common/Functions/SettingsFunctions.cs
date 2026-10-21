using System.Globalization;

namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;

/// <summary>Décrit les réglages matériels et audio sans construire de contrôles graphiques.</summary>
/// <remarks>L’application choisit les contrôles à partir des descriptions du contrat public ; le module fournit valeurs et clés de ressources.</remarks>
internal static class SettingsDescriptionFunctions
{
    /// <summary>Assemble les blocs du modèle puis place devant eux les réglages propres à l’adaptateur.</summary>
    /// <param name="configuration">Configuration du modèle et de son cœur sélectionné.</param>
    /// <returns>Blocs CPU, mémoire, vidéo et audio, précédés des éventuels blocs de l’adaptateur.</returns>
    /// <remarks>La fréquence CPU est affichée en MHz et la mémoire en Kio. Les informations matérielles sont en lecture seule.</remarks>
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration)
    {
        var hardware = HardwareModelCatalog.Get(configuration.Model);
        var cpuResource = SettingsConstants.CpuResourcePrefix + hardware.DefaultCpu;
        var blocks = new List<EmulationSettingsBlock>
        {
            Block(EmulationMachineTab.Cpu, SettingsConstants.ResourceCpu,
                new EmulationSettingsField(SettingsConstants.Cpu, EmulationMachineTab.Cpu, EmulationMachineTab.Cpu.ToString(), SettingsConstants.ResourceCpu, EmulationSettingsEditor.Information, cpuResource, [new(cpuResource, cpuResource)]),
                Information(SettingsConstants.CpuFrequency, EmulationMachineTab.Cpu, SettingsConstants.ResourceFrequency, EmulationHardwareSettingsFunctions.FormatMegahertz(hardware.DefaultCpuFrequencyHz, "0.##"))),
            Block(EmulationMachineTab.Ram, SettingsConstants.ResourceMemory,
                Information(SettingsConstants.MainMemory, EmulationMachineTab.Ram, SettingsConstants.ResourceMemory, $"{hardware.MainMemoryBytes / SettingsConstants.Kibibyte} KiB")),
            Block(EmulationMachineTab.Video, SettingsConstants.ResourceVideo,
                Information(SettingsConstants.Video + "_controller", EmulationMachineTab.Video, SettingsConstants.ResourceVideoController, "Motorola MC6845"),
                Information(SettingsConstants.Video + "_pixels", EmulationMachineTab.Video, SettingsConstants.ResourceVideoPixels, "Acorn Video ULA"),
                Information(SettingsConstants.Video + "_teletext", EmulationMachineTab.Video, SettingsConstants.ResourceTeletext, "Mullard SAA5050")),
            Audio(configuration)
        };
        if (configuration.Core != Emulator.None && new Engine().Adapter(configuration) is IEmulatorSettings settings)
            blocks.InsertRange(BufferConstants.FirstCollectionIndex, settings.DescribeSettings(configuration));
        return blocks;
    }

    /// <summary>Décrit le bloc audio compact : activation, périphérique de sortie, latence et volume.</summary>
    /// <param name="configuration">Configuration fournissant l’activation et les options audio enregistrées.</param>
    /// <returns>Bloc utilisant les choix communs de latence en millisecondes et de volume en pourcentage.</returns>
    private static EmulationSettingsBlock Audio(MachineConfiguration configuration) =>
        Block(EmulationMachineTab.Audio, SettingsConstants.ResourceAudio,
            EmulationHardwareSettingsFunctions.Toggle(SettingsConstants.AudioEnabled, EmulationMachineTab.Audio, EmulationMachineTab.Audio.ToString(), SettingsConstants.ResourceAudioEnabled, configuration.AudioEnabled, null, null),
            EmulationHardwareSettingsFunctions.AudioOutput(SettingsConstants.AudioOutput, EmulationMachineTab.Audio.ToString(), SettingsConstants.ResourceAudioOutput, configuration.Options.GetValueOrDefault(SettingsConstants.AudioOutput, SettingsConstants.DefaultOutput), [new(SettingsConstants.DefaultOutput, SettingsConstants.ResourceDefaultOutput)], null, null),
            Selection(configuration, SettingsConstants.AudioLatency, SettingsConstants.ResourceAudioLatency, EmulationSettingConstants.DefaultAudioLatencyMilliseconds, EmulationSettingConstants.AudioLatencyMillisecondsChoices, "ms"),
            Selection(configuration, VideoAudioSettingsConstants.AudioVolumeOption, SettingsConstants.ResourceAudioVolume, EmulationSettingConstants.DefaultAudioVolumePercent, EmulationSettingConstants.AudioVolumePercentChoices, "%")) with { IsCompact = true };

    /// <summary>Construit une sélection audio numérique à partir des options de la configuration.</summary>
    /// <param name="configuration">Configuration contenant les valeurs textuelles des options.</param>
    /// <param name="id">Identifiant de l’option et du champ.</param>
    /// <param name="resource">Clé de traduction du libellé.</param>
    /// <param name="defaultValue">Valeur de repli employée par le service commun.</param>
    /// <param name="values">Choix numériques proposés.</param>
    /// <param name="unit">Unité affichée, par exemple ms ou %.</param>
    /// <returns>Description du champ de sélection dans l’onglet audio.</returns>
    private static EmulationSettingsField Selection(MachineConfiguration configuration, string id, string resource, int defaultValue, IReadOnlyList<int> values, string unit) =>
        EmulationHardwareSettingsFunctions.AudioSelection(configuration.Options, id, EmulationMachineTab.Audio.ToString(), resource, defaultValue, values, unit, null);


    /// <summary>Construit une information matérielle affichée en lecture seule.</summary>
    /// <param name="id">Identifiant stable du champ.</param>
    /// <param name="tab">Onglet de destination, également utilisé comme groupe.</param>
    /// <param name="resource">Clé de traduction du libellé.</param>
    /// <param name="value">Texte de la valeur déjà formatée.</param>
    /// <returns>Champ utilisant l’éditeur Information.</returns>
    private static EmulationSettingsField Information(string id, EmulationMachineTab tab, string resource, string value) => new(id, tab, tab.ToString(), resource, EmulationSettingsEditor.Information, value);

    /// <summary>Regroupe des champs dans un bloc de réglages.</summary>
    /// <param name="tab">Onglet recevant le bloc.</param>
    /// <param name="resource">Clé utilisée pour l’identité du bloc et son titre traduit.</param>
    /// <param name="fields">Descriptions des champs, dans leur ordre d’affichage.</param>
    /// <returns>Bloc descriptif transmis à l’application.</returns>
    private static EmulationSettingsBlock Block(EmulationMachineTab tab, string resource, params EmulationSettingsField[] fields) => new(resource, tab, resource, fields);
}
