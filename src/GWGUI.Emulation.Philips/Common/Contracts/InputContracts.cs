using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Philips.Common.Contracts;

/// <summary>Action configurable décrite par un adaptateur, avec sa touche émulée éventuelle.</summary>
/// <param name="Definition">Description commune de l’action : identité, libellé et association physique initiale.</param>
/// <param name="Key">Touche émulée à produire pour cette action, ou null lorsqu’elle ne représente pas une touche.</param>
/// <remarks>Le contrat SDK conserve les informations présentées dans l’éditeur d’entrées. La touche
/// facultative permet aussi de décrire les contrôleurs dont une commande agit sur le clavier émulé.
/// Ce record ne possède aucun périphérique ; les données reçues restent stables pendant leur consultation.</remarks>
internal sealed record InputActionDefinition(InputBindingDefinition Definition, EmulationKey? Key = null)
{
    /// <summary>Identifiant de l’action porté par sa définition, sans identité locale concurrente.</summary>
    internal string Id => Definition.Id;

    /// <summary>Expression physique initiale portée par la définition, utilisée en l’absence d’association explicite.</summary>
    internal string DefaultBinding => Definition.DefaultBinding;
}
