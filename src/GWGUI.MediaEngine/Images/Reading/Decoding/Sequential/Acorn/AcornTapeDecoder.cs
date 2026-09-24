using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Globalization;
using System.Text;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Functions;
using GWGUI.MediaEngine.Interfaces.Decoding;
using GWGUI.MediaEngine.Images.Models.Sectors;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Models.Sequential;

namespace GWGUI.MediaEngine.Images.Reading.Decoding.Sequential.Acorn;

/// <summary>Decodes Acorn cassette filing system blocks from UEF byte streams or compatible PCM FSK signals.</summary>
public sealed class AcornTapeDecoder : ISequentialMediaDecoder
{
    private static readonly IReadOnlySet<string> SupportedFormats =
        new[] { TapeImageFormatIds.Uef, TapeImageFormatIds.Wav }
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedMachines =
        new[] { DiskSystemIds.AcornBbc }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public string Id => UefConstants.DecoderId;
    public IReadOnlySet<string> FormatIds => SupportedFormats;
    public IReadOnlySet<string> MachineIds => SupportedMachines;

    public bool CanDecode(MediaImageDocument document, string? machineId = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        return document.MediaKind == MediaKind.Tape
            && SupportedFormats.Contains(document.FormatId)
            && (machineId is null || SupportedMachines.Contains(machineId))
            && document.Representation is SequentialMediaImageRepresentation;
    }

    public async Task<SequentialDecodeResult> DecodeAsync(
        MediaImageDocument document,
        string? machineId = null,
        CancellationToken cancellationToken = default)
    {
        if (!CanDecode(document, machineId))
            throw new InvalidDataException("The media document is not compatible with the Acorn cassette decoder.");
        var representation = (SequentialMediaImageRepresentation)document.Representation;
        return document.FormatId.Equals(TapeImageFormatIds.Uef, StringComparison.OrdinalIgnoreCase)
            ? await DecodeUefAsync(representation, cancellationToken).ConfigureAwait(false)
            : await DecodeWaveAsync(document, representation, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<SequentialDecodeResult> DecodeUefAsync(
        SequentialMediaImageRepresentation representation,
        CancellationToken cancellationToken)
    {
        var blocks = new List<SequentialDecodedBlock>();
        var consumed = new HashSet<long>();
        foreach (var segment in (representation.Segments ?? []).OrderBy(segment => segment.Position))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (segment.Kind != SequentialSegmentKind.DataBlock
                || !IsImplicitData(segment)
                || await ReadRangeAsync(segment.DataRange, cancellationToken).ConfigureAwait(false) is not { } bytes)
                continue;
            var decoded = DecodeBlocks(bytes, segment.Position, "uef-implicit", blocks.Count);
            if (decoded.Count == 0) continue;
            blocks.AddRange(decoded);
            consumed.Add(segment.Position);
        }
        return CreateResult(representation, blocks, consumed, "No valid Acorn cassette block was found in the UEF byte streams.");
    }

    private static async Task<SequentialDecodeResult> DecodeWaveAsync(
        MediaImageDocument document,
        SequentialMediaImageRepresentation representation,
        CancellationToken cancellationToken)
    {
        if (!PcmSampleFunctions.TryGetProfile(document.Metadata, out var channels, out var sampleRate, out var bitsPerSample, out var blockAlign))
            return new SequentialDecodeResult(UefConstants.DecoderId, 0, [], representation.Segments ?? [],
                ["The WAV PCM profile is incomplete or incompatible."]);

        var decodedBytes = new List<byte>();
        var sourcePositions = new List<long>();
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
            var serial = FskSerialFunctions.Decode(
                samples,
                sampleRate,
                UefConstants.DefaultBaudRate,
                UefConstants.AcornZeroFrequency,
                UefConstants.AcornOneFrequency,
                UefConstants.AcornDataBitsPerFrame,
                UefConstants.AcornStopBitsPerFrame,
                UefConstants.MinimumSamplesPerBit);
            decodedBytes.AddRange(serial.Bytes);
            validFrames += serial.ValidFrames;
            testedFrames += serial.TestedFrames;
            if (serial.Bytes.Count > 0) sourcePositions.Add(segment.Position);
        }

        var blocks = DecodeBlocks(decodedBytes, sourcePositions, "wav-fsk", 0);
        var consumed = blocks.Count == 0 ? [] : sourcePositions.ToHashSet();
        var result = CreateResult(representation, blocks, consumed,
            "No valid Acorn cassette block was found in the decoded WAV serial stream.");
        if (blocks.Count == 0 || testedFrames == 0) return result;
        var serialConfidence = validFrames / (double)testedFrames;
        return new SequentialDecodeResult(result.DecoderId, Math.Min(result.Confidence, serialConfidence),
            result.Blocks, result.UndecodedSegments, result.Diagnostics);
    }

