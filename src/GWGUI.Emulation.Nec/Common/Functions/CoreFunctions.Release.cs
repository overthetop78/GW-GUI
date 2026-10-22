using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation;
namespace GWGUI.Emulation.Nec.Common.Functions;

/// <summary>Fournit le contrat commun de sélection et installation des versions de cœur.</summary>
internal static class CoreReleaseFunctions
{
    /// <summary>Fournit le contrat commun de sélection et installation des versions de cœur.</summary>
    /// <value>Service emprunté du SDK, disponible pour tout module par son interface publique.</value>
    /// <remarks>Le client HTTP, les chemins, la progression et l’annulation sont empruntés pendant les opérations. Le service ferme ses réponses et flux ; Common conserve les adaptations et erreurs du module. Cet accès ne lance aucun téléchargement et ne possède aucun cœur.</remarks>
    internal static IEmulationCoreReleases Releases => EmulationSdk.CoreReleases;

}
