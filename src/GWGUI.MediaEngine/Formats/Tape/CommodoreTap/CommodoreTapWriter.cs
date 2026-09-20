using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Globalization;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Interfaces.Writing;
using GWGUI.MediaEngine.Representations.Sequential;

namespace GWGUI.MediaEngine.Formats.Tape.CommodoreTap;

/// <summary>Writes pulse durations that are exactly representable by the selected Commodore TAP version.</summary>
public sealed class CommodoreTapWriter : IMediaImageWriter
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { TapeImageFormatIds.CommodoreTap }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds =
        new[] { MediaRepresentationKind.Sequential }.ToFrozenSet();
    private readonly IAtomicImageFileWriter files;

    public CommodoreTapWriter(IAtomicImageFileWriter? files = null) =>
        this.files = files ?? new AtomicImageFileWriter();

    public string Id => MediaImageWriterIds.TapeCommodoreTap;
    public IReadOnlySet<string> FormatIds => SupportedFormatIds;
    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedRepresentationKinds;
    public IReadOnlySet<string> ProducedFileExtensions => CommodoreTapFormat.Extensions;
    public bool ProducesMultipleFiles => false;

    public bool CanWrite(MediaImageDocument document, string targetFormatId, string targetExtension) =>
        SupportedFormatIds.Contains(targetFormatId)
        && CommodoreTapFormat.Extensions.Contains(targetExtension)
        && document.MediaKind == MediaKind.Tape
        && TryGetProfile(document.Metadata, out var profile)
        && TryEncodeRecords(document, profile.Version, out _);

    public async Task<IReadOnlyList<string>> WriteAsync(
        MediaImageDocument document,
        string outputPath,
        string targetFormatId,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(outputPath).ToLowerInvariant();
        if (!SupportedFormatIds.Contains(targetFormatId)
            || !CommodoreTapFormat.Extensions.Contains(extension)
            || document.MediaKind != MediaKind.Tape
            || !TryGetProfile(document.Metadata, out var profile)
            || !TryEncodeRecords(document, profile.Version, out var records))
            throw new InvalidDataException("The sequential document contains pulses that cannot be represented by the selected Commodore TAP profile.");

        var dataLength = records.Sum(record => (long)record.Length);
        if (dataLength > uint.MaxValue) throw new NotSupportedException("The Commodore TAP pulse stream exceeds its 32-bit data length.");
        await files.WriteAsync(
            outputPath,
            async (output, token) =>
            {
                var header = new byte[CommodoreTapConstants.HeaderSize];
                System.Text.Encoding.ASCII.GetBytes(profile.Signature, header.AsSpan(CommodoreTapConstants.SignatureOffset));
                header[CommodoreTapConstants.VersionOffset] = profile.Version;
                header[CommodoreTapConstants.PlatformOffset] = profile.Platform;
                header[CommodoreTapConstants.VideoStandardOffset] = profile.VideoStandard;
                BinaryPrimitives.WriteUInt32LittleEndian(
                    header.AsSpan(CommodoreTapConstants.DataLengthOffset, CommodoreTapConstants.DataLengthSize),
                    checked((uint)dataLength));
                await output.WriteAsync(header, token).ConfigureAwait(false);
                foreach (var record in records) await output.WriteAsync(record, token).ConfigureAwait(false);
            },
            cancellationToken).ConfigureAwait(false);
        return [outputPath];
    }

    private static bool TryGetProfile(IReadOnlyDictionary<string, string> metadata, out TapProfile profile)
    {
        profile = default;
        if (!metadata.TryGetValue(CommodoreTapConstants.SignatureMetadataKey, out var signature)
            || !CommodoreTapFormat.Signatures.Contains(signature)
            || !TryReadByte(metadata, CommodoreTapConstants.VersionMetadataKey, out var version)
            || !CommodoreTapFormat.Versions.Contains(version)
            || !TryReadByte(metadata, CommodoreTapConstants.PlatformMetadataKey, out var platform)
            || !TryReadByte(metadata, CommodoreTapConstants.VideoStandardMetadataKey, out var videoStandard)
            || !CommodoreTapConstants.TimerClockRates.ContainsKey((platform, videoStandard)))
            return false;
        profile = new TapProfile(signature, version, platform, videoStandard);
        return true;
    }

    private static bool TryEncodeRecords(MediaImageDocument document, byte version, out IReadOnlyList<byte[]> records)
    {
        records = [];
        if (document.Representation is not SequentialMediaImageRepresentation { Segments: { Count: > 0 } segments }) return false;
        var encoded = new List<byte[]>(segments.Count);
        foreach (var segment in segments.OrderBy(segment => segment.Position))
        {
            if (segment.Kind != SequentialSegmentKind.Pulse
                || !segment.Metadata.TryGetValue(CommodoreTapConstants.CycleCountMetadataKey, out var cycleText)
                || !int.TryParse(cycleText, NumberStyles.None, CultureInfo.InvariantCulture, out var cycles)
                || cycles <= 0)
                return false;
            if (cycles % CommodoreTapConstants.ShortPulseCycleMultiplier == 0
                && cycles / CommodoreTapConstants.ShortPulseCycleMultiplier is >= 1 and <= byte.MaxValue)
            {
                encoded.Add([(byte)(cycles / CommodoreTapConstants.ShortPulseCycleMultiplier)]);
                continue;
            }
            if (version == CommodoreTapConstants.OriginalVersion || cycles > 0x00ff_ffff) return false;
            encoded.Add([0, (byte)cycles, (byte)(cycles >> 8), (byte)(cycles >> 16)]);
        }
        records = encoded;
        return true;
    }

    private static bool TryReadByte(IReadOnlyDictionary<string, string> metadata, string key, out byte value)
    {
        value = 0;
        return metadata.TryGetValue(key, out var text)
            && byte.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value);
    }

    private readonly record struct TapProfile(string Signature, byte Version, byte Platform, byte VideoStandard);
}
