using System.Buffers.Binary;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Recognition.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images;

/// <summary>Writes Apple II 5.25-inch nibble streams without inventing a new image format.</summary>
public sealed class AppleNibbleImageWriter(FluxEncoderRegistry? encoders = null)
{
    private const int NibTrackLength = 6_656;
    private const int WozTrackDataLength = 6_648;
    private readonly FluxEncoderRegistry _encoders = encoders ?? new FluxEncoderRegistry();

    public Task WriteAsync(SectorImage image, string path, CancellationToken cancellationToken = default) =>
        Path.GetExtension(path).ToLowerInvariant() switch
        {
            DiskImageFileExtensions.Nib => WriteNibAsync(image, path, cancellationToken),
            DiskImageFileExtensions.Woz => WriteWozAsync(image, path, cancellationToken),
            _ => throw new NotSupportedException("Apple nibble output must use the NIB or WOZ extension.")
        };

    public async Task WriteNibAsync(SectorImage image, string path, CancellationToken cancellationToken = default)
    {
        var tracks = EncodeTracks(image, WozTrackDataLength * 8, cancellationToken);
        var output = new byte[tracks.Count * NibTrackLength];
        Array.Fill(output, (byte)0xff);
        for (var track = 0; track < tracks.Count; track++)
            PackBits(tracks[track], output.AsSpan(track * NibTrackLength, NibTrackLength));
        await File.WriteAllBytesAsync(path, output, cancellationToken).ConfigureAwait(false);
    }

    public async Task WriteWozAsync(SectorImage image, string path, CancellationToken cancellationToken = default)
    {
        var tracks = EncodeTracks(image, WozTrackDataLength * 8, cancellationToken);
        using var stream = new MemoryStream();
        stream.Write("WOZ1"u8);
        stream.Write([0xff, 0x0a, 0x0d, 0x0a]);
        stream.Write(new byte[4]);

        var info = new byte[60];
        info[0] = 1; // INFO version.
        info[1] = 1; // Apple II 5.25-inch disk.
        info[2] = 0; // Write protected: no.
        info[3] = 1; // Synchronized tracks.
        info[4] = 1; // Cleaned image.
        System.Text.Encoding.ASCII.GetBytes("GW GUI").CopyTo(info, 5);
        WriteChunk(stream, "INFO", info);

        var tmap = new byte[160];
        Array.Fill(tmap, (byte)0xff);
        for (var track = 0; track < tracks.Count && track < 40; track++)
            for (var quarter = 0; quarter < 4; quarter++) tmap[track * 4 + quarter] = (byte)track;
        WriteChunk(stream, "TMAP", tmap);

        var trks = new byte[tracks.Count * NibTrackLength];
        for (var track = 0; track < tracks.Count; track++)
        {
            var entry = trks.AsSpan(track * NibTrackLength, NibTrackLength);
            PackBits(tracks[track], entry[..WozTrackDataLength]);
            BinaryPrimitives.WriteUInt16LittleEndian(entry.Slice(WozTrackDataLength, 2), checked((ushort)tracks[track].Count));
        }
        WriteChunk(stream, "TRKS", trks);

        var output = stream.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(output.AsSpan(8, 4), Crc32(output.AsSpan(12)));
        await File.WriteAllBytesAsync(path, output, cancellationToken).ConfigureAwait(false);
    }

    private IReadOnlyList<IReadOnlyList<bool>> EncodeTracks(SectorImage image, int maximumBits, CancellationToken cancellationToken)
    {
        if (!image.FormatId.Equals(DiskImageFormatIds.AppleIIRwts18, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("The source does not contain an Apple II RWTS18 image.");
        var tracks = new List<IReadOnlyList<bool>>(image.Cylinders);
        for (var cylinder = 0; cylinder < image.Cylinders; cylinder++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var sectors = new List<TrackSector>(6);
            for (var sector = 0; sector < 6; sector++)
            {
                var logical = cylinder * 6 + sector;
                if (!image.TryGetBlock(logical, out var block) || block.Data.Count != 768)
                    throw new InvalidDataException($"RWTS18 track {cylinder} sector {sector} is missing or invalid.");
                sectors.Add(new(sector, block.Data));
            }
            var encoded = _encoders.Encode(DiskImageFormatIds.AppleIIRwts18, new(cylinder, 0, sectors));
            if (encoded.Bits.Count > maximumBits)
                throw new InvalidDataException($"RWTS18 track {cylinder} does not fit in an Apple nibble track.");
            tracks.Add(encoded.Bits);
        }
        return tracks;
    }

    private static void PackBits(IReadOnlyList<bool> bits, Span<byte> destination)
    {
        destination.Fill(0xff);
        for (var bit = 0; bit < bits.Count; bit++)
        {
            var mask = (byte)(1 << (7 - bit % 8));
            if (bits[bit]) destination[bit / 8] |= mask;
            else destination[bit / 8] &= (byte)~mask;
        }
    }

    private static void WriteChunk(Stream stream, string id, byte[] data)
    {
        stream.Write(System.Text.Encoding.ASCII.GetBytes(id));
        Span<byte> length = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(length, checked((uint)data.Length));
        stream.Write(length);
        stream.Write(data);
    }

    private static uint Crc32(ReadOnlySpan<byte> data)
    {
        var crc = uint.MaxValue;
        foreach (var value in data)
        {
            crc ^= value;
            for (var bit = 0; bit < 8; bit++) crc = (crc >> 1) ^ (0xedb88320u & (uint)-(int)(crc & 1));
        }
        return ~crc;
    }
}
