namespace GWGUI.Emulation.Example.Common.Constants;

/// <summary>Indices de canaux, tailles de lots et bornes des valeurs audio utilisées par Common.</summary>
/// <remarks>Une frame stéréo contient deux échantillons entrelacés, gauche puis droit.
/// Les nombres de frames comptent des instants audio ; les indices d’échantillons désignent les positions
/// dans les tableaux. Les gains sont normalisés entre zéro et un, indépendamment des valeurs PCM signées.</remarks>
internal static class AudioConstants
{
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
    internal const float DefaultVolume = MaximumVolume;
    /// <summary>Indice du premier échantillon dans un tableau audio.</summary>
    internal const int FirstSampleIndex = 0;
    /// <summary>Valeur PCM signée sur seize bits la plus basse : −32 768.</summary>
    /// <remarks>short est le type C# System.Int16 ; MinValue est la borne constante fournie par .NET.</remarks>
    internal const int MinimumSampleValue = short.MinValue;
    /// <summary>Valeur PCM signée sur seize bits la plus haute : 32 767.</summary>
    /// <remarks>Cette borne sert à conserver un résultat représentable par un échantillon System.Int16.</remarks>
    internal const int MaximumSampleValue = short.MaxValue;
}

