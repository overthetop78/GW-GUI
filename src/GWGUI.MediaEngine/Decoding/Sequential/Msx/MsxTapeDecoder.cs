using System.Collections.Frozen;
using System.Globalization;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Decoding.Sequential.Tzx;

using GWGUI.MediaEngine.Functions;
using GWGUI.MediaEngine.Interfaces.Decoding;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Representations.Sequential;

namespace GWGUI.MediaEngine.Decoding.Sequential.Msx;

/// <summary>Validates MSX byte groups retained in CAS or TSX and decodes standard FSK serial frames from PCM WAV.</summary>
public sealed class MsxTapeDecoder : ISequentialMediaDecoder
{
    private static readonly IReadOnlySet<string> SupportedFormats =
        new[] { TapeImageFormatIds.MsxCas, TapeImageFormatIds.Tzx, TapeImageFormatIds.Wav }
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedMachines =
        new[] { DiskSystemIds.Msx }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public string Id => MsxCasConstants.DecoderId;
    public IReadOnlySet<string> FormatIds => SupportedFormats;
    public IReadOnlySet<string> MachineIds => SupportedMachines;

    public bool CanDecode(MediaImageDocument document, string? machineId = null)
    {
        if (document.MediaKind != MediaKind.Tape
            || !SupportedFormats.Contains(document.FormatId)
            || document.Representation is not SequentialMediaImageRepresentation
            || machineId is not null && !SupportedMachines.Contains(machineId))
            return false;
        return !document.FormatId.Equals(TapeImageFormatIds.Tzx, StringComparison.OrdinalIgnoreCase)
            || machineId is not null
            || document.Metadata.TryGetValue("contextExtension", out var extension)
                && extension.Equals(DiskImageFileExtensions.Tsx, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<SequentialDecodeResult> DecodeAsync(
        MediaImageDocument document,
        string? machineId = null,
        CancellationToken cancellationToken = default)
    {
        if (!CanDecode(document, machineId))
            throw new InvalidDataException("The media document has no explicit MSX cassette context.");
        var representation = (SequentialMediaImageRepresentation)document.Representation;
        if (document.FormatId.Equals(TapeImageFormatIds.MsxCas, StringComparison.OrdinalIgnoreCase))
            return await DecodeCasAsync(representation, cancellationToken).ConfigureAwait(false);
        if (document.FormatId.Equals(TapeImageFormatIds.Tzx, StringComparison.OrdinalIgnoreCase))
            return await DecodeTsxAsync(document, representation, cancellationToken).ConfigureAwait(false);
        return await DecodeWaveAsync(document, representation, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<SequentialDecodeResult> DecodeCasAsync(
        SequentialMediaImageRepresentation representation,
        CancellationToken cancellationToken)
    {
        var blocks = new List<SequentialDecodedBlock>();
        var consumed = new HashSet<long>();
        string? currentFileType = null;
        foreach (var segment in (representation.Segments ?? []).OrderBy(segment => segment.Position))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (segment.Kind == SequentialSegmentKind.TapeMark
                && segment.Metadata.ContainsKey(MsxCasConstants.SeparatorMetadataKey))
            {
                consumed.Add(segment.Position);
                continue;
            }
            if (segment.Kind != SequentialSegmentKind.DataBlock) continue;
            var data = await ReadRangeAsync(segment.DataRange, cancellationToken).ConfigureAwait(false);
            if (data is null) continue;
            if (segment.Metadata.TryGetValue(MsxCasConstants.FileTypeMetadataKey, out var fileType))
                currentFileType = fileType;
            if (currentFileType is null) continue;
            blocks.Add(CreateBlock(blocks.Count, data, [segment.Position], "cas", currentFileType, true));
            consumed.Add(segment.Position);
        }
        return Result(representation, blocks, consumed, "No structurally identified MSX CAS byte group was found.");
    }

    private static async Task<SequentialDecodeResult> DecodeTsxAsync(
        MediaImageDocument document,
        SequentialMediaImageRepresentation sourceRepresentation,
        CancellationToken cancellationToken)
    {
        var signal = await new TzxSignalDecoder().DecodeAsync(document, cancellationToken).ConfigureAwait(false);
        var blocks = new List<SequentialDecodedBlock>();
        var consumed = new HashSet<long>();
        foreach (var segment in signal.Timeline.Segments ?? [])
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (segment.Kind != SequentialSegmentKind.DataBlock) continue;
            var data = await ReadRangeAsync(segment.DataRange, cancellationToken).ConfigureAwait(false);
            if (data is null) continue;
            var sourcePosition = TryReadLong(segment.Metadata, "sourceSegmentPosition", out var parsedPosition)
                ? parsedPosition
                : segment.Position;
            var fileType = IdentifyFileType(data);
            blocks.Add(CreateBlock(blocks.Count, data, [sourcePosition], "tsx", fileType, fileType is null ? null : true));
            consumed.Add(sourcePosition);
        }
        var result = Result(sourceRepresentation, blocks, consumed, "No MSX data block could be expanded from the TSX image.");
        if (signal.Diagnostics.Count == 0) return result;
        return new SequentialDecodeResult(
            result.DecoderId,
            result.Confidence,
            result.Blocks,
            result.UndecodedSegments,
            [.. result.Diagnostics, .. signal.Diagnostics]);
    }

    private static async Task<SequentialDecodeResult> DecodeWaveAsync(
        MediaImageDocument document,
        SequentialMediaImageRepresentation representation,
        CancellationToken cancellationToken)
    {
        if (!PcmSampleFunctions.TryGetProfile(document.Metadata, out var channels, out var sampleRate, out var bitsPerSample, out var blockAlign))
            return new SequentialDecodeResult(IdValue, 0, [], representation.Segments ?? [], ["The WAV PCM profile is incomplete or incompatible."]);
        var allBytes = new List<byte>();
        var consumed = new HashSet<long>();
        var validFrames = 0;
        var testedFrames = 0;
        foreach (var segment in (representation.Segments ?? [])
                     .Where(segment => segment.Kind == SequentialSegmentKind.Samples && segment.ChannelNumber == 0)
                     .OrderBy(segment => segment.Position))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var pcm = await ReadRangeAsync(segment.DataRange, cancellationToken).ConfigureAwait(false);
            if (pcm is null) continue;
            var samples = PcmSampleFunctions.ExtractChannel(pcm, channels, bitsPerSample, blockAlign, 0);
            var decoded = FskSerialFunctions.Decode(
                samples,
                sampleRate,
                MsxCasConstants.StandardBaudRate,
                MsxCasConstants.ZeroFrequency,
                MsxCasConstants.OneFrequency,
                MsxCasConstants.DataBitsPerFrame,
                MsxCasConstants.StopBitsPerFrame,
                MsxCasConstants.MinimumSamplesPerBit);
            allBytes.AddRange(decoded.Bytes);
            validFrames += decoded.ValidFrames;
            testedFrames += decoded.TestedFrames;
            if (decoded.Bytes.Count > 0) consumed.Add(segment.Position);
        }
        var fileType = IdentifyFileType(allBytes);
        var blocks = allBytes.Count == 0
            ? Array.Empty<SequentialDecodedBlock>()
            : [CreateBlock(0, allBytes.ToArray(), consumed.ToArray(), "wav-fsk", fileType, testedFrames > 0 && validFrames == testedFrames)];
        var undecoded = (representation.Segments ?? []).Where(segment => !consumed.Contains(segment.Position)).ToArray();
        var confidence = testedFrames == 0 ? 0 : validFrames / (double)testedFrames;
        return new SequentialDecodeResult(
            IdValue,
            confidence,
            blocks,
            undecoded,
            blocks.Length == 0 ? ["No complete MSX FSK serial frame was found in the selected WAV channel."] : []);
    }

    private static SequentialDecodeResult Result(
        SequentialMediaImageRepresentation representation,
        IReadOnlyList<SequentialDecodedBlock> blocks,
        IReadOnlySet<long> consumed,
        string emptyDiagnostic)
    {
        var undecoded = (representation.Segments ?? []).Where(segment => !consumed.Contains(segment.Position)).ToArray();
        var confidence = blocks.Count == 0 ? 0 : blocks.Count(block => block.IntegrityValid != false) / (double)blocks.Count;
        return new SequentialDecodeResult(IdValue, confidence, blocks, undecoded, blocks.Count == 0 ? [emptyDiagnostic] : []);
    }

    private static SequentialDecodedBlock CreateBlock(
        long position,
        ReadOnlyMemory<byte> data,
        IReadOnlyList<long> sourcePositions,
        string sourceKind,
        string? fileType,
        bool? integrityValid)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [MsxCasConstants.SourceKindMetadataKey] = sourceKind
        };
        if (integrityValid is { } serialFramesValid)
            metadata[MsxCasConstants.SerialFramesValidMetadataKey] = serialFramesValid.ToString(CultureInfo.InvariantCulture);
        if (fileType is not null) metadata[MsxCasConstants.FileTypeMetadataKey] = fileType;
        return new SequentialDecodedBlock(position, data, sourcePositions, integrityValid, metadata);
    }

    private static string? IdentifyFileType(IReadOnlyList<byte> data)
    {
        if (data.Count < MsxCasConstants.FileTypeMarkerLength) return null;
        var first = data[0];
        for (var index = 1; index < MsxCasConstants.FileTypeMarkerLength; index++)
            if (data[index] != first) return null;
        return first switch
        {
            MsxCasConstants.BinaryFileMarker => "binary",
            MsxCasConstants.TokenizedBasicFileMarker => "basic",
            MsxCasConstants.AsciiFileMarker => "ascii",
            _ => null
        };
    }

    private static async Task<byte[]?> ReadRangeAsync(MediaDataRange? range, CancellationToken cancellationToken)
    {
        if (range is not { Source: not null, Length: > 0 and <= int.MaxValue }) return null;
        var data = new byte[checked((int)range.Length)];
        await range.Source.ReadExactlyAsync(range.SourceOffset, data, cancellationToken).ConfigureAwait(false);
        return data;
    }

    private static bool TryReadLong(IReadOnlyDictionary<string, string> metadata, string key, out long value)
    {
        value = 0;
        return metadata.TryGetValue(key, out var text)
            && long.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value)
            && value >= 0;
    }

    private const string IdValue = MsxCasConstants.DecoderId;
}
