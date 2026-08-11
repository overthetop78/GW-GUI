using System.Buffers.Binary;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Recognition.TeleDisk;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.SectorImages.Reading;

namespace GWGUI.MediaEngine.Containers.TeleDisk;

/// <summary>Reads ordinary (uppercase TD signature) TeleDisk images.</summary>
public sealed class Td0Reader : ISectorImageReader
{
    public bool CanRead(string path) => Path.GetExtension(path).Equals(DiskImageFileExtensions.Td0, StringComparison.OrdinalIgnoreCase);

    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        return Read(data);
    }

    internal static SectorImage Read(ReadOnlySpan<byte> data)
    {
        if (data.Length < Td0Layout.HeaderSize || !data[Td0Layout.SignatureOffset..].StartsWith(Td0Format.UncompressedSignature))
            throw new InvalidDataException("The image is not an uncompressed TeleDisk image.");

        var offset = Td0Layout.HeaderSize;
        var stepping = data[Td0Layout.SteppingOffset];
        if ((stepping & Td0Layout.CommentPresentMask) != 0)
        {
            EnsureAvailable(data, offset, Td0Layout.CommentHeaderSize, "TeleDisk comment header");
            var commentLength = ReadUInt16(data, offset + Td0Layout.CommentLengthOffset);
            offset += Td0Layout.CommentHeaderSize;
            EnsureAvailable(data, offset, commentLength, "TeleDisk comment");
            offset += commentLength;
        }

        var sectors = new List<Td0Sector>();
        while (true)
        {
            EnsureAvailable(data, offset, Td0Layout.ByteFieldSize, "TeleDisk track header");
            var sectorCount = data[offset + Td0Layout.TrackSectorCountOffset];
            if (sectorCount == Td0Layout.EndOfTracks) break;
            EnsureAvailable(data, offset, Td0Layout.TrackHeaderSize, "TeleDisk track header");
            var trackCylinder = data[offset + Td0Layout.TrackCylinderOffset];
            var trackHead = data[offset + Td0Layout.TrackHeadOffset] & Td0Layout.HeadMask;
            offset += Td0Layout.TrackHeaderSize;

            for (var index = 0; index < sectorCount; index++)
            {
                EnsureAvailable(data, offset, Td0Layout.SectorHeaderSize, "TeleDisk sector header");
                var cylinder = data[offset + Td0Layout.SectorCylinderOffset];
                var head = data[offset + Td0Layout.SectorHeadOffset] & Td0Layout.HeadMask;
                var number = data[offset + Td0Layout.SectorNumberOffset];
                var sizeCode = data[offset + Td0Layout.SectorSizeCodeOffset];
                var flags = (Td0SectorFlags)data[offset + Td0Layout.SectorFlagsOffset];
                offset += Td0Layout.SectorHeaderSize;

                if (sizeCode > Td0Layout.MaximumSectorSizeCode) throw new InvalidDataException($"TeleDisk sector {cylinder}/{head}/{number} has an invalid size code.");
                var expectedLength = Td0Layout.BaseSectorSize << sizeCode;
                byte[] sectorData;
                if ((flags & Td0SectorFlags.UnavailableMask) != 0)
                {
                    sectorData = new byte[expectedLength];
                }
                else
                {
                    EnsureAvailable(data, offset, Td0Layout.SectorDataHeaderSize, "TeleDisk sector data header");
                    var encodedLength = ReadUInt16(data, offset + Td0Layout.EncodedLengthOffset);
                    var encoding = (Td0SectorEncoding)data[offset + Td0Layout.EncodingOffset];
                    offset += Td0Layout.SectorDataHeaderSize;
                    if (encodedLength == 0) throw new InvalidDataException($"TeleDisk sector {cylinder}/{head}/{number} has no encoded data.");
                    var payloadLength = encodedLength - Td0Layout.EncodingFieldSize;
                    EnsureAvailable(data, offset, payloadLength, "TeleDisk sector data");
                    sectorData = Td0SectorDecoder.Decode(data.Slice(offset, payloadLength), encoding, expectedLength);
                    offset += payloadLength;
                }

                sectors.Add(new(cylinder, head, number, sectorData, (flags & Td0SectorFlags.DataCrcError) == 0));
            }

            if (sectorCount != 0 && sectors[^1].Cylinder != trackCylinder)
                throw new InvalidDataException("A TeleDisk track contains an inconsistent cylinder number.");
            if (sectorCount != 0 && sectors[^1].Head != trackHead)
                throw new InvalidDataException("A TeleDisk track contains an inconsistent head number.");
        }

        if (sectors.Count == 0) throw new InvalidDataException("The TeleDisk image contains no sectors.");
        var blockSize = sectors.GroupBy(sector => sector.Data.Length).OrderByDescending(group => group.Count()).First().Key;
        // Copy-protected TeleDisk images may add a few deliberately unusual sectors.
        // Reconstruct the normal logical image from the dominant sector size rather
        // than rejecting the complete disk.
        var logicalSectors = sectors.Where(sector => sector.Data.Length == blockSize).ToArray();

        var cylinders = logicalSectors.Max(sector => sector.Cylinder) + 1;
        var heads = logicalSectors.Max(sector => sector.Head) + 1;
        var sectorsPerTrack = logicalSectors.GroupBy(sector => (sector.Cylinder, sector.Head)).Max(group => group.Count());
        var blocks = logicalSectors
            .OrderBy(sector => sector.Cylinder).ThenBy(sector => sector.Head).ThenBy(sector => sector.Number)
            .Select((sector, logical) => new SectorBlock(logical,
                new SectorAddress(sector.Cylinder, sector.Head, sector.Number), sector.Data, sector.IntegrityValid))
            .ToArray();
        var formatId = Td0SectorImageClassifier.Detect(blocks, blockSize, cylinders, heads, sectorsPerTrack);
        return new SectorImage(formatId, blockSize, cylinders, heads, sectorsPerTrack, blocks, capacity: blocks.LongLength * blockSize, logicalBlockCount: blocks.Length);
    }

    private static void EnsureAvailable(ReadOnlySpan<byte> data, int offset, int count, string description)
    {
        if (offset < 0 || count < 0 || offset > data.Length - count) throw new InvalidDataException($"The {description} is truncated.");
    }

    private static ushort ReadUInt16(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(data[offset..]);
    private sealed record Td0Sector(int Cylinder, int Head, int Number, byte[] Data, bool IntegrityValid);
}
