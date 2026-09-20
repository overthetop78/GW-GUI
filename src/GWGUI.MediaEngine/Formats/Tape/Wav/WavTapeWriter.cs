using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Interfaces.Writing;
using GWGUI.MediaEngine.Representations.Sequential;
using GWGUI.MediaEngine.Writing;

namespace GWGUI.MediaEngine.Formats.Tape.Wav;

/// <summary>Writes explicit interleaved integer PCM segments without resampling or mixing channels.</summary>
public sealed class WavTapeWriter : IMediaImageWriter
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { TapeImageFormatIds.Wav }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentations =
        new[] { MediaRepresentationKind.Sequential }.ToFrozenSet();
    private readonly IAtomicImageFileWriter files;

    public WavTapeWriter(IAtomicImageFileWriter? files = null) =>
        this.files = files ?? new AtomicImageFileWriter();

    public string Id => MediaImageWriterIds.TapeWav;
    public IReadOnlySet<string> FormatIds => SupportedFormatIds;
    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedRepresentations;
    public IReadOnlySet<string> ProducedFileExtensions => WavTapeFormat.Extensions;
    public bool ProducesMultipleFiles => false;

    public bool CanWrite(MediaImageDocument document, string targetFormatId, string targetExtension)
    {
        ArgumentNullException.ThrowIfNull(document);
        return SupportedFormatIds.Contains(targetFormatId)
            && WavTapeFormat.Extensions.Contains(targetExtension)
            && document.MediaKind == MediaKind.Tape
            && TryGetProfile(document, out _)
            && TryGetDataRanges(document, out _);
    }

    public async Task<IReadOnlyList<string>> WriteAsync(
        MediaImageDocument document,
        string outputPath,
        string targetFormatId,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(outputPath).ToLowerInvariant();
        if (!CanWrite(document, targetFormatId, extension)
            || !TryGetProfile(document, out var profile)
            || !TryGetDataRanges(document, out var ranges))
            throw new InvalidDataException("The sequential document cannot be written as integer PCM WAV without resampling or data loss.");
        var dataLength = ranges.Sum(range => range.Length);
        if (dataLength > uint.MaxValue || dataLength + WavConstants.RiffHeaderSize + WavConstants.ChunkHeaderSize + WavConstants.PcmFormatChunkSize > uint.MaxValue + 8L)
            throw new NotSupportedException("The WAV output exceeds the RIFF 32-bit length limit.");
        await files.WriteAsync(
            outputPath,
            (output, token) => WriteWaveAsync(output, profile, ranges, dataLength, token),
            cancellationToken).ConfigureAwait(false);
        return [outputPath];
    }

    private static async Task WriteWaveAsync(
        Stream output,
        PcmProfile profile,
        IReadOnlyList<MediaDataRange> ranges,
        long dataLength,
        CancellationToken cancellationToken)
    {
        var header = new byte[WavConstants.RiffHeaderSize + WavConstants.ChunkHeaderSize + WavConstants.PcmFormatChunkSize + WavConstants.ChunkHeaderSize];
        WriteIdentifier(header, 0, WavConstants.RiffIdentifier);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(4), checked((uint)(header.Length - 8 + dataLength)));
        WriteIdentifier(header, 8, WavConstants.WaveIdentifier);
        WriteIdentifier(header, 12, WavConstants.FormatChunkIdentifier);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(16), WavConstants.PcmFormatChunkSize);
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(20), WavConstants.PcmFormatTag);
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(22), profile.Channels);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(24), profile.SampleRate);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(28), checked(profile.SampleRate * profile.BlockAlign));
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(32), profile.BlockAlign);
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(34), profile.BitsPerSample);
        WriteIdentifier(header, 36, WavConstants.DataChunkIdentifier);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(40), checked((uint)dataLength));
        await output.WriteAsync(header, cancellationToken).ConfigureAwait(false);
        var buffer = new byte[64 * 1024];
        foreach (var range in ranges)
        {
            var completed = 0L;
            while (completed < range.Length)
            {
                var count = (int)Math.Min(buffer.Length, range.Length - completed);
                await range.Source!.ReadExactlyAsync(range.SourceOffset + completed, buffer.AsMemory(0, count), cancellationToken).ConfigureAwait(false);
                await output.WriteAsync(buffer.AsMemory(0, count), cancellationToken).ConfigureAwait(false);
                completed += count;
            }
        }
        if ((dataLength & 1) != 0) await output.WriteAsync(new byte[1], cancellationToken).ConfigureAwait(false);
    }

    private static bool TryGetProfile(MediaImageDocument document, out PcmProfile profile)
    {
        profile = default;
        if (!TryReadUInt16(document.Metadata, "channels", out var channels)
            || !TryReadUInt32(document.Metadata, "sampleRate", out var sampleRate)
            || !TryReadUInt16(document.Metadata, "bitsPerSample", out var bitsPerSample)
            || !TryReadUInt16(document.Metadata, "blockAlign", out var blockAlign)
            || !WavTapeFormat.IsSupportedPcm(channels, checked((int)sampleRate), bitsPerSample)
            || blockAlign != channels * (bitsPerSample / 8))
            return false;
        profile = new PcmProfile(channels, sampleRate, bitsPerSample, blockAlign);
        return true;
    }

    private static bool TryGetDataRanges(MediaImageDocument document, out IReadOnlyList<MediaDataRange> ranges)
    {
        ranges = [];
        if (document.Representation is not SequentialMediaImageRepresentation { Segments: { } segments }
            || segments.Count == 0
            || segments.Any(segment => segment.Kind != SequentialSegmentKind.Samples || segment.DataRange?.Source is null))
            return false;
        var selected = segments.Where(segment => segment.ChannelNumber is null or 0)
            .Select(segment => segment.DataRange!)
            .ToArray();
        if (selected.Length == 0 || selected.Any(range => range.Kind != MediaDataRangeKind.Stored)) return false;
        ranges = selected;
        return true;
    }

    private static bool TryReadUInt16(IReadOnlyDictionary<string, string> values, string key, out ushort value)
    {
        value = 0;
        return values.TryGetValue(key, out var text)
            && ushort.TryParse(text, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out value);
    }

    private static bool TryReadUInt32(IReadOnlyDictionary<string, string> values, string key, out uint value)
    {
        value = 0;
        return values.TryGetValue(key, out var text)
            && uint.TryParse(text, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out value);
    }

    private static void WriteIdentifier(Span<byte> destination, int offset, string value) =>
        System.Text.Encoding.ASCII.GetBytes(value, destination[offset..]);

    private readonly record struct PcmProfile(ushort Channels, uint SampleRate, ushort BitsPerSample, ushort BlockAlign);
}
