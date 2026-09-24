namespace GWGUI.MediaEngine.Constants;

/// <summary>Shared CHD container constants used by every supported media family.</summary>
internal static class ChdConstants
{
    public const string Signature = "MComprHD";
    public const uint Version5 = 5;
    public const int Version5HeaderSize = 124;
    public const int Version5MapEntrySize = 4;
    public const int MetadataHeaderSize = 16;
    public const int Sha1Length = 20;
    public const int DefaultHunkSize = 512 * 1_024;
    public const uint UncompressedCodec = 0;
    public const uint ZlibCodec = 0x7A6C6962;
    public const uint LzmaCodec = 0x6C7A6D61;
    public const uint HuffmanCodec = 0x68756666;
    public const uint FlacCodec = 0x666C6163;
    public const uint CdZlibCodec = 0x63647A6C;
    public const uint CdLzmaCodec = 0x63646C7A;
    public const uint CdFlacCodec = 0x6364666C;
    public const uint HardDiskMetadataTag = 0x47444444;
    public const uint CdTrackMetadataTag = 0x43485452;
    public const uint CdTrackMetadata2Tag = 0x43485432;
    public const uint DvdMetadataTag = 0x44564420;
    public const uint MetadataLengthMask = 0x00FFFFFF;
    public const uint MetadataFlagsMask = 0xFF000000;
    public const int CdFrameSize = 2_448;
    public const int CdTrackFrameAlignment = 4;
    public const string HardDiskMetadataFormat = "CYLS:{0},HEADS:{1},SECS:{2},BPS:{3}";
    public const string CdTrackMetadataPrefix = "TRACK:";
    public const string CdTrackMetadata2PregapMarker = " PREGAP:";
}
