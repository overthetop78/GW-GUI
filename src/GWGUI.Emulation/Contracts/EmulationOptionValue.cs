namespace GWGUI.Emulation.Contracts;

/// <summary>Choix natif d'une option, avec ses métadonnées de présentation et de cadence.</summary>
/// <param name="Value">Code technique transmis au cœur.</param>
/// <param name="Label">Libellé technique de repli.</param>
/// <param name="ResourceKey">Clé de traduction, ou null pour une valeur sans traduction.</param>
/// <param name="InvariantDisplayValue">Présentation technique invariante, lorsqu'elle est fournie.</param>
/// <param name="VideoStandard">Norme vidéo associée au choix, ou null sans norme déclarée.</param>
/// <param name="FrequencyHz">Fréquence associée au choix en Hz, ou null sans fréquence déclarée.</param>
/// <param name="MachineIds">Profils compatibles avec ce choix, ou null sans restriction.</param>
/// <param name="FrameSkipRatio">Rapport de saut d'images associé au choix, ou null sans rapport déclaré.</param>
/// <remarks>Le choix conserve les données du catalogue sans imposer de filtrage ni modifier le code natif.</remarks>
public sealed record EmulationOptionValue(string Value, string Label, string? ResourceKey = null, string? InvariantDisplayValue = null, EmulationVideoStandard? VideoStandard = null, double? FrequencyHz = null, IReadOnlySet<string>? MachineIds = null, EmulationFrameSkipRatio? FrameSkipRatio = null);
