using System.Globalization;

namespace GWGUI.Emulation.Functions;

/// <summary>Construction commune des réglages matériels, audio et des champs de configuration.</summary>
/// <remarks>Les clés de ressources partagées restent dans le SDK ; les noms et valeurs invariantes ne sont pas dupliqués dans les traductions des modules.</remarks>
internal static class EmulationHardwareSettingsFunctions
{
    private const string CpuResource = "Emulation.Cpu.Processor";
    private const string CpuModelResource = "Emulation.Cpu.Model";
    private const string CpuFrequencyResource = "Emulation.Cpu.SpeedOriginal";
    private const string MemoryResource = "Emulation.Memory.Main";
    private const string ResolutionResource = "Emulation.Video.Resolution";

    /// <summary>Crée un choix dont le texte est invariant, sans inventer une clé de traduction.</summary>
    internal static EmulationSettingsChoice InvariantChoice(string id, string text, long? numericValue = null) =>
        new(id, string.Empty, text, numericValue);

    /// <summary>Ajoute les blocs CPU, mémoire et résolution à partir des caractéristiques réellement fournies.</summary>
    internal static void AddBlocks(List<EmulationSettingsBlock> blocks, string fieldPrefix, string cpuResourceKey,
        Func<string?> cpuFrequency, Func<string?> mainMemory, Func<string?> displayMemory,
        string displayMemoryResourceKey, Func<string?> resolution, bool isCompactCpu)
    {
        var cpu = new List<EmulationSettingsField>
        {
            new(fieldPrefix + "_cpu_model", EmulationMachineTab.Cpu, CpuResource, CpuModelResource,
                EmulationSettingsEditor.Information, cpuResourceKey, [new(cpuResourceKey, cpuResourceKey)])
        };
        if (cpuFrequency() is { } frequency)
            cpu.Add(Information(fieldPrefix + "_cpu_frequency", EmulationMachineTab.Cpu, CpuResource, CpuFrequencyResource, frequency));
        blocks.Add(new(CpuResource, EmulationMachineTab.Cpu, CpuResource, cpu.ToArray(), IsCompact: isCompactCpu));
        var memory = new List<EmulationSettingsField>();
        if (mainMemory() is { } main)
            memory.Add(Information(fieldPrefix + "_main_memory", EmulationMachineTab.Ram, MemoryResource, MemoryResource, main));
        if (displayMemory() is { } display)
            memory.Add(Information(fieldPrefix + "_display_memory", EmulationMachineTab.Ram, MemoryResource, displayMemoryResourceKey, display));
        if (memory.Count > 0)
            blocks.Add(new(MemoryResource, EmulationMachineTab.Ram, MemoryResource, memory.ToArray()));
        if (resolution() is { } dimensions)
            blocks.Add(new(ResolutionResource, EmulationMachineTab.Video, ResolutionResource,
                [Information(fieldPrefix + "_display_resolution", EmulationMachineTab.Video, ResolutionResource, ResolutionResource, dimensions)]));
    }

    /// <summary>Formate la fréquence en MHz ou kHz selon la politique demandée et la culture courante.</summary>
    internal static string FormatFrequency(long hertz, bool useKilohertz = false) =>
        !useKilohertz || hertz >= 1_000_000
            ? FormatMegahertz(hertz, "0.######")
            : $"{(hertz / 1_000d).ToString("0.###", CultureInfo.CurrentCulture)} kHz";

    /// <summary>Formate une fréquence en MHz avec le format numérique demandé.</summary>
    internal static string FormatMegahertz(long hertz, string format) =>
        $"{(hertz / 1_000_000d).ToString(format, CultureInfo.CurrentCulture)} MHz";

    /// <summary>Formate la capacité mémoire avec la représentation invariante commune.</summary>
    internal static string FormatMemory(long bytes) => bytes >= 1024
        ? $"{(bytes / 1024).ToString(CultureInfo.InvariantCulture)} KiB"
        : $"{bytes.ToString(CultureInfo.InvariantCulture)} B";

