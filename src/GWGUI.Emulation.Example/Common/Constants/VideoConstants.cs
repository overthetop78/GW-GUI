namespace GWGUI.Emulation.Example.Common.Constants;

/// <summary>Nombre de tampons vidéo et bornes des fréquences proposées par les réglages.</summary>
/// <remarks>Le nombre de tampons compte les images conservées pour séparer leur préparation de leur lecture.
/// Les fréquences sont exprimées en images par seconde. Les caractéristiques courantes du cœur sont reçues
/// par les interfaces vidéo, indépendamment des valeurs proposées par les réglages du module.</remarks>
internal static class VideoConstants
{
    /// <summary>Nombre de tampons composant le jeu vidéo : deux.</summary>
    internal const int BufferCount = 2;
    /// <summary>Cadence minimale de fonctionnement, en images par seconde.</summary>
    /// <remarks>Cette borne sert au réglage du rythme de la boucle ; le seuil de validité vérifie séparément la valeur annoncée par le cœur.</remarks>
    internal const double MinimumFramesPerSecond = 1;
    /// <summary>Cadence maximale de fonctionnement, en images par seconde.</summary>
    /// <remarks>Cette borne limite le rythme demandé à la boucle et ne décrit pas la fréquence du moniteur physique.</remarks>
    internal const double MaximumFramesPerSecond = 1000;
}

