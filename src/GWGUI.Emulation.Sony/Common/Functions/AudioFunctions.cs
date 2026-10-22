using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation;
namespace GWGUI.Emulation.Sony.Common.Functions;

/// <summary>Fournit les traitements audio communs par leur contrat public.</summary>
internal static class AudioFunctions
{
    /// <summary>Crée le tampon audio commun possédé par la session du cœur.</summary>
    /// <returns>Interface publique du tampon, sans dépendance à sa classe d’implémentation.</returns>
    /// <remarks>Le producteur conserve ses blocs stables et vide le tampon avec ClearNative lors de son arrêt,
    /// en détachant aussi son dernier bloc. Le service ne prend possession d’aucune sortie audio empruntée.</remarks>
    internal static IEmulationAudioBuffer CreateBuffer() => EmulationSdk.CreateAudioBuffer();


    /// <summary>Fournit les traitements audio communs par leur contrat public.</summary>
    /// <value>Service emprunté du SDK, disponible pour tout module par son interface publique.</value>
    /// <remarks>Le service emprunte les pointeurs et sorties reçus pendant chaque appel. Les tableaux produits appartiennent à la session appelante. Les surcharges conservent leurs contrôles de taille et leurs règles d’allocation ; aucun périphérique de sortie supplémentaire n’est créé par cet accès.</remarks>
    internal static IEmulationAudioProcessing Processing => EmulationSdk.AudioProcessing;

}
