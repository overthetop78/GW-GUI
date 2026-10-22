using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation;
namespace GWGUI.Emulation.Nintendo.Common.Functions;

/// <summary>Accès commun au contrat de transport et de durée de vie des hôtes de cœur.</summary>
internal static class CoreHostFunctions
{
    /// <summary>Fournit le contrat commun de transport, de statut et de durée de vie des hôtes.</summary>
    /// <value>Service emprunté du SDK, indépendant du nom du module et de son adaptateur.</value>
    /// <remarks>Les adaptateurs passent par cet accès Common pour appeler les codecs et les opérations de session.
    /// Le service expose uniquement son interface publique ; il conserve les implémentations et limites du protocole
    /// dans Emulation. Les flux, vues, processus et cœurs restent possédés par la session appelante, qui les ferme
    /// dans son finally et attend l’arrêt effectif du cœur et de son hôte. Cet accès ne crée aucune session,
    /// ne mémorise aucune ressource reçue et ne requiert aucun enregistrement du nom de module dans le SDK.
    /// Les codecs propres au module conservent leur configuration et leurs diagnostics.</remarks>
    internal static IEmulationHostProtocol Protocol => EmulationSdk.HostProtocol;

}
