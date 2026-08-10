using System.Buffers.Binary;
using GWGUI.MediaEngine.Containers.Apple.Woz;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Recognition.Apple;
using GWGUI.MediaEngine.Recognition.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images;

/// <summary>Writes Apple II 5.25-inch nibble streams without inventing a new image format.</summary>
public sealed class AppleNibbleImageWriter(FluxEncoderRegistry? encoders = null)
{
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
        var tracks = EncodeTracks(image, WozLayout.Woz1BitCountOffset * NibTrackFormat.BitsPerByte, cancellationToken);
        var output = new byte[tracks.Count * NibTrackFormat.TrackLength];
        Array.Fill(output, (byte)0xff);
        for (var track = 0; track < tracks.Count; track++)
            PackBits(tracks[track], output.AsSpan(track * NibTrackFormat.TrackLength, NibTrackFormat.TrackLength));
        await File.WriteAllBytesAsync(path, output, cancellationToken).ConfigureAwait(false);
    }

    public async Task WriteWozAsync(SectorImage image, string path, CancellationToken cancellationToken = default)
    {
        var tracks = EncodeTracks(image, WozLayout.Woz1BitCountOffset * NibTrackFormat.BitsPerByte, cancellationToken);
        using var stream = new MemoryStream();
        stream.Write(WozFormat.Version1Signature);
        stream.Write(WozFormat.HeaderMarker);
        stream.Write(new byte[WozLayout.CrcLength]);

        var info = new byte[60];
        info[0] = 1; // INFO version.
        info[WozLayout.InfoDiskTypeOffset] = WozFormat.AppleII525DiskType;
        info[2] = 0; // Write protected: no.
        info[3] = 1; // Synchronized tracks.
        info[4] = 1; // Cleaned image.
        System.Text.Encoding.ASCII.GetBytes("GW GUI").CopyTo(info, 5);
        WriteChunk(stream, WozFormat.InfoChunkId, info);

        var tmap = new byte[WozLayout.TrackMapLength];
        Array.Fill(tmap, WozLayout.MissingTrackDescriptor);
        for (var track = 0; track < tracks.Count && track < WozLayout.AppleIITrackCount; track++)
            for (var quarter = 0; quarter < WozLayout.TrackMapEntriesPerTrack; quarter++)
                tmap[track * WozLayout.TrackMapEntriesPerTrack + quarter] = (byte)track;
        WriteChunk(stream, WozFormat.TrackMapChunkId, tmap);

        var trks = new byte[tracks.Count * WozLayout.Woz1TrackEntryLength];
        for (var track = 0; track < tracks.Count; track++)
        {
            var entry = trks.AsSpan(track * WozLayout.Woz1TrackEntryLength, WozLayout.Woz1TrackEntryLength);
            PackBits(tracks[track], entry[..WozLayout.Woz1BitCountOffset]);
            BinaryPrimitives.WriteUInt16LittleEndian(entry.Slice(WozLayout.Woz1BitCountOffset, WozLayout.Woz1BitCountLength), checked((ushort)tracks[track].Count));
        }
        WriteChunk(stream, WozFormat.TracksChunkId, trks);

        var output = stream.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(output.AsSpan(WozLayout.CrcOffset, WozLayout.CrcLength), Crc32(output.AsSpan(WozLayout.ChunksOffset)));
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
            var mask = (byte)(1 << (NibTrackFormat.BitsPerByte - 1 - bit % NibTrackFormat.BitsPerByte));
            if (bits[bit]) destination[bit / NibTrackFormat.BitsPerByte] |= mask;
            else destination[bit / NibTrackFormat.BitsPerByte] &= (byte)~mask;
        }
    }

    private static void WriteChunk(Stream stream, string id, byte[] data)
    {
        stream.Write(System.Text.Encoding.ASCII.GetBytes(id));
        Span<byte> length = stackalloc byte[WozLayout.ChunkLengthSize];
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
            for (var bit = 0; bit < NibTrackFormat.BitsPerByte; bit++)
                crc = (crc >> 1) ^ (WozFormat.Crc32Polynomial & (uint)-(int)(crc & 1));
        }
        return ~crc;
    }
}
