using System.Buffers.Binary;

namespace GWGUI.MediaEngine.Containers.I86f;

public sealed class I86fReader
{
    public async Task<I86fImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        if (data.Length < I86fLayout.MinimumFileLength || BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(I86fFormat.SignatureOffset, I86fFormat.SignatureLength)) != I86fFormat.Signature) throw new InvalidDataException("The file does not contain an 86F signature.");

        var fileFlags = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(I86fLayout.FileFlagsOffset, I86fLayout.FileFlagsLength));
        var tableEntryCount = (fileFlags & 0x0008) != 0 ? I86fLayout.TwoSideTrackTableEntries : I86fLayout.TrackTableEntriesPerSide;
        var tableEnd = checked(I86fLayout.TrackTableOffset + tableEntryCount * I86fLayout.TrackTableEntrySize);
        if (data.Length < tableEnd) throw new InvalidDataException("The 86F track table is incomplete.");

        var tracks = new List<I86fTrack>();
        for (var logicalTrack = 0; logicalTrack < tableEntryCount; logicalTrack++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var offset = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(I86fLayout.TrackTableOffset + logicalTrack * I86fLayout.TrackTableEntrySize, I86fLayout.TrackTableEntrySize)));
            if (offset == 0) continue;
            var nextOffset = NextOffset(data, logicalTrack + 1, tableEntryCount, data.Length);
            var track = ReadTrack(data, logicalTrack, offset, nextOffset, fileFlags);
            if (track is not null) tracks.Add(track);
        }
        return new(fileFlags, tracks);
    }

    private static I86fTrack? ReadTrack(byte[] data, int logicalTrack, int offset, int nextOffset, ushort fileFlags)
    {
        var hasExtraBitCells = (fileFlags & 0x0080) != 0;
        var headerSize = hasExtraBitCells ? I86fLayout.ExtendedTrackHeaderSize : I86fLayout.StandardTrackHeaderSize;
        if (offset < 0 || offset > data.Length - headerSize || nextOffset < offset + headerSize) throw new InvalidDataException("An 86F track points outside the image.");

        var trackFlags = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(offset + I86fLayout.TrackFlagsOffset, I86fLayout.FileFlagsLength));
        var bitCount = hasExtraBitCells && (fileFlags & 0x0060) == 0 && (fileFlags & 0x1000) != 0 ? BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(offset + I86fLayout.ExplicitBitCountOffset)) : checked((nextOffset - offset - headerSize) * I86fLayout.BitsPerByte);
        if (bitCount <= 0) throw new InvalidDataException("An 86F track has an invalid bit-cell count.");
        var byteCount = checked(((bitCount + I86fLayout.WordBitAlignment - 1) / I86fLayout.WordBitAlignment) * I86fLayout.BytesPerWord);
        if (offset + headerSize > data.Length - byteCount) throw new InvalidDataException("An 86F track is incomplete.");

        var source = data.AsSpan(offset + headerSize, byteCount);
        var reverseBytes = (fileFlags & 0x0800) != 0;
        var bits = new bool[bitCount];
        for (var bit = 0; bit < bitCount; bit++)
        {
            var wordByte = bit / I86fLayout.WordBitAlignment * I86fLayout.BytesPerWord;
            var byteInWord = bit / I86fLayout.BitsPerByte % I86fLayout.BytesPerWord;
            if (reverseBytes) byteInWord ^= I86fLayout.BytesPerWord - 1;
            bits[bit] = (source[wordByte + byteInWord] & (I86fLayout.MostSignificantBitMask >> (bit % I86fLayout.BitsPerByte))) != 0;
        }
        return bits.Any(value => value) ? new(logicalTrack, trackFlags, bitCount, bits) : null;
    }

    private static int NextOffset(byte[] data, int start, int count, int fallback)
    {
        for (var index = start; index < count; index++)
        {
            var value = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(I86fLayout.TrackTableOffset + index * I86fLayout.TrackTableEntrySize, I86fLayout.TrackTableEntrySize)));
            if (value != 0) return value;
        }
        return fallback;
    }
}
