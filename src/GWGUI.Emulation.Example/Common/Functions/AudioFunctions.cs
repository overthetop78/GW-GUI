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
    /// <example>
    /// <code>
    /// var stereo = AudioFunctions.Processing.SingleFrame(left, right);
    /// var batch = AudioFunctions.Processing.CopyBatch(data, frameCount);
    /// </code>
    /// SingleFrame produit deux échantillons signés sur seize bits : gauche, puis droite.
    /// CopyBatch reçoit le pointeur emprunté du callback et un nombre de trames stéréo,
    /// jamais une taille en octets. Les deux opérations retournent un tableau géré stable
    /// que la session peut conserver après le callback ; le pointeur reste possédé par son producteur.
    /// Les autres dispositions utilisent les surcharges du même contrat Processing.
    /// </example>
    internal static IEmulationAudioProcessing Processing => EmulationSdk.AudioProcessing;

    /// <summary>Crée le tampon commun possédé par une session de cœur.</summary>
    /// <returns>Interface du tampon existant ; le propriétaire vide ses blocs lors de l’arrêt et de la destruction.</returns>
    internal static IEmulationAudioBuffer CreateBuffer() => EmulationSdk.CreateAudioBuffer();
}
