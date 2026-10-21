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
}

