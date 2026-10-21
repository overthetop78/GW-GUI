namespace GWGUI.Emulation.Example.Common.Constants;

/// <summary>Indices et compteurs utilisés pour parcourir les tampons et les collections.</summary>
/// <remarks>Un indice désigne une position ; un compteur désigne un nombre d’éléments.
/// Leurs noms distincts rendent explicite le rôle d’une valeur, même lorsque plusieurs valent zéro.</remarks>
internal static class BufferConstants
{
    /// <summary>Indice du premier tampon dans une collection de tampons.</summary>
    internal const int FirstBufferIndex = 0;
    /// <summary>Indice du premier élément d’une collection indexée à partir de zéro.</summary>
    internal const int FirstCollectionIndex = 0;
    /// <summary>Nombre d’éléments d’une collection vide.</summary>
    internal const int EmptyCollectionCount = 0;
    /// <summary>Pas unitaire pour avancer à l’indice suivant pendant un parcours.</summary>
    internal const int IndexIncrement = 1;
    /// <summary>Position du début d’un flux, en octets, lors d’un repositionnement.</summary>
    internal const int FirstStreamPosition = 0;
    /// <summary>Nombre d’éléments d’une opération portant sur un seul objet.</summary>
    internal const int SingleItemCount = 1;
    /// <summary>Nombre d’octets renvoyé par une lecture lorsque la fin du flux est atteinte.</summary>
    internal const int EndOfStreamByteCount = 0;
    /// <summary>Taille en octets du tampon utilisé par les transferts de fichiers : 81 920 octets.</summary>
    /// <remarks>Cette taille décrit une copie de flux ; elle est indépendante des capacités des tampons audio ou vidéo.</remarks>
    internal const int FileTransferBufferSize = 81920;
    /// <summary>Longueur en octets d’un flux vide, typée sur 64 bits pour les propriétés de longueur des flux.</summary>
    internal const long EmptyStreamLength = 0;
}

