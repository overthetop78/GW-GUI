using System.Buffers.Binary;

namespace GWGUI.MediaEngine.Containers.ImageDisk;

/// <summary>Écrit un conteneur ImageDisk en conservant modes, cartes, tailles et états.</summary>
public sealed class ImdWriter
{
    /// <summary>Valide et écrit atomiquement l'image détaillée.</summary>
    public async Task WriteAsync(ImdImage image, string path, CancellationToken cancellationToken = default)
    {
        var bytes = Build(image);
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

    private static byte[] Build(ImdImage image)
    {
        if (image.Tracks.Count == 0) throw new InvalidDataException("The ImageDisk image contains no tracks.");
        var output = new List<byte>();
        var comment = string.IsNullOrEmpty(image.Comment) ? ImdFormat.DefaultComment : image.Comment;
        if (!comment.StartsWith("IMD", StringComparison.Ordinal)) comment = $"IMD {comment}";
        output.AddRange(System.Text.Encoding.ASCII.GetBytes(comment));
        output.Add(ImdFormat.CommentTerminator);
        foreach (var track in image.Tracks) WriteTrack(output, track);
        return output.ToArray();
    }

    private static void WriteTrack(List<byte> output, ImdTrack track)
    {
        if (!Enum.IsDefined(track.Mode) || track.Sectors.Count is 0 or > byte.MaxValue) throw new InvalidDataException("The ImageDisk track header is not representable.");
        var hasCylinderMap = track.Sectors.Any(sector => sector.Cylinder != track.Cylinder);
        var hasHeadMap = track.Sectors.Any(sector => sector.Head != track.Head);
        var flags = (ImdHeadFlags)(track.Head & (byte)ImdHeadFlags.HeadMask);
        if (hasCylinderMap) flags |= ImdHeadFlags.HasCylinderMap;
        if (hasHeadMap) flags |= ImdHeadFlags.HasHeadMap;
        var commonSizeCode = TryGetCommonSizeCode(track.Sectors, out var sizeCode);
        output.Add((byte)track.Mode);
        output.Add(track.Cylinder);
        output.Add((byte)flags);
        output.Add(checked((byte)track.Sectors.Count));
        output.Add(commonSizeCode ? sizeCode : ImdLayout.ExplicitSectorSizeCode);
        output.AddRange(track.Sectors.Select(sector => sector.Number));
        if (hasCylinderMap) output.AddRange(track.Sectors.Select(sector => sector.Cylinder));
        if (hasHeadMap) output.AddRange(track.Sectors.Select(sector => sector.Head));
        if (!commonSizeCode)
        {
            foreach (var sector in track.Sectors)
            {
                var size = new byte[ImdLayout.SectorSizeMapEntrySize];
                BinaryPrimitives.WriteUInt16LittleEndian(size, checked((ushort)sector.Size));
                output.AddRange(size);
            }
        }
        foreach (var sector in track.Sectors) WriteSector(output, sector);
    }

    private static void WriteSector(List<byte> output, ImdSector sector)
    {
        if (!Enum.IsDefined(sector.RecordType) || sector.Data.Count != sector.Size) throw new InvalidDataException("An ImageDisk sector record is invalid.");
        output.Add((byte)sector.RecordType);
        if (!sector.RecordType.HasData()) return;
        if (sector.RecordType.IsCompressed())
        {
            if (sector.Data.Count == 0 || sector.Data.Any(value => value != sector.Data[0])) throw new InvalidDataException("A compressed ImageDisk sector does not contain one repeated byte.");
            output.Add(sector.Data[0]);
            return;
        }
        output.AddRange(sector.Data);
    }

    private static bool TryGetCommonSizeCode(IReadOnlyList<ImdSector> sectors, out byte sizeCode)
    {
        sizeCode = 0;
        if (sectors.Select(sector => sector.Size).Distinct().Count() != 1) return false;
        var size = sectors[0].Size;
        for (byte code = 0; code <= ImdLayout.MaximumExponentialSizeCode; code++)
        {
            if ((ImdLayout.BaseSectorSize << code) != size) continue;
            sizeCode = code;
            return true;
        }
        return false;
    }
}
