using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Globalization;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Reading.Sources;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Representations.Sequential;

namespace GWGUI.MediaEngine.Formats.Tape.CommodoreTap;

/// <summary>Reads Commodore TAP timing records as pulses while preserving their exact source ranges.</summary>
public sealed class CommodoreTapReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { TapeImageFormatIds.CommodoreTap }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds = new[] { MediaKind.Tape }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds =
        new[] { MediaRepresentationKind.Sequential }.ToFrozenSet();
    private static readonly IReadOnlyList<ReadOnlyMemory<byte>> Signatures =
    [
        System.Text.Encoding.ASCII.GetBytes(CommodoreTapConstants.C64Signature),
        System.Text.Encoding.ASCII.GetBytes(CommodoreTapConstants.C16Signature)
    ];

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => CommodoreTapFormat.Extensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => Signatures;
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentationKinds;
    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    async ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null && !SupportedFormatIds.Contains(context.RequestedFormatId)) return false;
        try
        {
            await ReadHeaderAsync(context.Source.PrimaryPath, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (InvalidDataException)
        {
            return false;
        }
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        var header = await ReadHeaderAsync(context.Source.PrimaryPath, cancellationToken).ConfigureAwait(false);
        var source = new FileRandomAccessData(context.Source.PrimaryPath);
        await using var input = new FileStream(context.Source.PrimaryPath, FileMode.Open, FileAccess.Read, FileShare.Read,
            64 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
        input.Position = CommodoreTapConstants.HeaderSize;

        var segments = new List<SequentialMediaSegment>();
        var elapsed = TimeSpan.Zero;
        var position = 0L;
        var extended = new byte[CommodoreTapConstants.ExtendedPulseByteCount];
        while (input.Position < input.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var encodedOffset = input.Position;
            var first = input.ReadByte();
            if (first < 0) break;
            var encodedLength = 1;
            int cycles;
            if (first != 0)
            {
                cycles = first * CommodoreTapConstants.ShortPulseCycleMultiplier;
            }
            else if (header.Version == CommodoreTapConstants.OriginalVersion)
            {
                cycles = CommodoreTapConstants.Version0OverflowCycles;
            }
            else
            {
                if (input.Length - input.Position < CommodoreTapConstants.ExtendedPulseByteCount)
                    throw new InvalidDataException("The final Commodore TAP extended pulse is truncated.");
                await input.ReadExactlyAsync(extended, cancellationToken).ConfigureAwait(false);
                cycles = extended[0] | extended[1] << 8 | extended[2] << 16;
                encodedLength += CommodoreTapConstants.ExtendedPulseByteCount;
                if (cycles == 0) throw new InvalidDataException("A Commodore TAP extended pulse has zero duration.");
            }

            var duration = TimeSpan.FromSeconds((double)cycles / header.ClockRate);
            segments.Add(new SequentialMediaSegment(
                position++,
                SequentialSegmentKind.Pulse,
                cycles,
                elapsed,
                duration,
                direction: SequentialTravelDirection.Forward,
                dataRange: new MediaDataRange(encodedOffset, encodedLength, MediaDataRangeKind.Stored, source, encodedOffset),
                metadata: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [CommodoreTapConstants.PulseMetadataKey] = bool.TrueString,
                    [CommodoreTapConstants.CycleCountMetadataKey] = cycles.ToString(CultureInfo.InvariantCulture),
                    [CommodoreTapConstants.EncodedLengthMetadataKey] = encodedLength.ToString(CultureInfo.InvariantCulture),
                    [CommodoreTapConstants.HalfWaveMetadataKey] =
                        (header.Version == CommodoreTapConstants.HalfWaveVersion).ToString(CultureInfo.InvariantCulture)
                }));
            elapsed += duration;
        }

        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [CommodoreTapConstants.SignatureMetadataKey] = header.Signature,
            [CommodoreTapConstants.VersionMetadataKey] = header.Version.ToString(CultureInfo.InvariantCulture),
            [CommodoreTapConstants.PlatformMetadataKey] = header.Platform.ToString(CultureInfo.InvariantCulture),
            [CommodoreTapConstants.VideoStandardMetadataKey] = header.VideoStandard.ToString(CultureInfo.InvariantCulture),
            [CommodoreTapConstants.ClockRateMetadataKey] = header.ClockRate.ToString(CultureInfo.InvariantCulture)
        };
        return new MediaImageDocument(
            context.Source,
            TapeImageFormatIds.CommodoreTap,
            MediaKind.Tape,
            new SequentialMediaImageRepresentation(context.Length, elapsed, segments),
            [],
            [],
            metadata);
    }

    private static async Task<Header> ReadHeaderAsync(string path, CancellationToken cancellationToken)
    {
        await using var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            CommodoreTapConstants.HeaderSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
        if (input.Length < CommodoreTapConstants.HeaderSize)
            throw new InvalidDataException("The Commodore TAP header is truncated.");
        var bytes = new byte[CommodoreTapConstants.HeaderSize];
        await input.ReadExactlyAsync(bytes, cancellationToken).ConfigureAwait(false);
        var signature = System.Text.Encoding.ASCII.GetString(
            bytes,
            CommodoreTapConstants.SignatureOffset,
            CommodoreTapConstants.SignatureLength);
        if (!CommodoreTapFormat.Signatures.Contains(signature))
            throw new InvalidDataException("The Commodore TAP signature is invalid.");
        var version = bytes[CommodoreTapConstants.VersionOffset];
        if (!CommodoreTapFormat.Versions.Contains(version))
            throw new InvalidDataException($"Commodore TAP version {version} is unsupported.");
        var platform = bytes[CommodoreTapConstants.PlatformOffset];
        var videoStandard = bytes[CommodoreTapConstants.VideoStandardOffset];
        if (!CommodoreTapConstants.TimerClockRates.TryGetValue((platform, videoStandard), out var clockRate))
            throw new InvalidDataException("The Commodore TAP platform and video standard combination is unsupported.");
        var declaredDataLength = BinaryPrimitives.ReadUInt32LittleEndian(
            bytes.AsSpan(CommodoreTapConstants.DataLengthOffset, CommodoreTapConstants.DataLengthSize));
        if (declaredDataLength != input.Length - CommodoreTapConstants.HeaderSize)
            throw new InvalidDataException("The Commodore TAP data length does not match its header.");
        return new Header(signature, version, platform, videoStandard, clockRate);
    }

    private readonly record struct Header(string Signature, byte Version, byte Platform, byte VideoStandard, int ClockRate);
}
