namespace GWGUI.Emulation.Example.Common.Constants;

/// <summary>Limites de présentation des noms de firmware dans le résumé compact d’une machine.</summary>
/// <remarks>La troncature concerne le texte affiché ; elle ne change ni le chemin conservé ni l’identité du firmware.</remarks>
internal static class ConfigurationSummaryFunctionsConstants
{
    /// <summary>Caractère ajouté à la fin d’un nom dont une partie a été retirée.</summary>
    internal const string Ellipsis = "…";
    /// <summary>Nombre maximal de caractères conservés pour un nom présenté sans troncature.</summary>
    internal const int MaximumShortNameLength = 15;
    /// <summary>Nombre de caractères conservés avant les points de suspension lors d’une troncature.</summary>
    internal const int TruncatedShortNameLength = 14;
}
