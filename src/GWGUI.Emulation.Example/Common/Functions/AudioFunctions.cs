using GWGUI.Emulation;

namespace GWGUI.Emulation.Example.Common.Functions;

/// <summary>Adaptation des échantillons natifs vers les données audio communes.</summary>
/// <remarks>Le traitement est fourni par l’interface AudioProcessing du SDK ; Common constitue le relais
/// utilisé par les adaptateurs, sans recopier le calcul ni gérer de périphérique de sortie.</remarks>
internal static class AudioFunctions
{
    /// <summary>Crée le tampon commun possédé par une session de cœur.</summary>
    /// <returns>Interface du tampon existant ; le propriétaire vide ses blocs lors de l’arrêt et de la destruction.</returns>
    internal static IEmulationAudioBuffer CreateBuffer() => EmulationSdk.CreateAudioBuffer();

    /// <summary>Copie les trames stéréo dont les pointeurs sont empruntés pendant un callback natif.</summary>
    /// <param name="data">Adresse des échantillons PCM entrelacés sur seize bits.</param>
    /// <param name="frameCount">Nombre de trames comprenant chacune gauche puis droite.</param>
    /// <returns>Nouveau tableau géré contenant la copie validée par le traitement commun.</returns>
    internal static short[] CopyBatch(nint data, nuint frameCount) => EmulationSdk.AudioProcessing.CopyBatch(data, frameCount);

    /// <summary>Construit une frame PCM stéréo entrelacée à partir de ses deux échantillons.</summary>
    /// <param name="left">Échantillon signé sur seize bits du canal gauche.</param>
    /// <param name="right">Échantillon signé sur seize bits du canal droit.</param>
    /// <returns>Nouveau tableau contenant gauche puis droite ; son propriétaire est l’appelant.</returns>
    internal static short[] SingleFrame(short left, short right) => EmulationSdk.AudioProcessing.SingleFrame(left, right);
}
