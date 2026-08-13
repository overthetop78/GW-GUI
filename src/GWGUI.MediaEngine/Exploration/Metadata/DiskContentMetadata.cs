namespace GWGUI.MediaEngine.Exploration.Metadata;

/// <summary>Décrit les caractéristiques de contenu prouvées par les octets d'une image.</summary>
public sealed record DiskContentMetadata(bool HasValidAmigaBootLoader, string? ModificationId, IReadOnlyList<string> CompressionIds)
{
    /// <summary>Indique qu'au moins une caractéristique nommée a été identifiée.</summary>
    public bool HasIdentifiedCharacteristics => ModificationId is not null || CompressionIds.Count > 0;
}
