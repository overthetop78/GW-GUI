using System.Buffers.Binary;
using System.Collections.Frozen;
using System.IO;
using System.Text;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Formats.Tape.Amstrad;
using GWGUI.MediaEngine.Images.Formats.Tape.Tzx;
using GWGUI.MediaEngine.Images.Models.Sequential;
using GWGUI.MediaEngine.Images.Reading.Sources;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Interfaces.Encoding;

namespace GWGUI.MediaEngine.Images.Writing.Encoding.Sequential.Amstrad;

/// <summary>Encodes CPC firmware file blocks with their native CRC and timing as CDT/TZX or PCM audio.</summary>
public sealed class AmstradCpcTapeEncoder : ISequentialMediaEncoder
{
    private const int DefaultSampleRate = 44100;
    private static readonly IReadOnlySet<string> SupportedFormats =
        new[] { TapeImageFormatIds.Tzx, TapeImageFormatIds.Wav, TapeImageFormatIds.Voc }
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedMachines =
        new[] { DiskSystemIds.Amstrad }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public string Id => AmstradCpcTapeConstants.EncoderId;
    public IReadOnlySet<string> FormatIds => SupportedFormats;
    public IReadOnlySet<string> MachineIds => SupportedMachines;

    public bool CanEncode(SequentialEncodeRequest request) =>
        SupportedFormats.Contains(request.TargetFormatId)
        && SupportedMachines.Contains(request.MachineId)
        && request.Blocks.Count > 0
        && request.Blocks.All(block => block.Data.Length is > 0 and <= 2048)
        && request.RetainedSegments.Count == 0;

    public Task<SequentialMediaImageRepresentation> EncodeAsync(
        SequentialEncodeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!CanEncode(request))
            throw new InvalidDataException("CPC cassette encoding requires one or more decoded file blocks of at most 2048 bytes.");
        cancellationToken.ThrowIfCancellationRequested();

