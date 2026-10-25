namespace GWGUI.Emulation.Interfaces;

/// <summary>Document CUE opaque produit par les traitements du SDK, sans dépendance média dans le module.</summary>
/// <remarks>Le document ne possède aucun flux. Les pistes et métadonnées restent dans leur propriétaire média ;
/// le module utilise ses chemins pour préparer les copies de sa session.</remarks>
public interface IEmulationCueSheet
{
    /// <summary>Chemins des fichiers déclarés, dans leur ordre et relativement au CUE source.</summary>
    IReadOnlyList<string> DeclaredFiles { get; }
}
