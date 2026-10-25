namespace GWGUI.Emulation.Example.Common.Constants;

/// <summary>Choix du module pour présenter les versions de ses cœurs.</summary>
/// <remarks>Ces valeurs appartiennent au module. Le service Common les utilise pour décrire sa publication
/// et le runtime annonce la même version : les réglages de présentation restent définis à un seul endroit.</remarks>
internal static class CoreReleaseConstants
{
    /// <summary>Nombre de composantes conservées par Version.ToString : majeure, mineure et numéro de build.</summary>
    /// <remarks>La valeur 3 présente les trois composantes utilisées par la publication intégrée.
    /// Le numéro provient de l'assembly du cœur ; cette constante choisit uniquement sa présentation.</remarks>
    internal const int VersionComponentCount = 3;
}
