namespace GWGUI.Emulation.Example.Common.Constants;

/// <summary>Organisation du jeu de tampons vidéo et indices utilisés pour parcourir les images.</summary>
/// <remarks>Le double tampon sépare la préparation d’une image de la consultation de l’image publiée.
/// Les fonctions de vidéo calculent le prochain indice avec un incrément puis un modulo du nombre de tampons.
/// Les indices de tampon et de ligne commencent à zéro.</remarks>
internal static class VideoConstants
{
    /// <summary>Nombre de tampons composant le jeu vidéo : deux.</summary>
    internal const int BufferCount = 2;
    /// <summary>Indice du tampon initial dans le jeu vidéo.</summary>
    internal const int FirstBuffer = 0;
    /// <summary>Indice de la première ligne de pixels lors de la copie d’une image.</summary>
    internal const int FirstRow = 0;
    /// <summary>Pas ajouté à l’indice avant réduction modulo BufferCount.</summary>
    internal const int NextBufferStep = 1;
    /// <summary>Cadence vidéo de base, en images par seconde, avant l’annonce d’une valeur par le cœur.</summary>
    internal const double DefaultFramesPerSecond = 50;
    /// <summary>Cadence minimale de fonctionnement, en images par seconde.</summary>
    /// <remarks>Cette borne sert au réglage du rythme de la boucle ; le seuil de validité vérifie séparément la valeur annoncée par le cœur.</remarks>
    internal const double MinimumFramesPerSecond = 1;
    /// <summary>Cadence maximale de fonctionnement, en images par seconde.</summary>
    /// <remarks>Cette borne limite le rythme demandé à la boucle et ne décrit pas la fréquence du moniteur physique.</remarks>
    internal const double MaximumFramesPerSecond = 1000;
    /// <summary>Nombre d’images d’une opération portant sur une seule image.</summary>
    internal const int SingleFrameCount = 1;
    /// <summary>Séquence initiale du compteur d’images, avant la première image publiée.</summary>
    internal const long InitialFrameSequence = 0L;
    /// <summary>Seuil de validité de la cadence : une cadence exploitable est strictement supérieure à zéro.</summary>
    /// <remarks>Les bornes MinimumFramesPerSecond et MaximumFramesPerSecond limitent séparément le rythme de la boucle.</remarks>
    internal const double FrameRateValidityThreshold = 0d;
    /// <summary>Rapport d’aspect nul indiquant que le cœur n’a pas fourni de rapport explicite.</summary>
    internal const float UnspecifiedAspectRatio = 0f;
}