        var records = BuildRecords(request.Blocks);
        return Task.FromResult(request.TargetFormatId.Equals(TapeImageFormatIds.Tzx, StringComparison.OrdinalIgnoreCase)
            ? EncodeTzx(records)
            : EncodePcm(records, ReadSampleRate(request.Parameters)));
    }

    private static IReadOnlyList<byte[]> BuildRecords(IReadOnlyList<SequentialDecodedBlock> blocks)
    {
        var records = new List<byte[]>(blocks.Count * 2);
        for (var index = 0; index < blocks.Count; index++)
        {
            var block = blocks[index];
            var data = block.Data.ToArray();
            var fileName = ReadText(block.Metadata, "fileName", $"BLOCK{index + 1:D3}");
            var firstBlock = index == 0 || !HasSameFileName(blocks[index - 1], fileName);
            var lastBlock = index == blocks.Count - 1 || !HasSameFileName(blocks[index + 1], fileName);
            var header = new byte[AmstradCpcTapeConstants.SegmentDataLength];
            WriteName(header, fileName);
            header[AmstradCpcTapeConstants.BlockNumberOffset] = ReadByte(block.Metadata, "blockNumber", checked((byte)(index + 1)));
            header[AmstradCpcTapeConstants.LastBlockOffset] = ReadBoolean(block.Metadata, "lastBlock", lastBlock) ? (byte)0xff : (byte)0;
            header[AmstradCpcTapeConstants.FileTypeOffset] = ReadByte(block.Metadata, "fileType", 2);
            BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(AmstradCpcTapeConstants.DataLengthOffset), checked((ushort)data.Length));
            BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(AmstradCpcTapeConstants.DataLocationOffset), ReadUInt16(block.Metadata, "dataLocation"));
            header[AmstradCpcTapeConstants.FirstBlockOffset] = ReadBoolean(block.Metadata, "firstBlock", firstBlock) ? (byte)0xff : (byte)0;
            BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(AmstradCpcTapeConstants.LogicalLengthOffset), ReadUInt16(block.Metadata, "logicalLength", checked((ushort)data.Length)));
            BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(AmstradCpcTapeConstants.EntryAddressOffset), ReadUInt16(block.Metadata, "entryAddress"));
            records.Add(BuildRecord(AmstradCpcTapeConstants.HeaderSync, header));
            records.Add(BuildRecord(AmstradCpcTapeConstants.DataSync, data));
        }
        return records;
    }

    private static bool HasSameFileName(SequentialDecodedBlock block, string fileName) =>
        block.Metadata.TryGetValue("fileName", out var candidate)
        && string.Equals(candidate, fileName, StringComparison.Ordinal);

    private static byte[] BuildRecord(byte sync, ReadOnlySpan<byte> data)
    {
        var segmentCount = Math.Max(1, (data.Length + AmstradCpcTapeConstants.SegmentDataLength - 1) / AmstradCpcTapeConstants.SegmentDataLength);
        var encoded = new byte[1
            + segmentCount * (AmstradCpcTapeConstants.SegmentDataLength + AmstradCpcTapeConstants.SegmentCrcLength)
            + AmstradCpcTapeConstants.TrailerByteCount];
        encoded[0] = sync;
        for (var segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
        {
            var offset = segmentIndex * AmstradCpcTapeConstants.SegmentDataLength;
            var count = Math.Min(AmstradCpcTapeConstants.SegmentDataLength, data.Length - offset);
            var destinationOffset = 1 + segmentIndex * (AmstradCpcTapeConstants.SegmentDataLength + AmstradCpcTapeConstants.SegmentCrcLength);
            if (count > 0) data.Slice(offset, count).CopyTo(encoded.AsSpan(destinationOffset));
            var segmentData = encoded.AsSpan(destinationOffset, AmstradCpcTapeConstants.SegmentDataLength);
            BinaryPrimitives.WriteUInt16BigEndian(
                encoded.AsSpan(destinationOffset + AmstradCpcTapeConstants.SegmentDataLength),
                unchecked((ushort)~ComputeCrc(segmentData)));
        }
        encoded.AsSpan(encoded.Length - AmstradCpcTapeConstants.TrailerByteCount).Fill(AmstradCpcTapeConstants.TrailerByte);
        return encoded;
    }

    private static SequentialMediaImageRepresentation EncodeTzx(IReadOnlyList<byte[]> records)
    {
        var segments = records.Select((record, index) =>
        {
            var payload = CreateTurboPayload(record);
            var source = new MemoryRandomAccessData(payload);
            return new SequentialMediaSegment(
                index,
                SequentialSegmentKind.DataBlock,
                payload.Length,
                dataRange: new MediaDataRange(0, payload.Length, MediaDataRangeKind.Stored, source, 0),
                metadata: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [TzxConstants.BlockIdMetadataKey] = TzxConstants.TurboSpeedData.ToString("x2", System.Globalization.CultureInfo.InvariantCulture),
                    ["payloadLength"] = payload.Length.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["known"] = bool.TrueString,
                    ["reconstructedSignal"] = bool.TrueString
                });
        }).ToArray();
        return new SequentialMediaImageRepresentation(segments.Sum(segment => segment.Length ?? 0), segments: segments);
    }

    private static SequentialMediaImageRepresentation EncodePcm(IReadOnlyList<byte[]> records, int sampleRate)
    {
        var samples = new List<short>();
        short level = AmstradCpcTapeConstants.PcmAmplitude;
        foreach (var record in records)
        {
            for (var index = 0; index < sampleRate; index++) samples.Add(0);
            AddPulses(samples, ref level, AmstradCpcTapeConstants.OnePulseTStates, AmstradCpcTapeConstants.PilotPulseCount, sampleRate);
            AddPulses(samples, ref level, AmstradCpcTapeConstants.ZeroPulseTStates, 2, sampleRate);
            foreach (var value in record)
                for (var bit = 7; bit >= 0; bit--)
                    AddPulses(
                        samples,
                        ref level,
                        (value & (1 << bit)) == 0 ? AmstradCpcTapeConstants.ZeroPulseTStates : AmstradCpcTapeConstants.OnePulseTStates,
                        2,
                        sampleRate);
        }
        for (var index = 0; index < sampleRate; index++) samples.Add(0);

        var pcm = new byte[samples.Count * sizeof(short)];
        for (var index = 0; index < samples.Count; index++)
            BinaryPrimitives.WriteInt16LittleEndian(pcm.AsSpan(index * sizeof(short)), samples[index]);
        var source = new MemoryRandomAccessData(pcm);
        var duration = TimeSpan.FromSeconds(samples.Count / (double)sampleRate);
        var segment = new SequentialMediaSegment(
            0,
            SequentialSegmentKind.Samples,
            samples.Count,
            TimeSpan.Zero,
            duration,
            channelNumber: 0,
            direction: SequentialTravelDirection.Forward,
            dataRange: new MediaDataRange(0, pcm.Length, MediaDataRangeKind.Stored, source, 0),
            metadata: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["channel"] = "0",
                ["interleaved"] = bool.TrueString,
                ["blockAlign"] = sizeof(short).ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["sampleRate"] = sampleRate.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["bitsPerSample"] = AmstradCpcTapeConstants.PcmBitsPerSample.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["reconstructedSignal"] = bool.TrueString
            });
        return new SequentialMediaImageRepresentation(pcm.Length, duration, [segment]);
    }

    private static byte[] CreateTurboPayload(ReadOnlySpan<byte> data)
    {
        var payload = new byte[18 + data.Length];
        BinaryPrimitives.WriteUInt16LittleEndian(payload, AmstradCpcTapeConstants.OnePulseTStates);
        BinaryPrimitives.WriteUInt16LittleEndian(payload.AsSpan(2), AmstradCpcTapeConstants.ZeroPulseTStates);
        BinaryPrimitives.WriteUInt16LittleEndian(payload.AsSpan(4), AmstradCpcTapeConstants.ZeroPulseTStates);
        BinaryPrimitives.WriteUInt16LittleEndian(payload.AsSpan(6), AmstradCpcTapeConstants.ZeroPulseTStates);
        BinaryPrimitives.WriteUInt16LittleEndian(payload.AsSpan(8), AmstradCpcTapeConstants.OnePulseTStates);
        BinaryPrimitives.WriteUInt16LittleEndian(payload.AsSpan(10), AmstradCpcTapeConstants.PilotPulseCount);
        payload[12] = AmstradCpcTapeConstants.UsedBitsInLastByte;
        BinaryPrimitives.WriteUInt16LittleEndian(payload.AsSpan(13), AmstradCpcTapeConstants.PauseMilliseconds);
        WriteUInt24(payload.AsSpan(15), data.Length);
        data.CopyTo(payload.AsSpan(18));
        return payload;
    }

    private static void AddPulses(List<short> samples, ref short level, ushort tStates, int count, int sampleRate)
    {
        var sampleCount = Math.Max(1, (int)Math.Round(tStates * sampleRate / (double)TzxConstants.TStatesPerSecond));
        for (var pulse = 0; pulse < count; pulse++)
        {
            for (var sample = 0; sample < sampleCount; sample++) samples.Add(level);
            level = (short)-level;
        }
    }

    private static void WriteName(Span<byte> header, string name)
    {
        var bytes = System.Text.Encoding.Latin1.GetBytes(name);
        bytes.AsSpan(0, Math.Min(bytes.Length, AmstradCpcTapeConstants.FileNameLength))
            .CopyTo(header[AmstradCpcTapeConstants.FileNameOffset..]);
    }

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

    private static int ReadSampleRate(IReadOnlyDictionary<string, string> metadata) =>
        metadata.TryGetValue("sampleRate", out var value)
        && int.TryParse(value, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var sampleRate)
        && sampleRate >= 8000
            ? sampleRate
            : DefaultSampleRate;

    private static string ReadText(IReadOnlyDictionary<string, string> metadata, string key, string defaultValue) =>
        metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : defaultValue;

    private static byte ReadByte(IReadOnlyDictionary<string, string> metadata, string key, byte defaultValue) =>
        metadata.TryGetValue(key, out var value)
        && byte.TryParse(value, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var parsed)
            ? parsed : defaultValue;

    private static ushort ReadUInt16(IReadOnlyDictionary<string, string> metadata, string key, ushort defaultValue = 0) =>
        metadata.TryGetValue(key, out var value)
        && ushort.TryParse(value, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var parsed)
            ? parsed : defaultValue;

    private static bool ReadBoolean(IReadOnlyDictionary<string, string> metadata, string key, bool defaultValue) =>
        metadata.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed) ? parsed : defaultValue;

    private static void WriteUInt24(Span<byte> destination, int value)
    {
        destination[0] = (byte)value;
        destination[1] = (byte)(value >> 8);
        destination[2] = (byte)(value >> 16);
    }
}
