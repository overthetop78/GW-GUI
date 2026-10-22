using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Philips.Common.Functions;

internal static class AudioFunctions
{
    /// <summary>Fournit les traitements audio communs par leur contrat public.</summary>
    /// <value>Service emprunté du SDK, disponible pour tout module par son interface publique.</value>
    /// <remarks>Le service emprunte les pointeurs et sorties reçus pendant chaque appel. Les tableaux produits appartiennent à la session appelante. Les surcharges conservent leurs contrôles de taille et leurs règles d’allocation ; aucun périphérique de sortie supplémentaire n’est créé par cet accès.</remarks>
    internal static IEmulationAudioProcessing Processing => EmulationSdk.AudioProcessing;


    internal static short[] SingleFrame(short left, short right) => AudioFunctions.Processing.SingleFrame(left, right);
}
