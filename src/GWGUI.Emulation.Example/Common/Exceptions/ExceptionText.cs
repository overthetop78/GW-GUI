using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Example.Common.Functions.CoreHostFunctions;
using System.Globalization;


namespace GWGUI.Emulation.Example.Common.Exceptions;

/// <summary>Accès aux textes de diagnostic dans les ressources de localisation du module.</summary>
/// <remarks>Le catalogue reste associé à cet assembly. La culture est lue au moment de chaque demande,
/// ce qui permet de suivre la langue d’interface sans conserver un message traduit dans un champ statique.</remarks>
internal static class ExceptionText
{
    /// <summary>Catalogue partagé par les demandes de diagnostic de cet assembly.</summary>
    /// <remarks>Le contrat SDK conserve son cache et ferme les flux ouverts lors du chargement des ressources.
    /// Il recherche la culture d'interface, ses parents, l'anglais puis la base commune ; la culture invariante
    /// consulte directement cette base. Le module n'a aucun gestionnaire de ressources concret à libérer ici.</remarks>
    private static readonly IEmulationModuleLocalization Localization = CommonCoreHostFunctions.CreateLocalization(typeof(ExceptionText).Assembly, "GWGUI.Emulation.Example.Resources.Emulation");

    /// <summary>Résout une clé dans la culture d’interface courante.</summary>
    /// <param name="resourceKey">Clé technique exacte à rechercher dans les ressources.</param>
    /// <param name="arguments">Valeurs à substituer ; sans argument, le texte reste intact même avec des accolades.</param>
    /// <returns>Texte traduit ou clé d’origine, formaté avec la culture courante lorsque des arguments sont fournis.</returns>
    /// <remarks>CurrentUICulture sélectionne le texte ; CurrentCulture formate les nombres et les dates des arguments.
    /// Sans argument, les accolades du texte sont conservées. Avec des arguments, les indices et formats sont ceux
    /// du catalogue : une erreur de format signale un modèle incohérent au lieu de masquer le diagnostic.</remarks>
    internal static string Get(string resourceKey, params object[] arguments)
    {
        var value = Localization.TryGetString(resourceKey, CultureInfo.CurrentUICulture, out var text) ? text : resourceKey;
        return arguments.Length == 0 ? value : string.Format(CultureInfo.CurrentCulture, value, arguments);
    }
}

