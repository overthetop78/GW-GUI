namespace GWGUI.MediaEngine.Containers.Cp2;

/// <summary>Décrit les positions et tailles binaires du conteneur CP2.</summary>
internal static class Cp2Layout
{
    /// <summary>Longueur minimale d'un conteneur CP2, en octets.</summary>
    public const int MinimumFileLength = 34;
    /// <summary>Position de la signature CP2, en octets depuis le début du fichier.</summary>
    public const int SignatureOffset = 0;
    /// <summary>Position du premier groupe CP2, en octets depuis le début du fichier.</summary>
    public const int FirstGroupOffset = 28;
    /// <summary>Longueur de l'en-tête d'un groupe, en octets.</summary>
    public const int GroupHeaderSize = 4;
    /// <summary>Position relative du champ contenant la longueur des métadonnées.</summary>
    public const int MetadataLengthOffset = 2;
    /// <summary>Longueur d'un champ de longueur, en octets.</summary>
    public const int LengthFieldSize = 2;
    /// <summary>Nombre d'octets encadrant les métadonnées et les charges utiles.</summary>
    public const int FramingSize = 2;
    /// <summary>Ajustement retiré à la longueur déclarée avant de compter les descripteurs.</summary>
    public const int MetadataLengthAdjustment = 1;
    /// <summary>Longueur d'un descripteur de piste, en octets.</summary>
    public const int TrackDescriptorSize = 387;
    /// <summary>Longueur d'un descripteur sectoriel, en octets.</summary>
    public const int SectorDescriptorSize = 16;
    /// <summary>Longueur de l'en-tête d'un descripteur de piste, en octets.</summary>
    public const int TrackHeaderSize = 7;
    /// <summary>Position relative du numéro de cylindre de la piste.</summary>
    public const int TrackCylinderOffset = 0;
    /// <summary>Position relative du numéro de face de la piste.</summary>
    public const int TrackHeadOffset = 1;
    /// <summary>Position relative du nombre de secteurs de la piste.</summary>
    public const int TrackSectorCountOffset = 2;
    /// <summary>Nombre maximal de descripteurs sectoriels dans une piste.</summary>
    public const int MaximumSectorDescriptorCount = 23;
    /// <summary>Position relative du numéro de cylindre dans un descripteur sectoriel.</summary>
    public const int SectorCylinderOffset = 0;
    /// <summary>Position relative du numéro de face dans un descripteur sectoriel.</summary>
    public const int SectorHeadOffset = 1;
    /// <summary>Position relative du numéro logique dans un descripteur sectoriel.</summary>
    public const int SectorNumberOffset = 2;
    /// <summary>Position relative du code de taille dans un descripteur sectoriel.</summary>
    public const int SectorSizeCodeOffset = 3;
    /// <summary>Position relative de la position angulaire dans un descripteur sectoriel.</summary>
    public const int SectorPositionOffset = 5;
    /// <summary>Longueur du champ de position angulaire, en octets.</summary>
    public const int SectorPositionLength = 2;
    /// <summary>Taille sectorielle correspondant au code de taille zéro, en octets.</summary>
    public const int BaseSectorSize = 128;
    /// <summary>Code de taille sectorielle maximal accepté.</summary>
    public const int MaximumSectorSizeCode = 7;
    /// <summary>Taille des secteurs actuellement reconstruits, en octets.</summary>
    public const int ReconstructedSectorSize = 512;
}
