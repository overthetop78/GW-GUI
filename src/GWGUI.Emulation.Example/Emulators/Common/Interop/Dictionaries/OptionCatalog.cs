using GWGUI.Emulation.Example.Emulators.Common.Interop.Constants;

namespace GWGUI.Emulation.Example.Emulators.Common.Interop.Dictionaries;

/// <summary>Options natives de démonstration communes aux trois émulateurs et aux deux profils.</summary>
/// <remarks>Les identités d’options et codes de formats sont des données techniques invariantes.
/// Les libellés généraux d’éditeurs restent fournis par les ressources communes de l’application.</remarks>
internal static class OptionCatalog
{
    /// <summary>Déclarations immuables des options ; les valeurs courantes sont projetées séparément.</summary>
    internal static IReadOnlyList<CoreOption> All { get; } = Array.AsReadOnly<CoreOption>(
    [
        new(OptionConstants.Resolution, "OutputResolution", null, "Video", OptionConstants.DefaultResolution, OptionConstants.DefaultResolution,
            [new(OptionConstants.DefaultResolution, "320 × 200"), new(OptionConstants.LargeResolution, "640 × 400")], RequiresRestart: true, Tab: EmulationMachineTab.Video, ResourceKey: "Emulation.Video.Settings.Display"),
        new(OptionConstants.PixelFormat, "PixelFormat", null, "Video", "xrgb8888", "xrgb8888",
            [new("xrgb8888", "XRGB8888"), new("rgb565", "RGB565"), new("rgb1555", "RGB1555")], RequiresRestart: true, Tab: EmulationMachineTab.Video),
        new(OptionConstants.AudioTone, "AudioTone", null, "Audio", OptionConstants.Disabled, OptionConstants.Disabled,
            [new(OptionConstants.Disabled, OptionConstants.Disabled, ResourceKey: "Emulation.Value.Disabled"), new(OptionConstants.Enabled, OptionConstants.Enabled, ResourceKey: "Emulation.Value.Enabled")], Tab: EmulationMachineTab.Audio)
    ]);

    /// <summary>Projette les valeurs courantes d’une configuration sans modifier les déclarations partagées.</summary>
    /// <param name="configuration">Configuration courante, ou null pour les valeurs initiales.</param>
    /// <returns>Descriptions d’options portant leurs valeurs effectives.</returns>
    internal static IReadOnlyList<CoreOption> For(MachineConfiguration? configuration) => All.Select(option => option with { CurrentValue = configuration?.Options.GetValueOrDefault(option.Key, option.DefaultValue) ?? option.DefaultValue }).ToArray();

    /// <summary>Résout les dimensions d’image qui doivent être allouées par la session.</summary>
    /// <param name="configuration">Choix de résolution enregistré par les éditeurs communs.</param>
    /// <returns>Largeur et hauteur en pixels ; une valeur inconnue est rejetée.</returns>
    internal static (int Width, int Height) Resolution(MachineConfiguration configuration) => configuration.Options.GetValueOrDefault(OptionConstants.Resolution, OptionConstants.DefaultResolution) switch
    {
        OptionConstants.DefaultResolution => (320, 200),
        OptionConstants.LargeResolution => (640, 400),
        _ => throw new ArgumentException(nameof(configuration))
    };

    /// <summary>Résout le format des pixels à produire, indépendamment du présentateur de l’application.</summary>
    /// <param name="configuration">Configuration empruntée dont le choix de format est consulté.</param>
    /// <returns>Format public correspondant au code technique sélectionné.</returns>
    internal static EmulationPixelFormat PixelFormat(MachineConfiguration configuration) => configuration.Options.GetValueOrDefault(OptionConstants.PixelFormat, "xrgb8888") switch
    {
        "xrgb8888" => EmulationPixelFormat.Xrgb8888,
        "rgb565" => EmulationPixelFormat.Rgb565,
        "rgb1555" => EmulationPixelFormat.Rgb1555,
        _ => throw new ArgumentException(nameof(configuration))
    };
}
