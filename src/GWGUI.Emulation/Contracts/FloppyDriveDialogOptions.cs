namespace GWGUI.Emulation.Contracts;

public sealed record FloppyDriveDialogOptions(IReadOnlyList<FloppyDriveModelChoice> Models, string ImageDirectory, string ImageFilter, string DefaultExtension, bool CanCreateBlankMedia = true, bool CanConfigureBehavior = true, bool CanRedirectWrites = true)
{
    /// <summary>Affiche une politique globale de protection uniquement si le module la propose.</summary>
    public bool CanConfigureWriteProtection { get; init; } = true;

    /// <summary>Vitesses autorisées par le module ; null réutilise les présélections génériques du SDK.</summary>
    public IReadOnlyList<EmulationSettingsChoice>? SpeedChoices { get; init; }
}
