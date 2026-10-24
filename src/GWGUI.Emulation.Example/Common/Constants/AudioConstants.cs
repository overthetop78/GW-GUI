namespace GWGUI.Emulation.Example.Common.Constants;

/// <summary>Réglages et tailles de lots audio utilisés par Common.</summary>
/// <remarks>Les fréquences sont exprimées en hertz, les latences en millisecondes et les niveaux en pourcentage.
/// Une frame stéréo contient deux échantillons entrelacés, gauche puis droit ; les tailles de lots comptent
/// ces frames. Les valeurs initiales décrivent les choix du module, transmis aux services par leurs interfaces.</remarks>
internal static class AudioConstants
{
    /// <summary>Séparation stéréo complète en pourcentage, conservée lorsque ce réglage est applicable au cœur.</summary>
    internal const int FullStereoSeparation = 100;

    /// <summary>Nombre de canaux du format PCM stéréo entrelacé reçu par les callbacks natifs : deux.</summary>
    /// <remarks>Cette valeur décrit ce format précis ; elle ne limite pas les sorties multicanales décrites par leur layout.</remarks>
    internal const int StereoChannelCount = 2;
    /// <summary>Nombre maximal de frames accepté pour un lot audio natif : 65 536.</summary>
    /// <remarks>Cette borne concerne la taille d’un lot, pas la fréquence d’échantillonnage ni la durée totale du son.</remarks>
    internal const int MaximumFramesPerBatch = 64 * 1024;
    /// <summary>Volume initial choisi par le module, en pourcentage de l’amplitude : cent pour cent.</summary>
    internal const int DefaultVolumePercent = 100;
    /// <summary>Fréquence audio de base, en hertz, utilisée avant que le cœur annonce sa fréquence : 44 100 Hz.</summary>
    /// <remarks>Cette valeur de repli appartient au module ; la fréquence réellement annoncée par le cœur la remplace ensuite.</remarks>
    internal const int DefaultSampleRate = 44100;
    /// <summary>Latence initiale choisie par ce module, en millisecondes.</summary>
    /// <remarks>Cette constante permet de définir le choix local des configurations audio ; elle ne relaie pas une constante d’implémentation du SDK.</remarks>
    internal const int DefaultLatencyMilliseconds = 50;
    /// <summary>Choix de latence proposés par le module, en millisecondes, sous un nom unique.</summary>
    /// <remarks>Les deux listes équivalentes relevées sont réunies ici. Cette sélection inclut 35 et 250 ms ;
    /// elle exprime les choix du module, distincts de la liste générale de réglages. La vue exposée protège le tableau des modifications.</remarks>
    internal static IReadOnlyList<int> LatencyChoicesMilliseconds { get; } = Array.AsReadOnly<int>([20, 35, 50, 75, 100, 150, 250]);
}

