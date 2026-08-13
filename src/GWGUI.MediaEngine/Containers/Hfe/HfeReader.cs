using System.Buffers.Binary;

namespace GWGUI.MediaEngine.Containers.Hfe;

/// <summary>Lit et valide les conteneurs HFE version 1 à timing uniforme.</summary>
public sealed class HfeReader
{
    public async Task<HfeImage> ReadAsync(string path, CancellationToken cancellationToken = default) => Read(await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false));

    public HfeImage Read(byte[] container)
    {
        if (container.Length < HfeLayout.BlockSize || !container.AsSpan(HfeLayout.SignatureOffset, HfeLayout.SignatureLength).SequenceEqual(HfeFormat.Signature)) throw new InvalidDataException("L'en-tête HFE est absent ou tronqué.");
        var revision = container[HfeLayout.RevisionOffset];
        if (revision != HfeFormat.Revision) throw new NotSupportedException($"La révision HFE {revision} n'est pas prise en charge.");
        var cylinders = container[HfeLayout.CylinderCountOffset];
        var heads = container[HfeLayout.HeadCountOffset];
        var encoding = container[HfeLayout.EncodingOffset];
        var bitRate = BinaryPrimitives.ReadUInt16LittleEndian(container.AsSpan(HfeLayout.BitRateOffset));
        if (cylinders == 0 || heads is < 1 or > HfeFormat.MaximumHeadCount || bitRate == 0) throw new InvalidDataException("La géométrie ou le bitrate HFE est invalide.");
        var trackListOffset = BinaryPrimitives.ReadUInt16LittleEndian(container.AsSpan(HfeLayout.TrackListOffset)) * HfeLayout.BlockSize;
        if (trackListOffset < HfeLayout.BlockSize || trackListOffset + cylinders * HfeLayout.TrackListEntrySize > container.Length) throw new InvalidDataException("La table de pistes HFE est hors limites.");
        var calculatedBitCellTicks = HfeFormat.NanosecondsPerSecond / (HfeFormat.BitsPerDataBit * HfeFormat.TickNanoseconds * 1000L * bitRate);
        if (calculatedBitCellTicks == 0) throw new InvalidDataException("Le bitrate HFE ne peut pas être représenté avec la résolution temporelle interne.");
        var bitCellTicks = checked((uint)calculatedBitCellTicks);
        var tracks = new List<HfeTrack>(cylinders * heads);
        for (var cylinder = 0; cylinder < cylinders; cylinder++)
        {
            var entry = container.AsSpan(trackListOffset + cylinder * HfeLayout.TrackListEntrySize, HfeLayout.TrackListEntrySize);
            var dataBlock = BinaryPrimitives.ReadUInt16LittleEndian(entry[HfeLayout.TrackOffsetOffset..]);
            var totalLength = BinaryPrimitives.ReadUInt16LittleEndian(entry[HfeLayout.TrackLengthOffset..]);
            if (totalLength == 0 || totalLength % HfeFormat.MaximumHeadCount != 0) throw new InvalidDataException($"La longueur de piste HFE {cylinder} est invalide.");
            var sideLength = totalLength / HfeFormat.MaximumHeadCount;
            for (var head = 0; head < heads; head++)
            {
                var packed = new byte[sideLength];
                var remaining = sideLength;
                var destination = 0;
                var block = dataBlock;
                while (remaining > 0)
                {
                    var count = Math.Min(remaining, HfeLayout.SideChunkSize);
                    var source = block * HfeLayout.BlockSize + head * HfeLayout.SideChunkSize;
                    if (source < 0 || source + count > container.Length) throw new InvalidDataException($"Les données de piste HFE {cylinder}/{head} sont hors limites.");
                    container.AsSpan(source, count).CopyTo(packed.AsSpan(destination));
                    destination += count;
                    remaining -= count;
                    block++;
                }
                tracks.Add(new(cylinder, head, HfeBitPacking.Unpack(packed), bitCellTicks));
            }
        }
        return new(revision, cylinders, heads, encoding, bitRate, tracks.AsReadOnly());
    }
}
