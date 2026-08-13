using System.Buffers.Binary;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;

namespace GWGUI.MediaEngine.Containers.Hfe;

/// <summary>Écrit des pistes FM ou MFM uniformes dans un conteneur HFE version 1.</summary>
public sealed class HfeWriter
{
    public Task WriteAsync(
        IReadOnlyList<EncodedDiskTrack> tracks,
        string path,
        CancellationToken cancellationToken = default)
    {
        return WriteBytesAsync(Build(tracks), path, cancellationToken);
    }

    public Task WriteAsync(
        HfeImage image,
        string path,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(image);
        return WriteBytesAsync(Build(image), path, cancellationToken);
    }

    internal static byte[] Build(IReadOnlyList<EncodedDiskTrack> tracks)
    {
        if (tracks.Count == 0)
            throw new InvalidDataException("HFE exige au moins une piste encodée.");
        var encoding = ResolveEncoding(tracks);
        var bitRates = tracks.Select(track => BitRate(track.BitCellTicks)).Distinct().ToArray();
        if (bitRates.Length != 1)
            throw new NotSupportedException("HFE version 1 exige un bitrate uniforme sur toutes les pistes.");
        var cylinders = checked(tracks.Max(track => track.Cylinder) + 1);
        var heads = checked(tracks.Max(track => track.Head) + 1);
        var hfeTracks = tracks
            .Select(track => new HfeTrack(track.Cylinder, track.Head, track.Track.Bits, track.BitCellTicks))
            .ToArray();
        return Build(new HfeImage(
            HfeFormat.Revision,
            cylinders,
            heads,
            encoding,
            bitRates[0],
            hfeTracks));
    }

    internal static byte[] Build(HfeImage image)
    {
        Validate(image);
        var trackListBlockCount = Math.Max(
            1,
            (image.Cylinders * HfeLayout.TrackListEntrySize + HfeLayout.BlockSize - 1) /
            HfeLayout.BlockSize);
        var trackList = Enumerable
            .Repeat(HfeFormat.HeaderPadding, trackListBlockCount * HfeLayout.BlockSize)
            .ToArray();
        var trackData = new List<byte>();
        var trackDataBlock = 1 + trackListBlockCount;
        for (var cylinder = 0; cylinder < image.Cylinders; cylinder++)
        {
            var sides = Enumerable.Range(0, image.Heads)
                .Select(head => image.Tracks.SingleOrDefault(
                    track => track.Cylinder == cylinder && track.Head == head))
                .ToArray();
            if (sides.All(track => track is null))
                throw new InvalidDataException($"La piste HFE {cylinder} est absente.");
            var packed = sides
                .Select(track => track is null ? [] : HfeBitPacking.Pack(track.Bits))
                .ToArray();
            var sideLength = packed.Max(bytes => bytes.Length);
            if (sideLength == 0 || sideLength * HfeFormat.MaximumHeadCount > ushort.MaxValue)
                throw new InvalidDataException($"La piste HFE {cylinder} possède une longueur invalide.");
            var entryOffset = cylinder * HfeLayout.TrackListEntrySize;
            BinaryPrimitives.WriteUInt16LittleEndian(
                trackList.AsSpan(entryOffset + HfeLayout.TrackOffsetOffset),
                checked((ushort)trackDataBlock));
            BinaryPrimitives.WriteUInt16LittleEndian(
                trackList.AsSpan(entryOffset + HfeLayout.TrackLengthOffset),
                checked((ushort)(sideLength * HfeFormat.MaximumHeadCount)));
            AppendTrackData(trackData, packed, sideLength);
            trackDataBlock += (sideLength + HfeLayout.SideChunkSize - 1) / HfeLayout.SideChunkSize;
        }
        return BuildHeader(image)
            .Concat(trackList)
            .Concat(trackData)
            .ToArray();
    }

    private static void AppendTrackData(
        ICollection<byte> destination,
        IReadOnlyList<byte[]> packed,
        int sideLength)
    {
        var sideBlockCount = (sideLength + HfeLayout.SideChunkSize - 1) /
            HfeLayout.SideChunkSize;
        for (var block = 0; block < sideBlockCount; block++)
        {
            for (var head = 0; head < HfeFormat.MaximumHeadCount; head++)
            {
                var offset = block * HfeLayout.SideChunkSize;
                var count = head < packed.Count
                    ? Math.Min(HfeLayout.SideChunkSize, Math.Max(0, packed[head].Length - offset))
                    : 0;
                if (count > 0)
                    foreach (var value in packed[head].AsSpan(offset, count))
                        destination.Add(value);
                for (var index = count; index < HfeLayout.SideChunkSize; index++)
                    destination.Add(HfeFormat.TrackPadding);
            }
        }
    }

