using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Example.Common.Functions;

/// <summary>Adaptation des échantillons natifs vers les données audio communes.</summary>
/// <remarks>Le traitement est fourni par l’interface AudioProcessing du SDK ; Common constitue le relais
/// utilisé par les adaptateurs, sans recopier le calcul ni gérer de périphérique de sortie.</remarks>
internal static class AudioFunctions
{
    /// <summary>Fournit les traitements audio communs par leur contrat public.</summary>
    /// <value>Service emprunté du SDK, disponible pour tout module par son interface publique.</value>
    /// <remarks>Le service emprunte les pointeurs et sorties reçus pendant chaque appel. Les tableaux produits appartiennent à la session appelante. Les surcharges conservent leurs contrôles de taille et leurs règles d’allocation ; aucun périphérique de sortie supplémentaire n’est créé par cet accès.</remarks>
    internal static IEmulationAudioProcessing Processing => EmulationSdk.AudioProcessing;

    /// <summary>Crée le tampon commun possédé par une session de cœur.</summary>
    /// <returns>Interface du tampon existant ; le propriétaire vide ses blocs lors de l’arrêt et de la destruction.</returns>
    internal static IEmulationAudioBuffer CreateBuffer() => EmulationSdk.CreateAudioBuffer();

    /// <summary>Copie les trames stéréo dont les pointeurs sont empruntés pendant un callback natif.</summary>
    /// <param name="data">Adresse des échantillons PCM entrelacés sur seize bits.</param>
    /// <param name="frameCount">Nombre de trames comprenant chacune gauche puis droite.</param>
    /// <returns>Nouveau tableau géré contenant la copie validée par le traitement commun.</returns>
    internal static short[] CopyBatch(nint data, nuint frameCount) => AudioFunctions.Processing.CopyBatch(data, frameCount);

    /// <summary>Construit une frame PCM stéréo entrelacée à partir de ses deux échantillons.</summary>
    /// <param name="left">Échantillon signé sur seize bits du canal gauche.</param>
    /// <param name="right">Échantillon signé sur seize bits du canal droit.</param>
    /// <returns>Nouveau tableau contenant gauche puis droite ; son propriétaire est l’appelant.</returns>
    internal static short[] SingleFrame(short left, short right) => AudioFunctions.Processing.SingleFrame(left, right);
}