    private static EmulationSettingsField Information(string id, EmulationMachineTab tab, string block, string label, string value) =>
        Information(id, tab, block, label, value, null, null);

    /// <summary>Crée un champ informatif avec les ressources d’aide fournies.</summary>
    internal static EmulationSettingsField Information(string id, EmulationMachineTab tab, string block, string label, string value,
        string? shortHelp, string? detailedHelp) =>
        new(id, tab, block, label, EmulationSettingsEditor.Information, value,
            ExplanationResourceKey: shortHelp, DetailedExplanationResourceKey: detailedHelp);

    /// <summary>Crée un bloc avec son onglet, ses colonnes et sa présentation compacte éventuelle.</summary>
    internal static EmulationSettingsBlock Block(string id, EmulationMachineTab tab, string title, string icon, int columns,
        EmulationSettingsField[] fields, bool isCompact = false) => new(id, tab, title, fields, icon, columns, IsCompact: isCompact);

    /// <summary>Crée un réglage booléen avec les valeurs activée et désactivée communes.</summary>
    internal static EmulationSettingsField Toggle(string id, EmulationMachineTab tab, string block, string label, bool value,
        string? shortHelp, string? detailedHelp, bool refreshSettingsOnChange = false) =>
        new(id, tab, block, label, EmulationSettingsEditor.Toggle,
            value ? EmulationSettingConstants.Enabled : EmulationSettingConstants.Disabled,
            ExplanationResourceKey: shortHelp, DetailedExplanationResourceKey: detailedHelp,
            RefreshSettingsOnChange: refreshSettingsOnChange);

    /// <summary>Crée un champ numérique décrit par le module.</summary>
    internal static EmulationSettingsField Number(string id, EmulationMachineTab tab, string block, string label, string value,
        string? shortHelp, string? detailedHelp) =>
        new(id, tab, block, label, EmulationSettingsEditor.Number, value,
            ExplanationResourceKey: shortHelp, DetailedExplanationResourceKey: detailedHelp);

    /// <summary>Crée une sélection déclarée, avec aides et exigence éventuelle de redémarrage.</summary>
    internal static EmulationSettingsField Select(string id, EmulationMachineTab tab, string block, string label, string value,
        IReadOnlyList<EmulationSettingsChoice> choices, string? shortHelp, string? detailedHelp,
        bool isEnabled = true, bool requiresRestart = false) =>
        new(id, tab, block, label, EmulationSettingsEditor.Selection, value, choices,
            IsEnabled: isEnabled, ExplanationResourceKey: shortHelp, DetailedExplanationResourceKey: detailedHelp,
            RequiresRestart: requiresRestart);

    /// <summary>Crée la sélection des sorties audio disponibles dans l’hôte.</summary>
    internal static EmulationSettingsField AudioOutput(string id, string block, string label, string value,
        IReadOnlyList<EmulationSettingsChoice> choices, string? shortHelp, string? detailedHelp) =>
        new(id, EmulationMachineTab.Audio, block, label, EmulationSettingsEditor.Selection, value, choices,
            ChoiceSource: EmulationSettingsChoiceSource.AudioOutputDevices,
            ExplanationResourceKey: shortHelp, DetailedExplanationResourceKey: detailedHelp);

    /// <summary>Construit les choix audio numériques à partir des options et valeurs de repli fournies.</summary>
    internal static EmulationSettingsField AudioSelection(IReadOnlyDictionary<string, string> options,
        string id, string block, string resource, int fallback, IReadOnlyList<int> values, string unit,
        IFormatProvider? labelFormatProvider) =>
        Select(id, EmulationMachineTab.Audio, block, resource,
            options.GetValueOrDefault(id, fallback.ToString(CultureInfo.InvariantCulture)),
            values.Select(value => new EmulationSettingsChoice(value.ToString(CultureInfo.InvariantCulture),
                string.Empty, $"{value.ToString(labelFormatProvider)} {unit}")).ToArray(), null, null);
}