    private static byte[] BuildHeader(HfeImage image)
    {
        var header = Enumerable.Repeat(HfeFormat.HeaderPadding, HfeLayout.BlockSize).ToArray();
        HfeFormat.Signature.CopyTo(header.AsSpan(HfeLayout.SignatureOffset));
        header[HfeLayout.RevisionOffset] = image.Revision;
        header[HfeLayout.CylinderCountOffset] = checked((byte)image.Cylinders);
        header[HfeLayout.HeadCountOffset] = checked((byte)image.Heads);
        header[HfeLayout.EncodingOffset] = image.Encoding;
        BinaryPrimitives.WriteUInt16LittleEndian(
            header.AsSpan(HfeLayout.BitRateOffset),
            image.BitRate);
        BinaryPrimitives.WriteUInt16LittleEndian(
            header.AsSpan(HfeLayout.RpmOffset),
            HfeFormat.UnspecifiedRpm);
        header[HfeLayout.InterfaceModeOffset] = HfeFormat.UnknownInterfaceMode;
        header[HfeLayout.WriteProtectedOffset] = HfeFormat.WriteProtected;
        BinaryPrimitives.WriteUInt16LittleEndian(
            header.AsSpan(HfeLayout.TrackListOffset),
            1);
        header[HfeLayout.WriteAllowedOffset] = HfeFormat.WriteAllowed;
        header[HfeLayout.SingleStepOffset] = HfeFormat.SingleStep;
        return header;
    }

    private static async Task WriteBytesAsync(
        byte[] bytes,
        string path,
        CancellationToken cancellationToken)
    {
        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath) ?? Directory.GetCurrentDirectory();
        Directory.CreateDirectory(directory);
        var temporaryPath = Path.Combine(
            directory,
            $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            await File.WriteAllBytesAsync(temporaryPath, bytes, cancellationToken)
                .ConfigureAwait(false);
            File.Move(temporaryPath, fullPath, true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }

    private static void Validate(HfeImage image)
    {
        if (image.Revision != HfeFormat.Revision)
            throw new NotSupportedException($"La révision HFE {image.Revision} n'est pas prise en charge.");
        if (image.Cylinders is < 1 or > byte.MaxValue ||
            image.Heads is < 1 or > HfeFormat.MaximumHeadCount)
            throw new InvalidDataException("La géométrie HFE dépasse les limites du conteneur.");
        if (image.BitRate == 0)
            throw new InvalidDataException("Le bitrate HFE est invalide.");
        if (image.Tracks.Count == 0)
            throw new InvalidDataException("HFE exige au moins une piste.");
        if (image.Tracks.Any(track =>
                track.Cylinder < 0 ||
                track.Cylinder >= image.Cylinders ||
                track.Head < 0 ||
                track.Head >= image.Heads))
            throw new InvalidDataException("Une piste HFE est hors de la géométrie déclarée.");
        var duplicate = image.Tracks
            .GroupBy(track => (track.Cylinder, track.Head))
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
            throw new InvalidDataException(
                $"La piste HFE {duplicate.Key.Cylinder}/{duplicate.Key.Head} est dupliquée.");
    }

    private static byte ResolveEncoding(IEnumerable<EncodedDiskTrack> tracks)
    {
        var ids = tracks.Select(track => track.Track.EncoderId)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (ids.Length != 1)
            throw new NotSupportedException("HFE version 1 exige un encodage uniforme sur toutes les pistes.");
        return ids[0] switch
        {
            FluxCodecIds.IsoMfm => HfeFormat.IsoMfmEncoding,
            FluxCodecIds.AmigaMfm => HfeFormat.AmigaMfmEncoding,
            FluxCodecIds.IsoFm => HfeFormat.IsoFmEncoding,
            _ => throw new NotSupportedException(
                $"L'encodage {ids[0]} n'est pas encore pris en charge par le Writer HFE.")
        };
    }

    private static ushort BitRate(uint bitCellTicks)
    {
        if (bitCellTicks == 0)
            throw new InvalidDataException("La durée de cellule HFE ne peut pas être nulle.");
        var value = HfeFormat.NanosecondsPerSecond /
            (HfeFormat.BitsPerDataBit * HfeFormat.TickNanoseconds * 1000L * bitCellTicks);
        if (value is < 1 or > ushort.MaxValue)
            throw new InvalidDataException($"Le bitrate HFE calculé ({value} kbit/s) est invalide.");
        return checked((ushort)value);
    }
}
