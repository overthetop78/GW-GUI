namespace GWGUI.MediaEngine.Exploration.Metadata;

/// <summary>Décrit les caractéristiques de contenu prouvées par les octets d'une image.</summary>
public sealed record DiskContentMetadata(bool HasValidAmigaBootLoader, string? ModificationId, IReadOnlyList<string> CompressionIds, string? OrganizationId = null, int OrganizationMemberCount = 0)
{
    /// <summary>Indique qu'au moins une caractéristique nommée a été identifiée.</summary>
    public bool HasIdentifiedCharacteristics => ModificationId is not null || CompressionIds.Count > 0 || OrganizationId is not null;

    /// <summary>Indique qu'une organisation logique sans catalogue de fichiers a été reconnue.</summary>
    public bool HasCataloglessOrganization => OrganizationId is not null || HasValidAmigaBootLoader && HasIdentifiedCharacteristics;
}
