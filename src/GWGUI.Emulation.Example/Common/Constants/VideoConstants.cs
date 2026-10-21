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
}

