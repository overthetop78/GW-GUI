using System.Buffers;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation;
namespace GWGUI.Emulation.Nec.Common.Functions;

/// <summary>Accès commun au contrat de transport et de durée de vie des hôtes de cœur.</summary>
internal static class CoreHostFunctions
{
    /// <summary>Crée le jeu tournant de tableaux vidéo de la session par le contrat public du SDK.</summary>
    /// <param name="bufferCount">Nombre d’emplacements choisi par le module ; chaque emplacement peut retenir un tableau loué.</param>
    /// <param name="pool">Pool emprunté, ou null pour le pool partagé ; le propriétaire du jeu ne détruit pas ce pool.</param>
    /// <returns>Jeu possédé par l’appelant, à libérer après la dernière utilisation des images dans son finally.</returns>
    internal static IEmulationVideoBufferSet CreateVideoBufferSet(int bufferCount, ArrayPool<byte>? pool = null) => EmulationSdk.CreateVideoBufferSet(bufferCount, pool);

    /// <summary>Crée le propriétaire des segments vidéo partagés par le contrat public du SDK.</summary>
    /// <param name="namePrefix">Préfixe de session transmis au lecteur distant.</param>
    /// <param name="capacity">Politique existante du module, qui calcule la capacité en octets d’un emplacement.</param>
    /// <param name="separator">Séparateur entre le préfixe de session et le nom unique de génération.</param>
    /// <param name="nameFormat">Format de l’identifiant unique, conservé tel quel par la fabrique.</param>
    /// <returns>Rédacteur possédé par l’appelant ; il libère ses vues et segments lors de Dispose.</returns>
    /// <remarks>La session appelle Dispose dans son finally après la dernière écriture. Les vues exposées par
    /// l’interface restent empruntées aux codecs : ceux-ci ne les ferment pas et ne les conservent pas après remplacement.</remarks>
    internal static IEmulationSharedVideoWriter CreateSharedVideoWriter(string namePrefix, Func<int, int> capacity, string separator, string nameFormat)
        => EmulationSdk.CreateSharedVideoWriter(namePrefix, capacity, separator, nameFormat);


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
