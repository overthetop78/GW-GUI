namespace GWGUI.Emulation.Example.Common.Constants;

/// <summary>Indices de canaux, tailles de lots et bornes des valeurs audio utilisées par Common.</summary>
/// <remarks>Une frame stéréo contient deux échantillons entrelacés, gauche puis droit.
/// Les nombres de frames comptent des instants audio ; les indices d’échantillons désignent les positions
/// dans les tableaux. Les gains sont normalisés entre zéro et un, indépendamment des valeurs PCM signées.</remarks>
internal static class AudioConstants
{
    /// <summary>Séparation stéréo complète en pourcentage, conservée lorsque ce réglage est applicable au cœur.</summary>
    internal const int FullStereoSeparation = 100;

    /// <summary>Nombre de canaux du format PCM stéréo entrelacé reçu par les callbacks natifs : deux.</summary>
    /// <remarks>Cette valeur décrit ce format précis ; elle ne limite pas les sorties multicanales décrites par leur layout.</remarks>
    internal const int StereoChannelCount = 2;
    /// <summary>Diviseur de la fréquence d’échantillonnage pour le profil de tampon couvrant un cinquième de seconde.</summary>
    /// <remarks>La capacité correspondante est max(MinimumBufferedFrameCount, fréquence / diviseur).
    /// Ce profil de capacité reste distinct de la latence de sortie demandée au périphérique audio.</remarks>
    internal const int BufferDurationDivisor = 5;
    /// <summary>Capacité minimale du profil de tampon natif, exprimée en frames audio.</summary>
    internal const int MinimumBufferedFrameCount = 1;
    /// <summary>Position du canal gauche dans chaque paire d’échantillons stéréo entrelacés.</summary>
    internal const int LeftChannelIndex = 0;
    /// <summary>Position du canal droit dans chaque paire d’échantillons stéréo entrelacés.</summary>
    internal const int RightChannelIndex = 1;
    /// <summary>Nombre de frames correspondant à la réception d’une seule paire stéréo.</summary>
    internal const int SingleFrameCount = 1;
    /// <summary>Nombre maximal de frames accepté pour un lot audio natif : 65 536.</summary>
    /// <remarks>Cette borne concerne la taille d’un lot, pas la fréquence d’échantillonnage ni la durée totale du son.</remarks>
    internal const int MaximumFramesPerBatch = 64 * 1024;
    /// <summary>Gain normalisé minimal : les échantillons deviennent silencieux.</summary>
    internal const float MinimumVolume = 0f;
    /// <summary>Gain normalisé maximal : l’amplitude originale des échantillons est conservée.</summary>
    internal const float MaximumVolume = 1f;
    /// <summary>Gain initial avant application du volume configuré pour la machine.</summary>
    /// <remarks>Le facteur est déduit du choix initial en pourcentage ; changer ce choix conserve la cohérence des deux unités.</remarks>
    internal const float DefaultVolume = DefaultVolumePercent / 100f;
    /// <summary>Volume initial choisi par le module, en pourcentage de l’amplitude : cent pour cent.</summary>
    internal const int DefaultVolumePercent = 100;
    /// <summary>Indice du premier échantillon dans un tableau audio.</summary>
    internal const int FirstSampleIndex = 0;
    /// <summary>Valeur PCM signée sur seize bits la plus basse : −32 768.</summary>
    /// <remarks>short est le type C# System.Int16 ; MinValue est la borne constante fournie par .NET.</remarks>
    internal const int MinimumSampleValue = short.MinValue;
    /// <summary>Valeur PCM signée sur seize bits la plus haute : 32 767.</summary>
    /// <remarks>Cette borne sert à conserver un résultat représentable par un échantillon System.Int16.</remarks>
    internal const int MaximumSampleValue = short.MaxValue;
    /// <summary>Fréquence audio de base, en hertz, utilisée avant que le cœur annonce sa fréquence : 44 100 Hz.</summary>
    /// <remarks>Cette valeur de repli appartient au module ; la fréquence réellement annoncée par le cœur la remplace ensuite.</remarks>
    internal const int DefaultSampleRate = 44100;
    /// <summary>Valeur initiale d’un suivi de fréquence, indiquant qu’aucune fréquence n’a encore été appliquée à la sortie.</summary>
    /// <remarks>Zéro sert de marqueur de suivi ; il ne désigne pas une fréquence audio à utiliser pour démarrer la sortie.</remarks>
    internal const int UninitializedSampleRate = 0;
    /// <summary>Latence initiale choisie par ce module, en millisecondes.</summary>
    /// <remarks>Cette constante permet de définir le choix local des configurations audio ; elle ne relaie pas une constante d’implémentation du SDK.</remarks>
    internal const int DefaultLatencyMilliseconds = 50;
    /// <summary>Choix de latence proposés par le module, en millisecondes, sous un nom unique.</summary>
    /// <remarks>Les deux listes équivalentes relevées sont réunies ici. Cette sélection inclut 35 et 250 ms ;
    /// elle exprime les choix du module, distincts de la liste générale de réglages. La vue exposée protège le tableau des modifications.</remarks>
    internal static IReadOnlyList<int> LatencyChoicesMilliseconds { get; } = Array.AsReadOnly<int>([20, 35, 50, 75, 100, 150, 250]);
}

