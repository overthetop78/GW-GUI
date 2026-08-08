using System.Buffers.Binary;
using GWGUI.Scp.Decoding;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images;

/// <summary>Decodes Apple II NIB byte streams and WOZ1/WOZ2 bit-stream containers.</summary>
internal static class AppleNibbleImageDecoder
{
    private const int NibTrackLength = 6656;

    public static SectorImage ReadNib(ReadOnlySpan<byte> data)
    {
        if (data.Length == 0 || data.Length % NibTrackLength != 0)
            throw new InvalidDataException("The Apple NIB image length is invalid.");
        var tracks = new List<(int Track, IReadOnlyList<DecodedSector> Sectors)>();
        var decoder = new AppleGcrDecoder();
        for (var track = 0; track < data.Length / NibTrackLength; track++)
        {
            var bits = BytesToBits(data.Slice(track * NibTrackLength, NibTrackLength), NibTrackLength * 8);
            tracks.Add((track, decoder.DecodeBits(bits).Sectors ?? []));
        }
        return AppleDiskImageReader.CreateAppleIIFromDecodedTracks(tracks);
    }

    public static SectorImage ReadWoz(ReadOnlySpan<byte> data)
    {
        if (data.Length < 256 || !(data[..4].SequenceEqual("WOZ1"u8) || data[..4].SequenceEqual("WOZ2"u8)) ||
            !data.Slice(4, 4).SequenceEqual(new byte[] { 0xff, 0x0a, 0x0d, 0x0a }))
            throw new InvalidDataException("The WOZ header is invalid.");
        var version = data[3] - (byte)'0';
        var chunks = ReadChunks(data);
        if (!chunks.TryGetValue("INFO", out var info) || info.Length < 2 || info.Span[1] != 1)
            throw new NotSupportedException("Only Apple II 5.25-inch WOZ images are supported by this reader.");
        if (!chunks.TryGetValue("TMAP", out var tmap) || tmap.Length < 160 || !chunks.TryGetValue("TRKS", out var trks))
            throw new InvalidDataException("The WOZ track map or track data is missing.");

        var decoder = new AppleGcrDecoder();
        var tracks = new List<(int Track, IReadOnlyList<DecodedSector> Sectors)>();
        for (var track = 0; track < 40; track++)
        {
            var descriptor = tmap.Span[track * 4];
            if (descriptor == 0xff) continue;
            var bits = version == 1 ? ReadWoz1Track(trks.Span, descriptor) : ReadWoz2Track(data, trks.Span, descriptor);
            if (bits.Length == 0) continue;
            tracks.Add((track, decoder.DecodeBits(bits).Sectors ?? []));
        }
        return AppleDiskImageReader.CreateAppleIIFromDecodedTracks(tracks);
    }

    private static Dictionary<string, ReadOnlyMemory<byte>> ReadChunks(ReadOnlySpan<byte> data)
    {
        var chunks = new Dictionary<string, ReadOnlyMemory<byte>>(StringComparer.Ordinal);
        var offset = 12;
        while (offset <= data.Length - 8)
        {
            var id = System.Text.Encoding.ASCII.GetString(data.Slice(offset, 4));
            var length = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(offset + 4, 4)));
            offset += 8;
            if (length < 0 || offset > data.Length - length) throw new InvalidDataException($"The WOZ {id} chunk is truncated.");
            chunks[id] = data.Slice(offset, length).ToArray();
            offset += length;
        }
        return chunks;
    }

    private static bool[] ReadWoz1Track(ReadOnlySpan<byte> trks, int index)
    {
        const int entryLength = 6656;
        const int bitCountOffset = 6648;
        var offset = checked(index * entryLength);
        if (offset > trks.Length - entryLength) return [];
        var bitCount = BinaryPrimitives.ReadUInt16LittleEndian(trks.Slice(offset + bitCountOffset, 2));
        if (bitCount == 0 || bitCount > bitCountOffset * 8) return [];
        return BytesToBits(trks.Slice(offset, (bitCount + 7) / 8), bitCount);
    }

    private static bool[] ReadWoz2Track(ReadOnlySpan<byte> file, ReadOnlySpan<byte> trks, int index)
    {
        var descriptorOffset = checked(index * 8);
        if (descriptorOffset > trks.Length - 8) return [];
        var startBlock = BinaryPrimitives.ReadUInt16LittleEndian(trks.Slice(descriptorOffset, 2));
        var blockCount = BinaryPrimitives.ReadUInt16LittleEndian(trks.Slice(descriptorOffset + 2, 2));
        var bitCount = BinaryPrimitives.ReadUInt32LittleEndian(trks.Slice(descriptorOffset + 4, 4));
        var offset = checked(startBlock * 512);
        var byteCount = checked((int)((bitCount + 7) / 8));
        if (startBlock == 0 || blockCount == 0 || bitCount == 0 || byteCount > blockCount * 512 || offset > file.Length - byteCount) return [];
        return BytesToBits(file.Slice(offset, byteCount), checked((int)bitCount));
    }

    private static bool[] BytesToBits(ReadOnlySpan<byte> bytes, int bitCount)
    {
        var bits = new bool[bitCount];
        for (var bit = 0; bit < bitCount; bit++) bits[bit] = (bytes[bit / 8] & (1 << (7 - bit % 8))) != 0;
        return bits;
    }
}
