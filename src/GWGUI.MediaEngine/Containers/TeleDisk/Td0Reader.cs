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
        if (data.Length < Td0Layout.HeaderSize) throw Td0Exceptions.Truncated(Td0Section.ImageHeader, 0, Td0Layout.HeaderSize, data.Length);
        if (data[Td0Layout.SignatureOffset..].StartsWith(Td0Format.AdvancedCompressionSignature)) throw Td0Exceptions.AdvancedCompression();
        if (!data[Td0Layout.SignatureOffset..].StartsWith(Td0Format.UncompressedSignature)) throw Td0Exceptions.InvalidSignature();
        _ = data[Td0Layout.VersionOffset];
        _ = data[Td0Layout.DataRateOffset];
        var storedHeaderCrc = ReadUInt16(data, Td0Layout.HeaderCrcOffset);
        var calculatedHeaderCrc = Td0Crc16.Compute(data[..Td0Layout.HeaderCrcOffset]);
        if (storedHeaderCrc != calculatedHeaderCrc) throw Td0Exceptions.InvalidHeaderCrc(storedHeaderCrc, calculatedHeaderCrc);

        var offset = Td0Layout.HeaderSize;
        var stepping = data[Td0Layout.SteppingOffset];
        if ((stepping & Td0Layout.CommentPresentMask) != 0)
        {
            EnsureAvailable(data, offset, Td0Layout.CommentHeaderSize, Td0Section.CommentHeader);
            var commentLength = ReadUInt16(data, offset + Td0Layout.CommentLengthOffset);
            offset += Td0Layout.CommentHeaderSize;
            EnsureAvailable(data, offset, commentLength, Td0Section.Comment);
            offset += commentLength;
        }

        var sectors = new List<Td0Sector>();
        while (true)
        {
            EnsureAvailable(data, offset, Td0Layout.ByteFieldSize, Td0Section.TrackHeader);
            var sectorCount = data[offset + Td0Layout.TrackSectorCountOffset];
            if (sectorCount == Td0Layout.EndOfTracks) break;
            EnsureAvailable(data, offset, Td0Layout.TrackHeaderSize, Td0Section.TrackHeader);
            var trackCylinder = data[offset + Td0Layout.TrackCylinderOffset];
            var trackHead = data[offset + Td0Layout.TrackHeadOffset] & Td0Layout.HeadMask;
            var storedTrackCrc = data[offset + Td0Layout.TrackCrcOffset];
            var calculatedTrackCrc = (byte)Td0Crc16.Compute(data.Slice(offset, Td0Layout.TrackCrcOffset));
            if (storedTrackCrc != calculatedTrackCrc) throw Td0Exceptions.InvalidTrackCrc(trackCylinder, trackHead, storedTrackCrc, calculatedTrackCrc);
            offset += Td0Layout.TrackHeaderSize;

            for (var index = 0; index < sectorCount; index++)
            {
                EnsureAvailable(data, offset, Td0Layout.SectorHeaderSize, Td0Section.SectorHeader);
                var sectorOffset = offset;
                var cylinder = data[offset + Td0Layout.SectorCylinderOffset];
                var head = data[offset + Td0Layout.SectorHeadOffset] & Td0Layout.HeadMask;
                var number = data[offset + Td0Layout.SectorNumberOffset];
                var sizeCode = data[offset + Td0Layout.SectorSizeCodeOffset];
                var flags = (Td0SectorFlags)data[offset + Td0Layout.SectorFlagsOffset];
                offset += Td0Layout.SectorHeaderSize;

                if (sizeCode > Td0Layout.MaximumSectorSizeCode) throw Td0Exceptions.InvalidSizeCode(cylinder, head, number, sizeCode);
                var expectedLength = Td0Layout.BaseSectorSize << sizeCode;
                byte[] sectorData;
                if ((flags & Td0SectorFlags.UnavailableMask) != 0)
                {
                    sectorData = new byte[expectedLength];
                }
                else
                {
                    EnsureAvailable(data, offset, Td0Layout.SectorDataHeaderSize, Td0Section.SectorDataHeader);
                    var encodedLength = ReadUInt16(data, offset + Td0Layout.EncodedLengthOffset);
                    var encoding = (Td0SectorEncoding)data[offset + Td0Layout.EncodingOffset];
                    offset += Td0Layout.SectorDataHeaderSize;
                    if (encodedLength == 0) throw Td0Exceptions.MissingEncodedData(cylinder, head, number, offset);
                    var payloadLength = encodedLength - Td0Layout.EncodingFieldSize;
                    EnsureAvailable(data, offset, payloadLength, Td0Section.SectorData);
                    sectorData = Td0SectorDecoder.Decode(data.Slice(offset, payloadLength), encoding, expectedLength, cylinder, head, number);
                    offset += payloadLength;
                }

                var storedSectorCrc = data[sectorOffset + Td0Layout.SectorCrcOffset];
                var calculatedSectorCrc = (byte)Td0Crc16.Compute(sectorData);
                if (storedSectorCrc != calculatedSectorCrc) throw Td0Exceptions.InvalidSectorCrc(cylinder, head, number, storedSectorCrc, calculatedSectorCrc);

                sectors.Add(new(cylinder, head, number, sectorData, (flags & Td0SectorFlags.DataCrcError) == 0));
            }

            if (sectorCount != 0 && sectors[^1].Cylinder != trackCylinder) throw Td0Exceptions.InconsistentCylinder(trackCylinder, sectors[^1].Cylinder);
            if (sectorCount != 0 && sectors[^1].Head != trackHead) throw Td0Exceptions.InconsistentHead(trackHead, sectors[^1].Head);
        }

        if (sectors.Count == 0) throw Td0Exceptions.NoSectors();
        var blockSize = sectors.GroupBy(sector => sector.Data.Length).OrderByDescending(group => group.Count()).First().Key;
        // Les images TeleDisk protégées peuvent contenir quelques secteurs volontairement inhabituels.
        // L'image logique normale est reconstruite à partir de la taille sectorielle dominante.
        var logicalSectors = sectors.Where(sector => sector.Data.Length == blockSize).ToArray();

        var cylinders = logicalSectors.Max(sector => sector.Cylinder) + 1;
        var heads = logicalSectors.Max(sector => sector.Head) + 1;
        var sectorsPerTrack = logicalSectors.GroupBy(sector => (sector.Cylinder, sector.Head)).Max(group => group.Count());
        var blocks = logicalSectors
            .OrderBy(sector => sector.Cylinder).ThenBy(sector => sector.Head).ThenBy(sector => sector.Number)
            .Select((sector, logical) => new SectorBlock(logical, new SectorAddress(sector.Cylinder, sector.Head, sector.Number), sector.Data, sector.IntegrityValid))
            .ToArray();
        var formatId = Td0SectorImageClassifier.Detect(blocks, blockSize, cylinders, heads, sectorsPerTrack);
        return new SectorImage(formatId, blockSize, cylinders, heads, sectorsPerTrack, blocks, capacity: blocks.LongLength * blockSize, logicalBlockCount: blocks.Length);
    }

    private static void EnsureAvailable(ReadOnlySpan<byte> data, int offset, int count, Td0Section section)
    {
        if (offset < 0 || count < 0 || offset > data.Length - count) throw Td0Exceptions.Truncated(section, offset, count, Math.Max(0, data.Length - offset));
    }

    private static ushort ReadUInt16(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(data[offset..]);
    private sealed record Td0Sector(int Cylinder, int Head, int Number, byte[] Data, bool IntegrityValid);
}
