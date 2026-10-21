using System.Globalization;


namespace GWGUI.Emulation.Example.Common.Exceptions;

/// <summary>Accès aux textes de diagnostic dans les ressources de localisation du module.</summary>
/// <remarks>Le catalogue reste associé à cet assembly. La culture est lue au moment de chaque demande,
/// ce qui permet de suivre la langue d’interface sans conserver un message traduit dans un champ statique.</remarks>
internal static class ExceptionText
{
    // Catalogue partagé par les demandes de ce propriétaire ; il applique les règles de repli du service commun.
    private static readonly EmulationModuleLocalization Localization = new(typeof(ExceptionText).Assembly, "GWGUI.Emulation.Example.Resources.Emulation");

    /// <summary>Résout une clé dans la culture d’interface courante.</summary>
    /// <param name="resourceKey">Clé technique exacte à rechercher dans les ressources.</param>
    /// <returns>Texte traduit ou clé d’origine lorsqu’aucune traduction n’est disponible.</returns>
    internal static string Get(string resourceKey) => Localization.TryGetString(resourceKey, CultureInfo.CurrentUICulture, out var value) ? value : resourceKey;
}

