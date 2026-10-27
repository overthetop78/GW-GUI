using GWGUI.Emulation.Contracts;

namespace GWGUI.Emulation.Constants;

/// <summary>Choix génériques du facteur de vitesse des lecteurs, réutilisés lorsque le module ne les affine pas.</summary>
public static class EmulationDriveSpeedChoices
{
    /// <summary>Transfert à la vitesse matérielle.</summary>
    public const string Normal = "100";
    /// <summary>Transfert sans limitation de vitesse.</summary>
    public const string Maximum = "0";
    /// <summary>Présélections communes déjà proposées par les dialogues de lecteur.</summary>
    public static IReadOnlyList<EmulationSettingsChoice> Standard { get; } = Array.AsReadOnly<EmulationSettingsChoice>(
    [
        new(Normal, string.Empty, "100 %"),
        new("200", string.Empty, "200 %"),
        new("400", string.Empty, "400 %"),
        new("800", string.Empty, "800 %"),
        new(Maximum, "Emulation.Value.Maximum")
    ]);
}
