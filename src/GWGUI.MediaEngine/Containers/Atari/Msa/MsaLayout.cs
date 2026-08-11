namespace GWGUI.MediaEngine.Containers.Atari.Msa;

/// <summary>Décrit les positions, tailles et limites du conteneur MSA.</summary>
internal static class MsaLayout
{
    /// <summary>Longueur de l'en-tête, en octets.</summary>
    public const int HeaderSize = 10;
    /// <summary>Position de la signature dans l'en-tête, en octets.</summary>
    public const int SignatureOffset = 0;
    /// <summary>Position du nombre de secteurs par piste, en octets.</summary>
    public const int SectorsPerTrackOffset = 2;
    /// <summary>Position du nombre de faces diminué d'une unité, en octets.</summary>
    public const int HeadsOffset = 4;
    /// <summary>Position du premier cylindre, en octets.</summary>
    public const int StartCylinderOffset = 6;
    /// <summary>Position du dernier cylindre, en octets.</summary>
    public const int EndCylinderOffset = 8;
    /// <summary>Longueur du champ donnant la taille d'une piste, en octets.</summary>
    public const int TrackLengthFieldSize = 2;
    /// <summary>Taille d'un secteur Atari ST, en octets.</summary>
    public const int SectorSize = 512;
    /// <summary>Nombre minimal de secteurs accepté par piste.</summary>
    public const int MinimumSectorsPerTrack = 1;
    /// <summary>Nombre maximal de secteurs accepté par piste.</summary>
    public const int MaximumSectorsPerTrack = 36;
    /// <summary>Nombre minimal de faces accepté.</summary>
    public const int MinimumHeadCount = DiskGeometryConstants.SingleSidedHeadCount;
    /// <summary>Nombre maximal de faces accepté.</summary>
    public const int MaximumHeadCount = DiskGeometryConstants.DoubleSidedHeadCount;
    /// <summary>Indice maximal de cylindre accepté.</summary>
    public const int MaximumCylinder = 255;
    /// <summary>Longueur d'une séquence RLE, en octets.</summary>
    public const int RleSequenceSize = 4;
    /// <summary>Position de l'octet répété dans une séquence RLE.</summary>
    public const int RleValueOffset = 1;
    /// <summary>Position du compteur big-endian dans une séquence RLE.</summary>
    public const int RleCountOffset = 2;
}
