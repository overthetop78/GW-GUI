using System.Buffers.Binary;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.TeleDisk;

/// <summary>Écrit des conteneurs TeleDisk non compressés en conservant les enregistrements détaillés.</summary>
public sealed class Td0Writer
{
    /// <summary>Écrit atomiquement une image sectorielle avec une carte TeleDisk standard.</summary>
    public Task WriteAsync(SectorImage image, string path, CancellationToken cancellationToken = default) => WriteAsync(CreateDetailedImage(image), path, cancellationToken);

    /// <summary>Écrit atomiquement une image TeleDisk détaillée.</summary>
    public async Task WriteAsync(Td0Image image, string path, CancellationToken cancellationToken = default)
    {
        var bytes = Build(image, cancellationToken);
        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath) ?? Directory.GetCurrentDirectory();
        Directory.CreateDirectory(directory);
        var temporaryPath = Path.Combine(directory, $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            await File.WriteAllBytesAsync(temporaryPath, bytes, cancellationToken).ConfigureAwait(false);
            File.Move(temporaryPath, fullPath, true);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }

    internal static byte[] Build(Td0Image image, CancellationToken cancellationToken = default)
    {
        if (image.Tracks.Count == 0 || image.Tracks.Any(track => track.Sectors.Count > byte.MaxValue)) throw new InvalidDataException("The TeleDisk track map is not representable.");
        var output = new List<byte>();
        WriteHeader(output, image.Header, image.Comment is not null);
        if (image.Comment is not null) WriteComment(output, image.Comment);
        foreach (var track in image.Tracks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            WriteTrack(output, track);
        }
        output.Add(Td0Layout.EndOfTracks);
        return output.ToArray();
    }

