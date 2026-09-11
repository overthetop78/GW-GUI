namespace GWGUI.MediaEngine.Formats.Floppy.CpcDsk;

/// <summary>Identifie la disposition standard ou étendue d'un conteneur CPCEMU DSK.</summary>
public enum CpcDskContainerKind
{
    /// <summary>Toutes les pistes utilisent la même taille et chaque secteur sa taille nominale.</summary>
    Standard,

    /// <summary>Les pistes et les données sectorielles peuvent utiliser des tailles distinctes.</summary>
    Extended
}
