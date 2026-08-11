namespace GWGUI.MediaEngine.Containers.TeleDisk;

/// <summary>Identifie une section binaire TeleDisk dans les diagnostics.</summary>
internal enum Td0Section
{
    /// <summary>En-tête global de l'image.</summary>
    ImageHeader,
    /// <summary>En-tête du commentaire facultatif.</summary>
    CommentHeader,
    /// <summary>Données du commentaire facultatif.</summary>
    Comment,
    /// <summary>En-tête d'une piste.</summary>
    TrackHeader,
    /// <summary>En-tête d'un secteur.</summary>
    SectorHeader,
    /// <summary>En-tête de la charge utile sectorielle.</summary>
    SectorDataHeader,
    /// <summary>Charge utile sectorielle.</summary>
    SectorData
}
