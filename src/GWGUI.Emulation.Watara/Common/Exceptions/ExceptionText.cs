using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Watara.Common.Functions.CoreHostFunctions;
using System.Globalization;
using GWGUI.Emulation.Services;


namespace GWGUI.Emulation.Watara.Common.Exceptions;

/// <summary>Accès aux textes de diagnostic dans les ressources de localisation du module.</summary>
/// <remarks>Le catalogue reste associé à cet assembly. La culture est lue au moment de chaque demande,
/// ce qui permet de suivre la langue d’interface sans conserver un message traduit dans un champ statique.</remarks>
internal static class ExceptionText
{
    // Catalogue partagé par les demandes de ce propriétaire ; il applique les règles de repli du service commun.
    private static readonly IEmulationModuleLocalization Localization = CommonCoreHostFunctions.CreateLocalization(typeof(ExceptionText).Assembly, "GWGUI.Emulation.Watara.Resources.Emulation");

    /// <summary>Résout une clé dans la culture d’interface courante.</summary>
    /// <param name="resourceKey">Clé technique exacte à rechercher dans les ressources.</param>
    /// <param name="arguments">Valeurs à substituer ; sans argument, le texte reste intact même avec des accolades.</param>
    /// <returns>Texte traduit ou clé d’origine, formaté avec la culture courante lorsque des arguments sont fournis.</returns>
    internal static string Get(string resourceKey, params object[] arguments)
    {
        var value = Localization.TryGetString(resourceKey, CultureInfo.CurrentUICulture, out var text) ? text : resourceKey;
        return arguments.Length == 0 ? value : string.Format(CultureInfo.CurrentCulture, value, arguments);
    }
}

