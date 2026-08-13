using System.Buffers.Binary;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;

namespace GWGUI.MediaEngine.Containers.Hfe;

/// <summary>Écrit des pistes FM ou MFM uniformes dans un conteneur HFE version 1.</summary>
public sealed class HfeWriter
{
    public async Task WriteAsync(IReadOnlyList<EncodedDiskTrack> tracks, string path, CancellationToken cancellationToken = default)
    {
        var bytes = Build(tracks);
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

    internal static byte[] Build(IReadOnlyList<EncodedDiskTrack> tracks)
    {
        if (tracks.Count == 0) throw new InvalidDataException("HFE exige au moins une piste encodée.");
        var encoding = ResolveEncoding(tracks);
        var bitRates = tracks.Select(track => BitRate(track.BitCellTicks)).Distinct().ToArray();
        if (bitRates.Length != 1) throw new NotSupportedException("HFE version 1 exige un bitrate uniforme sur toutes les pistes.");
        var cylinders = checked(tracks.Max(track => track.Cylinder) + 1);
        var heads = checked(tracks.Max(track => track.Head) + 1);
        if (cylinders > byte.MaxValue || heads is < 1 or > HfeFormat.MaximumHeadCount) throw new InvalidDataException("La géométrie HFE dépasse les limites du conteneur.");
        var trackListBlockCount = Math.Max(1, (cylinders * HfeLayout.TrackListEntrySize + HfeLayout.BlockSize - 1) / HfeLayout.BlockSize);
        var trackList = Enumerable.Repeat(HfeFormat.HeaderPadding, trackListBlockCount * HfeLayout.BlockSize).ToArray();
        var trackData = new List<byte>();
        var trackDataBlock = 1 + trackListBlockCount;
        for (var cylinder = 0; cylinder < cylinders; cylinder++)
        {
            var sides = Enumerable.Range(0, heads).Select(head => tracks.SingleOrDefault(track => track.Cylinder == cylinder && track.Head == head)).ToArray();
            if (sides.All(track => track is null)) throw new InvalidDataException($"La piste HFE {cylinder} est absente.");
            var packed = sides.Select(track => track is null ? [] : HfeBitPacking.Pack(track.Track.Bits)).ToArray();
            var sideLength = packed.Max(bytes => bytes.Length);
            if (sideLength == 0 || sideLength * HfeFormat.MaximumHeadCount > ushort.MaxValue) throw new InvalidDataException($"La piste HFE {cylinder} possède une longueur invalide.");
            BinaryPrimitives.WriteUInt16LittleEndian(trackList.AsSpan(cylinder * HfeLayout.TrackListEntrySize + HfeLayout.TrackOffsetOffset), checked((ushort)trackDataBlock));
            BinaryPrimitives.WriteUInt16LittleEndian(trackList.AsSpan(cylinder * HfeLayout.TrackListEntrySize + HfeLayout.TrackLengthOffset), checked((ushort)(sideLength * HfeFormat.MaximumHeadCount)));
            var sideBlockCount = (sideLength + HfeLayout.SideChunkSize - 1) / HfeLayout.SideChunkSize;
            for (var block = 0; block < sideBlockCount; block++)
            {
                for (var head = 0; head < HfeFormat.MaximumHeadCount; head++)
                {
                    var offset = block * HfeLayout.SideChunkSize;
                    var count = head < packed.Length ? Math.Min(HfeLayout.SideChunkSize, Math.Max(0, packed[head].Length - offset)) : 0;
                    if (count > 0) trackData.AddRange(packed[head].AsSpan(offset, count).ToArray());
                    trackData.AddRange(Enumerable.Repeat(HfeFormat.TrackPadding, HfeLayout.SideChunkSize - count));
                }
            }
            trackDataBlock += sideBlockCount;
        }
        var header = Enumerable.Repeat(HfeFormat.HeaderPadding, HfeLayout.BlockSize).ToArray();
        HfeFormat.Signature.CopyTo(header.AsSpan(HfeLayout.SignatureOffset));
        header[HfeLayout.RevisionOffset] = HfeFormat.Revision;
        header[HfeLayout.CylinderCountOffset] = checked((byte)cylinders);
        header[HfeLayout.HeadCountOffset] = checked((byte)heads);
        header[HfeLayout.EncodingOffset] = encoding;
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(HfeLayout.BitRateOffset), bitRates[0]);
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(HfeLayout.RpmOffset), HfeFormat.UnspecifiedRpm);
        header[HfeLayout.InterfaceModeOffset] = HfeFormat.UnknownInterfaceMode;
        header[HfeLayout.WriteProtectedOffset] = HfeFormat.WriteProtected;
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(HfeLayout.TrackListOffset), 1);
        header[HfeLayout.WriteAllowedOffset] = HfeFormat.WriteAllowed;
        header[HfeLayout.SingleStepOffset] = HfeFormat.SingleStep;
        return header.Concat(trackList).Concat(trackData).ToArray();
    }

    private static byte ResolveEncoding(IEnumerable<EncodedDiskTrack> tracks)
    {
        var ids = tracks.Select(track => track.Track.EncoderId).Distinct(StringComparer.Ordinal).ToArray();
        if (ids.Length != 1) throw new NotSupportedException("HFE version 1 exige un encodage uniforme sur toutes les pistes.");
        return ids[0] switch { FluxCodecIds.IsoMfm => HfeFormat.IsoMfmEncoding, FluxCodecIds.IsoFm => HfeFormat.IsoFmEncoding, _ => throw new NotSupportedException($"L'encodage {ids[0]} n'est pas encore pris en charge par le Writer HFE.") };
    }

    private static ushort BitRate(uint bitCellTicks)
    {
        if (bitCellTicks == 0) throw new InvalidDataException("La durée de cellule HFE ne peut pas être nulle.");
        var value = HfeFormat.NanosecondsPerSecond / (HfeFormat.BitsPerDataBit * HfeFormat.TickNanoseconds * 1000L * bitCellTicks);
        if (value is < 1 or > ushort.MaxValue) throw new InvalidDataException($"Le bitrate HFE calculé ({value} kbit/s) est invalide.");
        return checked((ushort)value);
    }
}