    private static IReadOnlyList<SequentialDecodedBlock> DecodeBlocks(
        IReadOnlyList<byte> bytes,
        long sourcePosition,
        string sourceKind,
        int firstPosition) => DecodeBlocks(bytes, [sourcePosition], sourceKind, firstPosition);

    private static IReadOnlyList<SequentialDecodedBlock> DecodeBlocks(
        IReadOnlyList<byte> bytes,
        IReadOnlyList<long> sourcePositions,
        string sourceKind,
        int firstPosition)
    {
        var blocks = new List<SequentialDecodedBlock>();
        for (var offset = 0; offset < bytes.Count; offset++)
        {
            if (bytes[offset] != UefConstants.AcornBlockSync) continue;
            var nameStart = offset + 1;
            var nameEnd = nameStart;
            while (nameEnd < bytes.Count
                   && nameEnd - nameStart <= UefConstants.AcornMaximumFileNameLength
                   && bytes[nameEnd] != 0)
                nameEnd++;
            if (nameEnd >= bytes.Count
                || bytes[nameEnd] != 0
                || nameEnd - nameStart > UefConstants.AcornMaximumFileNameLength)
                continue;

            var fixedHeaderStart = nameEnd + 1;
            var fixedHeaderLength = UefConstants.AcornLoadAddressLength
                + UefConstants.AcornExecutionAddressLength
                + UefConstants.AcornBlockNumberLength
                + UefConstants.AcornDataLengthFieldLength
                + UefConstants.AcornFlagsLength
                + UefConstants.AcornNextAddressLength;
            var headerEnd = fixedHeaderStart + fixedHeaderLength;
            if (headerEnd + UefConstants.AcornChecksumLength > bytes.Count) continue;

            var dataLengthOffset = fixedHeaderStart
                + UefConstants.AcornLoadAddressLength
                + UefConstants.AcornExecutionAddressLength
                + UefConstants.AcornBlockNumberLength;
            var dataLength = ReadUInt16LittleEndian(bytes, dataLengthOffset);
            var dataStart = headerEnd + UefConstants.AcornChecksumLength;
            var blockEnd = dataStart + dataLength + UefConstants.AcornChecksumLength;
            if (blockEnd > bytes.Count) continue;

            var headerBytes = Copy(bytes, nameStart, headerEnd - nameStart);
            var dataBytes = Copy(bytes, dataStart, dataLength);
            var storedHeaderChecksum = ReadUInt16BigEndian(bytes, headerEnd);
            var storedDataChecksum = ReadUInt16BigEndian(bytes, dataStart + dataLength);
            var headerValid = Crc16Calculator.Compute(
                headerBytes,
                UefConstants.AcornChecksumPolynomial,
                UefConstants.AcornChecksumInitialValue) == storedHeaderChecksum;
            var dataValid = Crc16Calculator.Compute(
                dataBytes,
                UefConstants.AcornChecksumPolynomial,
                UefConstants.AcornChecksumInitialValue) == storedDataChecksum;

            var cursor = fixedHeaderStart;
            var loadAddress = ReadUInt32LittleEndian(bytes, cursor);
            cursor += UefConstants.AcornLoadAddressLength;
            var executionAddress = ReadUInt32LittleEndian(bytes, cursor);
            cursor += UefConstants.AcornExecutionAddressLength;
            var blockNumber = ReadUInt16LittleEndian(bytes, cursor);
            cursor += UefConstants.AcornBlockNumberLength + UefConstants.AcornDataLengthFieldLength;
            var flags = bytes[cursor++];
            var nextAddress = ReadUInt32LittleEndian(bytes, cursor);
            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [UefConstants.SourceKindMetadataKey] = sourceKind,
                [UefConstants.FileNameMetadataKey] = System.Text.Encoding.ASCII.GetString(Copy(bytes, nameStart, nameEnd - nameStart)),
                [UefConstants.LoadAddressMetadataKey] = loadAddress.ToString("x8", CultureInfo.InvariantCulture),
                [UefConstants.ExecutionAddressMetadataKey] = executionAddress.ToString("x8", CultureInfo.InvariantCulture),
                [UefConstants.BlockNumberMetadataKey] = blockNumber.ToString(CultureInfo.InvariantCulture),
                [UefConstants.FlagsMetadataKey] = flags.ToString("x2", CultureInfo.InvariantCulture),
                [UefConstants.NextAddressMetadataKey] = nextAddress.ToString("x8", CultureInfo.InvariantCulture)
            };
            blocks.Add(new SequentialDecodedBlock(firstPosition + blocks.Count, dataBytes, sourcePositions,
                headerValid && dataValid, metadata));
            offset = blockEnd - 1;
        }
        return blocks;
    }

    private static SequentialDecodeResult CreateResult(
        SequentialMediaImageRepresentation representation,
        IReadOnlyList<SequentialDecodedBlock> blocks,
        IReadOnlySet<long> consumed,
        string emptyDiagnostic)
    {
        var undecoded = (representation.Segments ?? []).Where(segment => !consumed.Contains(segment.Position)).ToArray();
        var confidence = blocks.Count == 0
            ? 0
            : blocks.Count(block => block.IntegrityValid == true) / (double)blocks.Count;
        return new SequentialDecodeResult(UefConstants.DecoderId, confidence, blocks, undecoded,
            blocks.Count == 0 ? [emptyDiagnostic] : []);
    }

    private static bool IsImplicitData(SequentialMediaSegment segment) =>
        segment.Metadata.TryGetValue(UefConstants.ChunkIdMetadataKey, out var chunkId)
        && ushort.TryParse(chunkId, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var parsed)
        && parsed == UefConstants.ImplicitData;

    private static async Task<byte[]?> ReadRangeAsync(MediaDataRange? range, CancellationToken cancellationToken)
    {
        if (range is not { Source: not null, Length: > 0 and <= int.MaxValue }) return null;
        var data = new byte[checked((int)range.Length)];
        await range.Source.ReadExactlyAsync(range.SourceOffset, data, cancellationToken).ConfigureAwait(false);
        return data;
    }

    private static byte[] Copy(IReadOnlyList<byte> source, int offset, int length)
    {
        var result = new byte[length];
        for (var index = 0; index < length; index++) result[index] = source[offset + index];
        return result;
    }

    private static ushort ReadUInt16LittleEndian(IReadOnlyList<byte> source, int offset) =>
        BinaryPrimitives.ReadUInt16LittleEndian(Copy(source, offset, sizeof(ushort)));

    private static ushort ReadUInt16BigEndian(IReadOnlyList<byte> source, int offset) =>
        BinaryPrimitives.ReadUInt16BigEndian(Copy(source, offset, sizeof(ushort)));

    private static uint ReadUInt32LittleEndian(IReadOnlyList<byte> source, int offset) =>
        BinaryPrimitives.ReadUInt32LittleEndian(Copy(source, offset, sizeof(uint)));
}
