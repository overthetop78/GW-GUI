namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Regroupe les définitions communes du format MFM Amiga.</summary>
internal static class AmigaMfmFormat
{
    /// <summary>Identifiant technique du codec Amiga MFM.</summary>
    public const string CodecId = FluxCodecIds.AmigaMfm;
    /// <summary>Nom affiché du codec Amiga MFM.</summary>
    public const string CodecDisplayName = FluxCodecDisplayNames.AmigaMfm;
    /// <summary>Nom du format utilisé dans les descriptions de structures.</summary>
    public const string StructureDescriptionName = "Amiga";
    /// <summary>Mot de synchronisation MFM Amiga.</summary>
    public const ushort SyncWord = 0x4489;
    /// <summary>Nombre de mots de synchronisation consécutifs.</summary>
    public const int SyncWordCount = 2;
    /// <summary>Nombre de bits MFM représentant un octet encodé.</summary>
    public const int EncodedByteBitCount = MfmEncoding.EncodedByteBitCount;
    /// <summary>Longueur totale des mots de synchronisation, en bits.</summary>
    public const int SyncBitCount = SyncWordCount * EncodedByteBitCount;
    /// <summary>Valeur identifiant un en-tête de secteur Amiga.</summary>
    public const byte FormatByte = 0xff;
    /// <summary>Nombre d'octets décodés composant le champ d'information.</summary>
    public const int InfoByteCount = 4;
    /// <summary>Position de l'octet de format dans le champ d'information décodé.</summary>
    public const int FormatByteOffset = 0;
    /// <summary>Position de l'octet réunissant la piste et la face.</summary>
    public const int TrackAndHeadOffset = 1;
    /// <summary>Position du numéro de secteur.</summary>
    public const int SectorNumberOffset = 2;
    /// <summary>Position du nombre de secteurs restant sur la piste.</summary>
    public const int RemainingSectorCountOffset = 3;
    /// <summary>Décalage appliqué au numéro de piste pour obtenir le cylindre.</summary>
    public const int TrackCylinderShift = 1;
    /// <summary>Masque isolant le numéro de face dans l'octet de piste.</summary>
    public const byte TrackHeadMask = 1;
    /// <summary>Longueur du label de secteur, en octets encodés.</summary>
    public const int LabelByteCount = 16;
    /// <summary>Nombre d'octets encodés couverts par la parité d'en-tête.</summary>
    public const int HeaderParitySourceByteCount = InfoByteCount + LabelByteCount;
    /// <summary>Position de l'octet haut de parité d'en-tête.</summary>
    public const int HeaderParityHighOffset = 22;
    /// <summary>Position de l'octet bas de parité d'en-tête.</summary>
    public const int HeaderParityLowOffset = 23;
    /// <summary>Position de l'octet haut de parité des données.</summary>
    public const int DataParityHighOffset = 26;
    /// <summary>Position de l'octet bas de parité des données.</summary>
    public const int DataParityLowOffset = 27;
    /// <summary>Longueur totale des champs de parité, en octets encodés.</summary>
    public const int ParityFieldByteCount = 8;
    /// <summary>Longueur de l'en-tête complet, en octets encodés.</summary>
    public const int EncodedHeaderByteCount = HeaderParitySourceByteCount + ParityFieldByteCount;
    /// <summary>Position du début des données dans le secteur encodé.</summary>
    public const int EncodedDataOffset = EncodedHeaderByteCount;
    /// <summary>Longueur du bloc de données odd/even, en octets encodés.</summary>
    public const int EncodedDataByteCount = 512;
    /// <summary>Longueur totale du secteur après les mots de synchronisation, en octets encodés.</summary>
    public const int EncodedSectorByteCount = EncodedHeaderByteCount + EncodedDataByteCount;
    /// <summary>Taille logique d'un secteur Amiga, en octets.</summary>
    public const int SectorByteCount = 512;
    /// <summary>Nombre de secteurs par piste d'une disquette double densité.</summary>
    public const int DoubleDensitySectorsPerTrack = 11;
    /// <summary>Nombre de secteurs par piste d'une disquette haute densité.</summary>
    public const int HighDensitySectorsPerTrack = 22;
    /// <summary>Nombre minimal de secteurs intègres rendant une piste haute densité crédible.</summary>
    public const int HighDensityCredibleSectorCount = 17;
    /// <summary>Nombre minimal de pistes crédibles requis pour retenir la haute densité.</summary>
    public const int HighDensityCredibleTrackCount = 2;
    /// <summary>Poids d'un secteur reconnu dans le calcul de confiance.</summary>
    public const int ConfidenceSectorWeight = 3;
    /// <summary>Diviseur propre au codec Amiga dans le calcul de confiance.</summary>
    public const double ConfidenceDivisor = 44;
    /// <summary>Nombre de bits composant un quartet.</summary>
    public const int NibbleBitCount = 4;
    /// <summary>Longueur du gap précédant une piste encodée, en bits.</summary>
    public const int LeadingGapBitCount = 100;
    /// <summary>Longueur minimale du gap terminant une piste encodée, en bits.</summary>
    public const int TrailingGapBitCount = 128;
    /// <summary>Plus grand cylindre tenant dans les sept bits du champ de piste.</summary>
    public const int MaximumCylinder = byte.MaxValue >> TrackCylinderShift;
    /// <summary>Plus grande face tenant dans le bit faible du champ de piste.</summary>
    public const int MaximumHead = TrackHeadMask;
    /// <summary>Plus grand numéro de secteur stockable dans l'en-tête.</summary>
    public const int MaximumSectorNumber = byte.MaxValue;
    /// <summary>Plus grand nombre de secteurs restant stockable dans l'en-tête.</summary>
    public const int MaximumRemainingSectorCount = byte.MaxValue;

    /// <summary>Empaquette le cylindre et la face dans l'octet de piste Amiga.</summary>
    /// <param name="cylinder">Cylindre compris entre zéro et 127.</param>
    /// <param name="head">Face égale à zéro ou un.</param>
    /// <returns>Octet de piste Amiga.</returns>
    public static byte PackTrack(int cylinder, int head) => (byte)((cylinder << TrackCylinderShift) | (head & TrackHeadMask));

    /// <summary>Crée l'erreur signalant une taille de secteur incompatible avec le format Amiga.</summary>
    /// <param name="actualSize">Taille reçue, en octets.</param>
    /// <returns>Erreur décrivant la taille attendue et la taille reçue.</returns>
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"Amiga sectors contain {SectorByteCount} bytes; received {actualSize} bytes.");

    /// <summary>Crée l'erreur signalant un nombre impair d'octets à encoder en odd/even.</summary>
    /// <param name="actualCount">Nombre d'octets reçu.</param>
    /// <returns>Erreur décrivant la contrainte de parité du nombre d'octets.</returns>
    public static ArgumentException OddEncodedByteCount(int actualCount) => new($"Amiga odd/even encoding requires an even byte count; received {actualCount} bytes.");

    /// <summary>Crée l'erreur signalant une plage incompatible avec le calcul de parité demandé.</summary>
    public static ArgumentException InvalidParityRange(int offset, int count, int available) => new($"Amiga parity range offset {offset}, count {count}, available {available} is invalid.");
}
