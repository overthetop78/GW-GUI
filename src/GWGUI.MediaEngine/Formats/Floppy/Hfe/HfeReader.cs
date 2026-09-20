using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Reading;
using GWGUI.MediaEngine.Recognition;

namespace GWGUI.MediaEngine.Formats.Floppy.Hfe;

/// <summary>Lit et valide les conteneurs HFE version 1 et HFE version 3 avec leurs opcodes de protection.</summary>
public sealed class HfeReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds = new[] { DiskImageFormatIds.RawHfe }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedExtensions = new[] { DiskImageFileExtensions.Hfe }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlyList<ReadOnlyMemory<byte>> SupportedSignatures = [HfeFormat.Signature.ToArray(), HfeFormat.Version3Signature.ToArray()];
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds = new[] { MediaKind.Floppy }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds = new[] { MediaRepresentationKind.Flux }.ToFrozenSet();

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => SupportedExtensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => SupportedSignatures;
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentationKinds;
    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    public async Task<HfeImage> ReadAsync(string path, CancellationToken cancellationToken = default) => Read(await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false));

    async ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null && !SupportedFormatIds.Contains(context.RequestedFormatId)) return false;
        if (context.Length < HfeLayout.BlockSize) return false;
        var header = await context.ReadHeaderAsync(HfeLayout.SignatureLength, cancellationToken).ConfigureAwait(false);
        return IsSupportedSignature(header.Span);
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        var bytes = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
        var image = Read(bytes.ToArray());
        var metadata = HfeMetadataFunctions.Create(image);
        return MediaImageDocumentFactory.CreateFloppyFlux(context.Source, DiskImageFormatIds.RawHfe, HfeProtectedTrackImageAdapter.Create(image), metadata);
    }

    public HfeImage Read(byte[] container)
    {
        if (container.Length < HfeLayout.BlockSize || !IsSupportedSignature(container.AsSpan(HfeLayout.SignatureOffset, HfeLayout.SignatureLength))) throw new InvalidDataException("L'en-tête HFE est absent ou tronqué.");
        var version3 = container.AsSpan(HfeLayout.SignatureOffset, HfeLayout.SignatureLength).SequenceEqual(HfeFormat.Version3Signature);
        var revision = container[HfeLayout.RevisionOffset];
        var expectedRevision = version3 ? HfeFormat.Version3Revision : HfeFormat.Revision;
        if (revision != expectedRevision) throw new NotSupportedException($"La révision HFE {revision} n'est pas prise en charge.");
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
            if (totalLength < HfeFormat.MaximumHeadCount) throw new InvalidDataException($"La longueur de piste HFE {cylinder} est invalide.");
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
                tracks.Add(version3
                    ? DecodeVersion3Track(cylinder, head, packed, bitCellTicks)
                    : new HfeTrack(cylinder, head, HfeBitPacking.Unpack(packed), bitCellTicks));
            }
        }
        return new(revision, cylinders, heads, encoding, bitRate, tracks.AsReadOnly());
    }

    private static bool IsSupportedSignature(ReadOnlySpan<byte> signature)
        => signature.SequenceEqual(HfeFormat.Signature) || signature.SequenceEqual(HfeFormat.Version3Signature);

    private static HfeTrack DecodeVersion3Track(int cylinder, int head, ReadOnlySpan<byte> packed, uint nominalTicks)
    {
        var bits = new List<bool>(packed.Length * HfeFormat.BitsPerByte);
        var timing = new List<TrackTimingSegment>();
        var features = new List<TrackFeature>();
        var intervals = new List<uint>();
        var currentTicks = nominalTicks;
        var timingStart = 0;
        uint intervalTicks = 0;
        uint indexTicks = 0;
        int? pendingIndex = null;
        var bitOffset = 0;

        for (var offset = 0; offset < packed.Length; offset++)
        {
            var value = packed[offset];
            var weak = false;
            if ((value & 0x0f) == 0x0f)
            {
                switch (value)
                {
                    case HfeFormat.Version3NopOpcode:
                        continue;
                    case HfeFormat.Version3IndexOpcode:
                        pendingIndex = bits.Count;
                        continue;
                    case HfeFormat.Version3BitRateOpcode:
                        if (++offset >= packed.Length) continue;
                        var parameter = ReverseBits(packed[offset]);
                        if (parameter == 0) throw new InvalidDataException($"L'opcode de débit HFE v3 de la piste {cylinder}/{head} contient une valeur nulle.");
                        if (bits.Count > timingStart)
                            timing.Add(new(timingStart, bits.Count - timingStart, currentTicks * (double)HfeFormat.TickNanoseconds));
                        currentTicks = checked((uint)Math.Max(1, nominalTicks * (long)parameter / 72));
                        timingStart = bits.Count;
                        continue;
                    case HfeFormat.Version3SkipBitsOpcode:
                        if (++offset >= packed.Length) continue;
                        bitOffset = ReverseBits(packed[offset]) & 7;
                        continue;
                    case HfeFormat.Version3RandomOpcode:
                        weak = true;
                        value = 0x55;
                        break;
                    default:
                        continue;
                }
            }

            var firstBit = bits.Count;
            for (var bit = bitOffset; bit < HfeFormat.BitsPerByte; bit++)
            {
                var set = (value & (1 << bit)) != 0;
                bits.Add(set);
                intervalTicks = checked(intervalTicks + currentTicks);
                indexTicks = checked(indexTicks + currentTicks);
                if (!set) continue;
                intervals.Add(intervalTicks);
                intervalTicks = 0;
            }
            if (weak && bits.Count > firstBit)
                features.Add(new(TrackFeatureKind.WeakRegion, firstBit, bits.Count - firstBit, "HFE v3 random data opcode"));
            bitOffset = 0;
        }

        if (bits.Count == 0 || indexTicks == 0) throw new InvalidDataException($"La piste HFE v3 {cylinder}/{head} ne contient aucune cellule.");
        if (intervalTicks > 0) intervals.Add(intervalTicks);
        if (bits.Count > timingStart)
            timing.Add(new(timingStart, bits.Count - timingStart, currentTicks * (double)HfeFormat.TickNanoseconds));
        if (pendingIndex is int index && index < bits.Count)
            features.Add(new(TrackFeatureKind.IndexMark, index, 1, "HFE v3 index opcode"));
        return new(cylinder, head, bits, nominalTicks, timing, features, new(indexTicks, intervals));
    }

    private static byte ReverseBits(byte value)
    {
        var result = 0;
        for (var bit = 0; bit < HfeFormat.BitsPerByte; bit++)
            result |= ((value >> bit) & 1) << (HfeFormat.BitsPerByte - 1 - bit);
        return (byte)result;
    }
}
