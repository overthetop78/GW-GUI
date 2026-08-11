namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Regroupe les définitions communes du format MFM Amiga.</summary>
internal static class AmigaMfmFormat
{
    public const string CodecId = FluxCodecIds.AmigaMfm;
    public const string CodecDisplayName = FluxCodecDisplayNames.AmigaMfm;
    public const string StructureDescriptionName = "Amiga";
    public const ushort SyncWord = 0x4489;
    public const int SyncWordCount = 2;
    public const int EncodedByteBitCount = MfmEncoding.EncodedByteBitCount;
    public const int SyncBitCount = SyncWordCount * EncodedByteBitCount;
    public const byte FormatByte = 0xff;
    public const int InfoByteCount = 4;
    public const int FormatByteOffset = 0;
    public const int TrackAndHeadOffset = 1;
    public const int SectorNumberOffset = 2;
    public const int RemainingSectorCountOffset = 3;
    public const int TrackCylinderShift = 1;
    public const byte TrackHeadMask = 1;
    public const int LabelByteCount = 16;
    public const int HeaderParitySourceByteCount = InfoByteCount + LabelByteCount;
    public const int HeaderParityHighOffset = 22;
    public const int HeaderParityLowOffset = 23;
    public const int DataParityHighOffset = 26;
    public const int DataParityLowOffset = 27;
    public const int ParityFieldByteCount = 8;
    public const int EncodedHeaderByteCount = HeaderParitySourceByteCount + ParityFieldByteCount;
    public const int EncodedDataOffset = EncodedHeaderByteCount;
    public const int EncodedDataByteCount = 512;
    public const int EncodedSectorByteCount = EncodedHeaderByteCount + EncodedDataByteCount;
    public const int SectorByteCount = 512;
    public const int ConfidenceSectorWeight = 3;
    public const double ConfidenceDivisor = 44;
    public const int NibbleBitCount = 4;
    public const int LeadingGapBitCount = 100;
    public const int TrailingGapBitCount = 128;

    public static ArgumentException InvalidSectorSize(int actualSize) => new($"Amiga sectors contain {SectorByteCount} bytes; received {actualSize} bytes.");
    public static ArgumentException OddEncodedByteCount(int actualCount) => new($"Amiga odd/even encoding requires an even byte count; received {actualCount} bytes.");
}
