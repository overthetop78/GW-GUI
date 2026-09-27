using System.Buffers.Binary;
using System.Collections.Frozen;
using System.IO;
using System.Text;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Functions;
using GWGUI.MediaEngine.Images.Formats.Tape.Amstrad;
using GWGUI.MediaEngine.Images.Formats.Tape.Tzx;
using GWGUI.MediaEngine.Images.Models.Sequential;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Interfaces.Decoding;

namespace GWGUI.MediaEngine.Images.Reading.Decoding.Sequential.Amstrad;

/// <summary>Decodes CPC firmware cassette records retained in CDT/TZX or captured as PCM audio.</summary>
public sealed class AmstradCpcTapeDecoder : ISequentialMediaDecoder
{
    private static readonly IReadOnlySet<string> SupportedFormats =
        new[] { TapeImageFormatIds.Tzx, TapeImageFormatIds.Wav, TapeImageFormatIds.Audio, TapeImageFormatIds.Voc }
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedMachines =
        new[] { DiskSystemIds.Amstrad }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public string Id => AmstradCpcTapeConstants.DecoderId;
    public IReadOnlySet<string> FormatIds => SupportedFormats;
    public IReadOnlySet<string> MachineIds => SupportedMachines;

    public bool CanDecode(MediaImageDocument document, string? machineId = null) =>
        document.MediaKind == MediaKind.Tape
        && SupportedFormats.Contains(document.FormatId)
        && (machineId is null || SupportedMachines.Contains(machineId))
        && document.Representation is SequentialMediaImageRepresentation;

    public async Task<SequentialDecodeResult> DecodeAsync(
        MediaImageDocument document,
        string? machineId = null,
        CancellationToken cancellationToken = default)
    {
        if (!CanDecode(document, machineId))
            throw new InvalidDataException("The media document is not compatible with the Amstrad CPC cassette decoder.");

        var representation = (SequentialMediaImageRepresentation)document.Representation;
        var records = document.FormatId.Equals(TapeImageFormatIds.Tzx, StringComparison.OrdinalIgnoreCase)
            ? await DecodeStructuredRecordsAsync(representation, cancellationToken).ConfigureAwait(false)
            : await DecodePcmRecordsAsync(document, representation, cancellationToken).ConfigureAwait(false);
        return BuildResult(representation, records);
    }

