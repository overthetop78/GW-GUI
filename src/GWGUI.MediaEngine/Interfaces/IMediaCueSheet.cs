namespace GWGUI.MediaEngine.Interfaces;

/// <summary>Document CUE produit par la préparation MediaEngine, conservant sa structure et ses métadonnées.</summary>
/// <remarks>Le document ne possède aucun fichier ouvert. Les opérations de préparation utilisent le document
/// retourné par leur lecteur ; ses pistes et métadonnées restent encapsulées dans MediaEngine.</remarks>
public interface IMediaCueSheet
{
    /// <summary>Chemins déclarés dans les commandes FILE, dans leur ordre et sans résolution relative.</summary>
    IReadOnlyList<string> DeclaredFiles { get; }
}