    private static void WriteHeader(List<byte> output, Td0Header header, bool hasComment)
    {
        var bytes = new byte[Td0Layout.HeaderSize];
        Td0Format.UncompressedSignature.CopyTo(bytes);
        bytes[Td0Layout.SequenceOffset] = header.Sequence;
        bytes[Td0Layout.CheckSignatureOffset] = header.CheckSignature;
        bytes[Td0Layout.VersionOffset] = header.Version;
        bytes[Td0Layout.DataRateOffset] = header.DataRate;
        bytes[Td0Layout.DriveTypeOffset] = header.DriveType;
        bytes[Td0Layout.SteppingOffset] = hasComment ? (byte)(header.TrackDensity | Td0Layout.CommentPresentMask) : (byte)(header.TrackDensity & ~Td0Layout.CommentPresentMask);
        bytes[Td0Layout.DosModeOffset] = header.DosMode;
        bytes[Td0Layout.SurfaceCountOffset] = header.Surfaces;
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(Td0Layout.HeaderCrcOffset), Td0Crc16.Compute(bytes.AsSpan(0, Td0Layout.HeaderCrcOffset)));
        output.AddRange(bytes);
    }

    private static void WriteComment(List<byte> output, Td0Comment comment)
    {
        if (comment.Data.Count > ushort.MaxValue) throw new InvalidDataException("The TeleDisk comment is too long.");
        var header = new byte[Td0Layout.CommentHeaderSize];
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(Td0Layout.CommentLengthOffset), checked((ushort)comment.Data.Count));
        header[Td0Layout.CommentYearOffset] = comment.Year;
        header[Td0Layout.CommentMonthOffset] = comment.Month;
        header[Td0Layout.CommentDayOffset] = comment.Day;
        header[Td0Layout.CommentHourOffset] = comment.Hour;
        header[Td0Layout.CommentMinuteOffset] = comment.Minute;
        header[Td0Layout.CommentSecondOffset] = comment.Second;
        var crc = Td0Crc16.Compute(header.AsSpan(Td0Layout.CommentLengthOffset));
        crc = Td0Crc16.Compute(comment.Data.ToArray(), crc);
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(Td0Layout.CommentCrcOffset), crc);
        output.AddRange(header);
        output.AddRange(comment.Data);
    }

    private static void WriteTrack(List<byte> output, Td0Track track)
    {
        var header = new byte[Td0Layout.TrackHeaderSize];
        header[Td0Layout.TrackSectorCountOffset] = checked((byte)track.Sectors.Count);
        header[Td0Layout.TrackCylinderOffset] = track.Cylinder;
        header[Td0Layout.TrackHeadOffset] = track.Head;
        header[Td0Layout.TrackCrcOffset] = (byte)Td0Crc16.Compute(header.AsSpan(0, Td0Layout.TrackCrcOffset));
        output.AddRange(header);
        foreach (var sector in track.Sectors) WriteSector(output, sector);
    }

    private static void WriteSector(List<byte> output, Td0Sector sector)
    {
        if (sector.SizeCode > Td0Layout.MaximumSectorSizeCode) throw new InvalidDataException($"TeleDisk sector {sector.Cylinder}/{sector.Head}/{sector.Number} has invalid size code {sector.SizeCode}.");
        var expectedLength = Td0Layout.BaseSectorSize << sector.SizeCode;
        var unavailable = (((Td0SectorFlags)sector.Flags) & Td0SectorFlags.UnavailableMask) != 0;
        if (unavailable != (sector.Data is null)) throw new InvalidDataException($"TeleDisk sector {sector.Cylinder}/{sector.Head}/{sector.Number} has inconsistent availability flags and data.");
        if (sector.Data is not null && sector.Data.Count != expectedLength) throw new InvalidDataException($"TeleDisk sector {sector.Cylinder}/{sector.Head}/{sector.Number} has {sector.Data.Count} bytes instead of {expectedLength}.");
        var data = sector.Data?.ToArray() ?? new byte[expectedLength];
        output.Add(sector.Cylinder);
        output.Add(sector.Head);
        output.Add(sector.Number);
        output.Add(sector.SizeCode);
        output.Add(sector.Flags);
        output.Add((byte)Td0Crc16.Compute(data));
        if (sector.Data is null) return;
        var encoded = Td0SectorEncoder.Encode(data);
        var encodedLength = checked((ushort)(encoded.Payload.Count + Td0Layout.EncodingFieldSize));
        output.Add((byte)encodedLength);
        output.Add((byte)(encodedLength >> 8));
        output.Add((byte)encoded.Encoding);
        output.AddRange(encoded.Payload);
    }

    private static Td0Image CreateDetailedImage(SectorImage image)
    {
        var sizeCode = GetSizeCode(image.BlockSize);
        var tracks = CreateTracks(image, sizeCode);
        var header = Td0HeaderFactory.Create(image);
        return new(header, null, tracks, image);
    }

    private static Td0Track[] CreateTracks(SectorImage image, byte sizeCode) => image.AvailableBlocks
        .GroupBy(block => (block.Address.Cylinder, block.Address.Head))
        .OrderBy(group => group.Key.Cylinder)
        .ThenBy(group => group.Key.Head)
        .Select(group => new Td0Track(checked((byte)group.Key.Cylinder), checked((byte)group.Key.Head), CreateSectors(group, sizeCode)))
        .ToArray();

    private static Td0Sector[] CreateSectors(IEnumerable<SectorBlock> blocks, byte sizeCode) => blocks
        .OrderBy(block => block.Address.Number)
        .Select(block => new Td0Sector(checked((byte)block.Address.Cylinder), checked((byte)block.Address.Head), checked((byte)block.Address.Number), sizeCode, block.IntegrityValid == false ? (byte)Td0SectorFlags.DataCrcError : (byte)Td0SectorFlags.None, block.Data))
        .ToArray();

    private static byte GetSizeCode(int size)
    {
        for (byte code = 0; code <= Td0Layout.MaximumSectorSizeCode; code++) if ((Td0Layout.BaseSectorSize << code) == size) return code;
        throw new InvalidDataException($"Sector size {size} cannot be represented by TeleDisk.");
    }
}
