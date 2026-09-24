using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Images.Reading.Sources;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Models.Sequential;

namespace GWGUI.MediaEngine.Images.Formats.Tape.SpectrumTap;

/// <summary>Reads a structurally complete Spectrum TAP stream and validates every block checksum.</summary>
public sealed class SpectrumTapReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormats =
        new[] { TapeImageFormatIds.SpectrumTap }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds = new[] { MediaKind.Tape }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentations =
        new[] { MediaRepresentationKind.Sequential }.ToFrozenSet();

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormats;
    IReadOnlySet<string> IMediaImageReader.Extensions => SpectrumTapFormat.Extensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => [];
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentations;
    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormats.Contains(formatId);

    async ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null && !SupportedFormats.Contains(context.RequestedFormatId)) return false;
        if (!Path.GetExtension(context.Source.PrimaryPath).Equals(DiskImageFileExtensions.Tap, StringComparison.OrdinalIgnoreCase)) return false;
        try
        {
            return await ValidateAsync(context.Source.PrimaryPath, requireChecksums: true, cancellationToken).ConfigureAwait(false) > 0;
        }
        catch (InvalidDataException)
        {
            return false;
        }
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        var blockCount = await ValidateAsync(context.Source.PrimaryPath, requireChecksums: false, cancellationToken).ConfigureAwait(false);
        if (blockCount == 0) throw new InvalidDataException("The Spectrum TAP image contains no block.");
        var source = new FileRandomAccessData(context.Source.PrimaryPath);
        await using var input = new FileStream(context.Source.PrimaryPath, FileMode.Open, FileAccess.Read, FileShare.Read,
            64 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
        var segments = new List<SequentialMediaSegment>(blockCount);
        var diagnostics = new List<string>();
        var lengthBytes = new byte[SpectrumTapConstants.LengthFieldSize];
        var index = 0L;
        while (input.Position < input.Length)
        {
            await input.ReadExactlyAsync(lengthBytes, cancellationToken).ConfigureAwait(false);
            var length = BinaryPrimitives.ReadUInt16LittleEndian(lengthBytes);
            var dataOffset = input.Position;
            var data = new byte[length];
            await input.ReadExactlyAsync(data, cancellationToken).ConfigureAwait(false);
            var checksumValid = Xor(data) == 0;
            if (!checksumValid) diagnostics.Add($"Spectrum TAP block {index} has an invalid XOR checksum.");
            segments.Add(new SequentialMediaSegment(
                index,
                SequentialSegmentKind.DataBlock,
                length,
                direction: SequentialTravelDirection.Forward,
                dataRange: new MediaDataRange(dataOffset, length, MediaDataRangeKind.Stored, source, dataOffset),
                metadata: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [SpectrumTapConstants.BlockMetadataKey] = bool.TrueString,
                    ["flag"] = data[0].ToString("x2", System.Globalization.CultureInfo.InvariantCulture),
                    ["checksumValid"] = checksumValid.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["header"] = (length == SpectrumTapConstants.HeaderBlockLength && data[0] == SpectrumTapConstants.HeaderFlag)
                        .ToString(System.Globalization.CultureInfo.InvariantCulture)
                }));
            index++;
        }

        return new MediaImageDocument(
            context.Source,
            TapeImageFormatIds.SpectrumTap,
            MediaKind.Tape,
            new SequentialMediaImageRepresentation(context.Length, segments: segments),
            [],
            diagnostics,
            new Dictionary<string, string>(StringComparer.Ordinal) { ["timing"] = "not-stored" });
    }

    private static async Task<int> ValidateAsync(string path, bool requireChecksums, CancellationToken cancellationToken)
    {
        await using var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            64 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
        var lengthBytes = new byte[SpectrumTapConstants.LengthFieldSize];
        var buffer = new byte[64 * 1024];
        var blocks = 0;
        while (input.Position < input.Length)
        {
            if (input.Length - input.Position < SpectrumTapConstants.LengthFieldSize)
                throw new InvalidDataException("The final Spectrum TAP length is truncated.");
            await input.ReadExactlyAsync(lengthBytes, cancellationToken).ConfigureAwait(false);
            var length = BinaryPrimitives.ReadUInt16LittleEndian(lengthBytes);
            if (length < 2 || length > input.Length - input.Position)
                throw new InvalidDataException("A Spectrum TAP block length is invalid.");
            byte checksum = 0;
            var remaining = (int)length;
            while (remaining > 0)
            {
                var count = Math.Min(buffer.Length, remaining);
                await input.ReadExactlyAsync(buffer.AsMemory(0, count), cancellationToken).ConfigureAwait(false);
                for (var index = 0; index < count; index++) checksum ^= buffer[index];
                remaining -= count;
            }
            if (requireChecksums && checksum != 0) return 0;
            blocks++;
        }
        return blocks;
    }

    private static byte Xor(ReadOnlySpan<byte> data)
    {
        byte result = 0;
        foreach (var value in data) result ^= value;
        return result;
    }
}
