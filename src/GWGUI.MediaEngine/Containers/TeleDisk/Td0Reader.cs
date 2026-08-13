using System.Buffers.Binary;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Recognition.TeleDisk;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.TeleDisk;

/// <summary>Lit les conteneurs TeleDisk non compressés portant la signature majuscule.</summary>
public sealed class Td0Reader
{
    /// <summary>Lit un conteneur TeleDisk et reconstruit son image sectorielle.</summary>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default) => (await ReadDetailedAsync(path, cancellationToken).ConfigureAwait(false)).SectorImage;

    /// <summary>Lit un conteneur TeleDisk en conservant ses en-têtes, son commentaire, ses cartes et ses états sectoriels.</summary>
    public async Task<Td0Image> ReadDetailedAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        return ReadDetailed(data, cancellationToken);
    }

    /// <summary>Analyse les octets d'un conteneur TeleDisk non compressé.</summary>
    internal static SectorImage Read(ReadOnlySpan<byte> data) => ReadDetailed(data).SectorImage;

    /// <summary>Analyse un conteneur TeleDisk en conservant toutes les informations réinscriptibles.</summary>
    public static Td0Image ReadDetailed(ReadOnlySpan<byte> data, CancellationToken cancellationToken = default)
    {
        if (data.Length < Td0Layout.HeaderSize) throw Td0Exceptions.Truncated(Td0Section.ImageHeader, 0, Td0Layout.HeaderSize, data.Length);
        if (data[Td0Layout.SignatureOffset..].StartsWith(Td0Format.AdvancedCompressionSignature)) throw Td0Exceptions.AdvancedCompression();
        if (!data[Td0Layout.SignatureOffset..].StartsWith(Td0Format.UncompressedSignature)) throw Td0Exceptions.InvalidSignature();
        var storedHeaderCrc = ReadUInt16(data, Td0Layout.HeaderCrcOffset);
        var calculatedHeaderCrc = Td0Crc16.Compute(data[..Td0Layout.HeaderCrcOffset]);
        if (storedHeaderCrc != calculatedHeaderCrc) throw Td0Exceptions.InvalidHeaderCrc(storedHeaderCrc, calculatedHeaderCrc);
        var header = new Td0Header(data[Td0Layout.SequenceOffset], data[Td0Layout.CheckSignatureOffset], data[Td0Layout.VersionOffset], data[Td0Layout.DataRateOffset], data[Td0Layout.DriveTypeOffset], data[Td0Layout.SteppingOffset], data[Td0Layout.DosModeOffset], data[Td0Layout.SurfaceCountOffset]);
        var offset = Td0Layout.HeaderSize;
        var comment = ReadComment(data, ref offset, header.TrackDensity);
        var tracks = new List<Td0Track>();
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureAvailable(data, offset, Td0Layout.ByteFieldSize, Td0Section.TrackHeader);
            var sectorCount = data[offset + Td0Layout.TrackSectorCountOffset];
            if (sectorCount == Td0Layout.EndOfTracks) break;
            EnsureAvailable(data, offset, Td0Layout.TrackHeaderSize, Td0Section.TrackHeader);
            var trackCylinder = data[offset + Td0Layout.TrackCylinderOffset];
            var trackHead = data[offset + Td0Layout.TrackHeadOffset];
            var storedTrackCrc = data[offset + Td0Layout.TrackCrcOffset];
            var calculatedTrackCrc = (byte)Td0Crc16.Compute(data.Slice(offset, Td0Layout.TrackCrcOffset));
            if (storedTrackCrc != calculatedTrackCrc) throw Td0Exceptions.InvalidTrackCrc(trackCylinder, trackHead & Td0Layout.HeadMask, storedTrackCrc, calculatedTrackCrc);
            offset += Td0Layout.TrackHeaderSize;
            var sectors = new List<Td0Sector>(sectorCount);
            for (var index = 0; index < sectorCount; index++) sectors.Add(ReadSector(data, ref offset));
            tracks.Add(new(trackCylinder, trackHead, sectors));
        }
        if (tracks.Sum(track => track.Sectors.Count) == 0) throw Td0Exceptions.NoSectors();
        return new(header, comment, tracks, BuildSectorImage(tracks));
    }

    private static Td0Comment? ReadComment(ReadOnlySpan<byte> data, ref int offset, byte trackDensity)
    {
        if ((trackDensity & Td0Layout.CommentPresentMask) == 0) return null;
        EnsureAvailable(data, offset, Td0Layout.CommentHeaderSize, Td0Section.CommentHeader);
        var commentHeader = data.Slice(offset, Td0Layout.CommentHeaderSize);
        var storedCrc = ReadUInt16(commentHeader, Td0Layout.CommentCrcOffset);
        var commentLength = ReadUInt16(commentHeader, Td0Layout.CommentLengthOffset);
        offset += Td0Layout.CommentHeaderSize;
        EnsureAvailable(data, offset, commentLength, Td0Section.Comment);
        var commentData = data.Slice(offset, commentLength).ToArray();
        var calculatedCrc = Td0Crc16.Compute(commentHeader[Td0Layout.CommentLengthOffset..]);
        calculatedCrc = Td0Crc16.Compute(commentData, calculatedCrc);
        if (storedCrc != calculatedCrc) throw Td0Exceptions.InvalidCommentCrc(storedCrc, calculatedCrc);
        offset += commentLength;
        return new(commentHeader[Td0Layout.CommentYearOffset], commentHeader[Td0Layout.CommentMonthOffset], commentHeader[Td0Layout.CommentDayOffset], commentHeader[Td0Layout.CommentHourOffset], commentHeader[Td0Layout.CommentMinuteOffset], commentHeader[Td0Layout.CommentSecondOffset], commentData);
    }

    private static Td0Sector ReadSector(ReadOnlySpan<byte> data, ref int offset)
    {
        EnsureAvailable(data, offset, Td0Layout.SectorHeaderSize, Td0Section.SectorHeader);
        var sectorHeader = data.Slice(offset, Td0Layout.SectorHeaderSize);
        var cylinder = sectorHeader[Td0Layout.SectorCylinderOffset];
        var head = sectorHeader[Td0Layout.SectorHeadOffset];
        var number = sectorHeader[Td0Layout.SectorNumberOffset];
        var sizeCode = sectorHeader[Td0Layout.SectorSizeCodeOffset];
        var flags = sectorHeader[Td0Layout.SectorFlagsOffset];
        offset += Td0Layout.SectorHeaderSize;
        if (sizeCode > Td0Layout.MaximumSectorSizeCode) throw Td0Exceptions.InvalidSizeCode(cylinder, head, number, sizeCode);
        var expectedLength = Td0Layout.BaseSectorSize << sizeCode;
        byte[]? sectorData = null;
        if ((((Td0SectorFlags)flags) & Td0SectorFlags.UnavailableMask) == 0)
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
        var crcData = sectorData ?? new byte[expectedLength];
        var storedSectorCrc = sectorHeader[Td0Layout.SectorCrcOffset];
        var calculatedSectorCrc = (byte)Td0Crc16.Compute(crcData);
        if (storedSectorCrc != calculatedSectorCrc) throw Td0Exceptions.InvalidSectorCrc(cylinder, head, number, storedSectorCrc, calculatedSectorCrc);
        return new(cylinder, head, number, sizeCode, flags, sectorData);
    }

    private static SectorImage BuildSectorImage(IReadOnlyList<Td0Track> tracks)
    {
        var sectors = tracks.SelectMany(track => track.Sectors).ToArray();
        var blockSize = sectors.GroupBy(sector => Td0Layout.BaseSectorSize << sector.SizeCode).OrderByDescending(group => group.Count()).First().Key;
        var logicalSectors = sectors.Where(sector => (Td0Layout.BaseSectorSize << sector.SizeCode) == blockSize).ToArray();
        var cylinders = tracks.Max(track => track.Cylinder) + 1;
        var heads = tracks.Max(track => track.Head & Td0Layout.HeadMask) + 1;
        var sectorsPerTrack = tracks.Max(track => track.Sectors.Count);
        var ordered = logicalSectors.OrderBy(sector => sector.Cylinder).ThenBy(sector => sector.Head & Td0Layout.HeadMask).ThenBy(sector => sector.Number).ToArray();
        var blocks = ordered.Select((sector, logical) => (sector, logical)).Where(item => item.sector.Data is not null).Select(item => new SectorBlock(item.logical, new(item.sector.Cylinder, item.sector.Head & Td0Layout.HeadMask, item.sector.Number), item.sector.Data!, (((Td0SectorFlags)item.sector.Flags) & Td0SectorFlags.DataCrcError) == 0, DiagnosticCode: item.sector.Flags)).ToArray();
        var formatId = Td0SectorImageClassifier.Detect(blocks, blockSize, cylinders, heads, sectorsPerTrack);
        return new(formatId, blockSize, cylinders, heads, sectorsPerTrack, blocks, capacity: ordered.LongLength * blockSize, logicalBlockCount: ordered.Length);
    }

    private static void EnsureAvailable(ReadOnlySpan<byte> data, int offset, int count, Td0Section section)
    {
        if (offset < 0 || count < 0 || offset > data.Length - count) throw Td0Exceptions.Truncated(section, offset, count, Math.Max(0, data.Length - offset));
    }

    private static ushort ReadUInt16(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(data[offset..]);
}