    private static async Task<IReadOnlyList<DecodedRecord>> DecodeStructuredRecordsAsync(
        SequentialMediaImageRepresentation representation,
        CancellationToken cancellationToken)
    {
        var records = new List<DecodedRecord>();
        foreach (var segment in representation.Segments ?? [])
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (segment.DataRange is not { Source: not null, Length: <= int.MaxValue } range) continue;
            var payload = new byte[checked((int)range.Length)];
            await range.Source.ReadExactlyAsync(range.SourceOffset, payload, cancellationToken).ConfigureAwait(false);
            if (!TryGetTzxDataRange(segment, payload, out var offset, out var length)) continue;
            if (offset + (long)length > payload.Length) continue;
            if (TryDecodeRecord(payload.AsSpan(offset, length), [segment.Position], out var record)) records.Add(record);
        }
        return records;
    }

    private static async Task<IReadOnlyList<DecodedRecord>> DecodePcmRecordsAsync(
        MediaImageDocument document,
        SequentialMediaImageRepresentation representation,
        CancellationToken cancellationToken)
    {
        if (!PcmSampleFunctions.TryGetProfile(document.Metadata, out var channels, out var sampleRate, out var bitsPerSample, out var blockAlign))
            return [];

        var records = new List<DecodedRecord>();
        foreach (var segment in (representation.Segments ?? [])
                     .Where(segment => segment.Kind == SequentialSegmentKind.Samples && segment.ChannelNumber == 0))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (segment.DataRange is not { Source: not null, Length: <= int.MaxValue } range) continue;
            var pcm = new byte[checked((int)range.Length)];
            await range.Source.ReadExactlyAsync(range.SourceOffset, pcm, cancellationToken).ConfigureAwait(false);
            var samples = PcmSampleFunctions.ExtractChannel(pcm, channels, bitsPerSample, blockAlign, 0);
            foreach (var encoded in DetectRecords(samples, sampleRate))
                if (TryDecodeRecord(encoded, [segment.Position], out var record)) records.Add(record);
        }
        return records;
    }

    private static SequentialDecodeResult BuildResult(
        SequentialMediaImageRepresentation representation,
        IReadOnlyList<DecodedRecord> records)
    {
        var blocks = new List<SequentialDecodedBlock>();
        var consumed = records.SelectMany(record => record.SourcePositions).ToHashSet();
        var diagnostics = new List<string>();
        HeaderRecord? pending = null;

        foreach (var record in records)
        {
            if (record.Sync == AmstradCpcTapeConstants.HeaderSync)
            {
                if (record.Data.Length < AmstradCpcTapeConstants.HeaderLength)
                {
                    diagnostics.Add("A CPC cassette header record is shorter than 64 bytes.");
                    pending = null;
                    continue;
                }
                pending = ParseHeader(record);
                continue;
            }

            if (record.Sync != AmstradCpcTapeConstants.DataSync || pending is null)
            {
                diagnostics.Add("A CPC cassette data record has no preceding header record.");
                continue;
            }

            var dataLength = Math.Min(pending.DataLength, record.Data.Length);
            var metadata = new Dictionary<string, string>(pending.Metadata, StringComparer.Ordinal)
            {
                ["sourceKind"] = "amstrad-cpc-cassette-records",
                ["headerIntegrityValid"] = pending.IntegrityValid.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["dataIntegrityValid"] = record.IntegrityValid.ToString(System.Globalization.CultureInfo.InvariantCulture)
            };
            blocks.Add(new SequentialDecodedBlock(
                blocks.Count,
                record.Data[..dataLength],
                pending.SourcePositions.Concat(record.SourcePositions).Distinct().ToArray(),
                pending.IntegrityValid && record.IntegrityValid && dataLength == pending.DataLength,
                metadata));
            if (dataLength != pending.DataLength)
                diagnostics.Add($"CPC cassette block {pending.BlockNumber} contains {dataLength} of {pending.DataLength} declared bytes.");
            pending = null;
        }

        if (pending is not null) diagnostics.Add("A CPC cassette header record has no following data record.");
        var validCount = blocks.Count(block => block.IntegrityValid == true);
        return new SequentialDecodeResult(
            AmstradCpcTapeConstants.DecoderId,
            blocks.Count == 0 ? 0 : validCount / (double)blocks.Count,
            blocks,
            (representation.Segments ?? []).Where(segment => !consumed.Contains(segment.Position)).ToArray(),
            blocks.Count == 0 ? diagnostics.Append("No complete CPC cassette file block was found.").ToArray() : diagnostics);
    }

    private static HeaderRecord ParseHeader(DecodedRecord record)
    {
        var header = record.Data.Span;
        var name = Encoding.Latin1.GetString(header.Slice(
                AmstradCpcTapeConstants.FileNameOffset,
                AmstradCpcTapeConstants.FileNameLength))
            .TrimEnd('\0', ' ');
        var blockNumber = header[AmstradCpcTapeConstants.BlockNumberOffset];
        var dataLength = BinaryPrimitives.ReadUInt16LittleEndian(header[AmstradCpcTapeConstants.DataLengthOffset..]);
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["fileName"] = name,
            ["blockNumber"] = blockNumber.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["lastBlock"] = (header[AmstradCpcTapeConstants.LastBlockOffset] != 0).ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["firstBlock"] = (header[AmstradCpcTapeConstants.FirstBlockOffset] != 0).ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["fileType"] = header[AmstradCpcTapeConstants.FileTypeOffset].ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["dataLength"] = dataLength.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["dataLocation"] = BinaryPrimitives.ReadUInt16LittleEndian(header[AmstradCpcTapeConstants.DataLocationOffset..]).ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["logicalLength"] = BinaryPrimitives.ReadUInt16LittleEndian(header[AmstradCpcTapeConstants.LogicalLengthOffset..]).ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["entryAddress"] = BinaryPrimitives.ReadUInt16LittleEndian(header[AmstradCpcTapeConstants.EntryAddressOffset..]).ToString(System.Globalization.CultureInfo.InvariantCulture)
        };
        return new HeaderRecord(blockNumber, dataLength, record.IntegrityValid, record.SourcePositions, metadata);
    }

    private static bool TryDecodeRecord(
        ReadOnlySpan<byte> encoded,
        IReadOnlyList<long> sourcePositions,
        out DecodedRecord record)
    {
        record = default!;
        if (encoded.Length < 1 + AmstradCpcTapeConstants.SegmentDataLength + AmstradCpcTapeConstants.SegmentCrcLength)
            return false;
        var sync = encoded[0];
        if (sync is not AmstradCpcTapeConstants.HeaderSync and not AmstradCpcTapeConstants.DataSync) return false;

        var body = encoded[1..];
        if (!TryGetSegmentCount(body, sync == AmstradCpcTapeConstants.HeaderSync, out var segmentCount)) return false;
        var data = new byte[segmentCount * AmstradCpcTapeConstants.SegmentDataLength];
        var integrityValid = true;
        for (var segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
        {
            var sourceOffset = segmentIndex * (AmstradCpcTapeConstants.SegmentDataLength + AmstradCpcTapeConstants.SegmentCrcLength);
            var segmentData = body.Slice(sourceOffset, AmstradCpcTapeConstants.SegmentDataLength);
            segmentData.CopyTo(data.AsSpan(segmentIndex * AmstradCpcTapeConstants.SegmentDataLength));
            var storedCrc = BinaryPrimitives.ReadUInt16BigEndian(body.Slice(sourceOffset + AmstradCpcTapeConstants.SegmentDataLength));
            integrityValid &= storedCrc == unchecked((ushort)~ComputeCrc(segmentData));
        }
        record = new DecodedRecord(sync, data, integrityValid, sourcePositions);
        return true;
    }

    private static bool TryGetSegmentCount(ReadOnlySpan<byte> body, bool header, out int segmentCount)
    {
        segmentCount = 0;
        var candidates = header ? new[] { 1 } : Enumerable.Range(1, 8).ToArray();
        foreach (var trailerLength in new[] { AmstradCpcTapeConstants.TrailerByteCount, 4 })
        foreach (var count in candidates)
        {
            var used = count * (AmstradCpcTapeConstants.SegmentDataLength + AmstradCpcTapeConstants.SegmentCrcLength);
            if (body.Length != used + trailerLength
                || !body[used..].ToArray().All(value => value == AmstradCpcTapeConstants.TrailerByte))
                continue;
            segmentCount = count;
            return true;
        }
        return false;
    }

    private static IReadOnlyList<byte[]> DetectRecords(IReadOnlyList<int> samples, int sampleRate)
    {
        var pulses = GetPulseLengths(samples, sampleRate);
        var records = new List<byte[]>();
        for (var index = 0; index < pulses.Count;)
        {
            var pilotStart = index;
            var pilotLength = pulses[index];
            while (index < pulses.Count && Close(pulses[index], pilotLength)) index++;
            if (index - pilotStart < AmstradCpcTapeConstants.MinimumPilotPulseCount || index + 1 >= pulses.Count)
            {
                index = pilotStart + 1;
                continue;
            }
            var zeroLength = pilotLength / 2;
            if (!Close(pulses[index], zeroLength) || !Close(pulses[index + 1], zeroLength))
            {
                index = pilotStart + 1;
                continue;
            }
            index += 2;
            var bits = new List<bool>();
            while (index + 1 < pulses.Count)
            {
                var first = pulses[index];
                var second = pulses[index + 1];
                if (!Close(first, second)) break;
                if (Close(first, zeroLength)) bits.Add(false);
                else if (Close(first, pilotLength)) bits.Add(true);
                else break;
                index += 2;
            }
            var bytes = new byte[bits.Count / 8];
            for (var byteIndex = 0; byteIndex < bytes.Length; byteIndex++)
                for (var bit = 0; bit < 8; bit++)
                    if (bits[byteIndex * 8 + bit]) bytes[byteIndex] |= (byte)(0x80 >> bit);
            if (bytes.Length > 0) records.Add(bytes);
        }
        return records;
    }

    private static IReadOnlyList<double> GetPulseLengths(IReadOnlyList<int> samples, int sampleRate)
    {
        var pulses = new List<double>();
        if (samples.Count == 0) return pulses;
        var previousSign = samples[0] >= 0;
        var previousCrossing = 0;
        for (var index = 1; index < samples.Count; index++)
        {
            var sign = samples[index] >= 0;
            if (sign == previousSign) continue;
            pulses.Add((index - previousCrossing) * (double)TzxConstants.TStatesPerSecond / sampleRate);
            previousCrossing = index;
            previousSign = sign;
        }
        return pulses;
    }

    private static bool TryGetTzxDataRange(SequentialMediaSegment segment, ReadOnlySpan<byte> payload, out int offset, out int length)
    {
        offset = length = 0;
        if (!segment.Metadata.TryGetValue(TzxConstants.BlockIdMetadataKey, out var idText)
            || !byte.TryParse(idText, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out var id)
            || segment.DataRange is not { Length: <= int.MaxValue })
            return false;
        offset = id switch
        {
            TzxConstants.StandardSpeedData => 4,
            TzxConstants.TurboSpeedData => 18,
            TzxConstants.PureData => 10,
            _ => -1
        };
        if (offset < 0 || payload.Length < offset) return false;
        length = id switch
        {
            TzxConstants.StandardSpeedData when payload.Length >= 4 => BinaryPrimitives.ReadUInt16LittleEndian(payload[2..]),
            TzxConstants.TurboSpeedData when payload.Length >= 18 => ReadUInt24(payload[15..]),
            TzxConstants.PureData when payload.Length >= 10 => ReadUInt24(payload[7..]),
            _ => 0
        };
        return length > 0;
    }

    private static int ReadUInt24(ReadOnlySpan<byte> bytes) => bytes[0] | bytes[1] << 8 | bytes[2] << 16;

    private static ushort ComputeCrc(ReadOnlySpan<byte> data)
    {
        var crc = AmstradCpcTapeConstants.CrcSeed;
        foreach (var value in data)
        {
            crc ^= (ushort)(value << 8);
            for (var bit = 0; bit < 8; bit++)
                crc = (ushort)((crc & 0x8000) != 0
                    ? (crc << 1) ^ AmstradCpcTapeConstants.CrcPolynomial
                    : crc << 1);
        }
        return crc;
    }

    private static bool Close(double actual, double expected) =>
        Math.Abs(actual - expected) <= expected * AmstradCpcTapeConstants.PulseToleranceRatio;

    private sealed record DecodedRecord(byte Sync, ReadOnlyMemory<byte> Data, bool IntegrityValid, IReadOnlyList<long> SourcePositions);

    private sealed record HeaderRecord(
        int BlockNumber,
        int DataLength,
        bool IntegrityValid,
        IReadOnlyList<long> SourcePositions,
        IReadOnlyDictionary<string, string> Metadata);
}
