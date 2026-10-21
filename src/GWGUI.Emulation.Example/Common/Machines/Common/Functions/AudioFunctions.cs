using GWGUI.Emulation;

namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;

/// <summary>Adaptation Common des données audio élémentaires vers les traitements partagés du SDK.</summary>
internal static class AudioFunctions
{

    /// <summary>Construit une frame stéréo entrelacée à partir d’une paire d’échantillons PCM signés sur seize bits.</summary>
    /// <param name="left">Échantillon du canal gauche.</param>
    /// <param name="right">Échantillon du canal droit.</param>
    /// <returns>Nouveau tableau de deux échantillons, gauche puis droit, possédé par l’appelant.</returns>
    internal static short[] SingleFrame(short left, short right) => EmulationSdk.AudioProcessing.SingleFrame(left, right);
}

/// <summary>Déclaration des fonctions de sortie audio, sans membre dans cette définition.</summary>
internal static class AudioOutputFunctions
{
}
