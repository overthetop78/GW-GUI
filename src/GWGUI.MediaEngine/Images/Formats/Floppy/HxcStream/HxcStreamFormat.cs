namespace GWGUI.MediaEngine.Images.Formats.Floppy.HxcStream;

internal static class HxcStreamFormat
{
    public const uint ChunkSignature = 0x484B4843;
    public const int ChunkHeaderSize = 12;
    public const int ChunkCrcSize = 4;
    public const uint MetadataBlock = 0;
    public const uint PackedIoBlock = 1;
    public const uint PackedStreamBlock = 2;
    public const int BlockHeaderSize = 8;
    public const int PackedIoHeaderSize = 16;
    public const int PackedStreamHeaderSize = 20;
    public const int DefaultResolutionNanoseconds = 40;
    public const int IoSampleFluxTicks = 16;
    public const int MaximumTrack = 83;
    public const int MaximumHead = 1;
    public const int MaximumChunkSize = 64 * 1024 * 1024;
    public const int MaximumUnpackedBlockSize = 64 * 1024 * 1024;
    public const int MaximumPulseCount = 500 * 1000 * 1000;
}
