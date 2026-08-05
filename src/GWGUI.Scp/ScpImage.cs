using System.Buffers.Binary;

namespace GWGUI.Scp;

[Flags]
public enum ScpFlags : byte { IndexAligned = 1, Tpi96 = 2, Rpm360 = 4, Normalized = 8, Writable = 16, Footer = 32, Extended = 64, ThirdPartyCreator = 128 }

public sealed record ScpHeader(byte Version, byte DiskType, byte Revolutions, byte StartTrack, byte EndTrack, ScpFlags Flags, byte BitCellEncoding, byte Heads, byte Resolution, uint Checksum)
{
    public int TrackCount => EndTrack - StartTrack + 1;
    public int ResolutionNanoseconds => 25 * (Resolution + 1);
    public string VersionText => $"{Version >> 4}.{Version & 0x0f}";
}

public sealed record ScpRevolution(uint IndexTimeTicks, uint DeclaredFluxCount, IReadOnlyList<uint> FluxIntervals)
{
    public double DurationMilliseconds(int resolutionNanoseconds) => IndexTimeTicks * resolutionNanoseconds / 1_000_000d;
    public double Rpm(int resolutionNanoseconds) => 60_000d / DurationMilliseconds(resolutionNanoseconds);
}

public sealed record ScpTrack(byte TrackNumber, int Cylinder, int Head, IReadOnlyList<ScpRevolution> Revolutions);
public sealed record ScpImage(ScpHeader Header, IReadOnlyList<ScpTrack> Tracks, bool ChecksumValid, long FileSize);

public interface IScpReader
{
    Task<ScpImage> ReadAsync(string path, CancellationToken cancellationToken = default);
}

public sealed class ScpReader : IScpReader
{
    public const int HeaderLength = 16;
    public const int FloppyTrackSlots = 168;
    public const int TrackTableOffset = 0x10;

    public async Task<ScpImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        return Read(data);
    }

    public ScpImage Read(ReadOnlySpan<byte> data)
    {
        var header = ReadHeader(data);
        if ((header.Flags & ScpFlags.Extended) != 0) throw new NotSupportedException("Extended SCP media are not floppy images.");
        var tableBytes = checked(TrackTableOffset + FloppyTrackSlots * 4);
        Require(data, 0, tableBytes, "track-offset table");
        var tracks = new List<ScpTrack>();
        for (var slot = header.StartTrack; slot <= header.EndTrack; slot++)
        {
            var offset = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(TrackTableOffset + slot * 4, 4));
            if (offset == 0) continue;
            tracks.Add(ReadTrack(data, checked((int)offset), slot, header));
        }
        var checksumValid = header.Checksum == 0 && (header.Flags & ScpFlags.Writable) != 0 || ComputeChecksum(data[TrackTableOffset..]) == header.Checksum;
        return new ScpImage(header, tracks, checksumValid, data.Length);
    }

    public static ScpHeader ReadHeader(ReadOnlySpan<byte> data)
    {
        Require(data, 0, HeaderLength, "SCP header");
        if (!data[..3].SequenceEqual("SCP"u8)) throw new InvalidDataException("The file does not contain an SCP signature.");
        if (data[5] is 0 or > 64) throw new InvalidDataException("The SCP revolution count is invalid.");
        if (data[7] < data[6] || data[7] >= FloppyTrackSlots) throw new InvalidDataException("The SCP track range is invalid.");
        if (data[9] is not (0 or 16)) throw new NotSupportedException($"Unsupported SCP bit-cell width: {data[9]}.");
        if (data[10] > 2) throw new InvalidDataException("The SCP head selector is invalid.");
        return new(data[3], data[4], data[5], data[6], data[7], (ScpFlags)data[8], data[9], data[10], data[11], BinaryPrimitives.ReadUInt32LittleEndian(data[12..16]));
    }

    private static ScpTrack ReadTrack(ReadOnlySpan<byte> data, int offset, int expectedTrack, ScpHeader header)
    {
        var descriptorSize = checked(4 + header.Revolutions * 12);
        Require(data, offset, descriptorSize, $"track {expectedTrack} header");
        var trackData = data[offset..];
        if (!trackData[..3].SequenceEqual("TRK"u8)) throw new InvalidDataException($"Track {expectedTrack} has no TRK signature.");
        if (trackData[3] != expectedTrack) throw new InvalidDataException($"Track table entry {expectedTrack} points to track {trackData[3]}.");
        var revolutions = new List<ScpRevolution>(header.Revolutions);
        for (var index = 0; index < header.Revolutions; index++)
        {
            var descriptor = 4 + index * 12;
            var indexTime = BinaryPrimitives.ReadUInt32LittleEndian(trackData.Slice(descriptor, 4));
            var fluxCount = BinaryPrimitives.ReadUInt32LittleEndian(trackData.Slice(descriptor + 4, 4));
            var relativeOffset = BinaryPrimitives.ReadUInt32LittleEndian(trackData.Slice(descriptor + 8, 4));
            var byteCount = checked((int)fluxCount * 2);
            Require(data, checked(offset + (int)relativeOffset), byteCount, $"track {expectedTrack}, revolution {index + 1} flux");
            var fluxBytes = data.Slice(offset + (int)relativeOffset, byteCount);
            var intervals = new List<uint>((int)Math.Min(fluxCount, (uint)int.MaxValue));
            uint overflow = 0;
            for (var position = 0; position < fluxBytes.Length; position += 2)
            {
                var value = BinaryPrimitives.ReadUInt16BigEndian(fluxBytes.Slice(position, 2));
                if (value == 0) { overflow = checked(overflow + 65536); continue; }
                intervals.Add(checked(overflow + value)); overflow = 0;
            }
            if (overflow != 0) intervals.Add(overflow);
            revolutions.Add(new(indexTime, fluxCount, intervals));
        }
        return new((byte)expectedTrack, expectedTrack / 2, expectedTrack % 2, revolutions);
    }

    private static uint ComputeChecksum(ReadOnlySpan<byte> data) { uint sum = 0; foreach (var value in data) sum = unchecked(sum + value); return sum; }
    private static void Require(ReadOnlySpan<byte> data, int offset, int length, string section) { if (offset < 0 || length < 0 || offset > data.Length - length) throw new InvalidDataException($"Incomplete or invalid {section}."); }
}

public static class ScpHeaderReader
{
    public static ScpHeader Read(ReadOnlySpan<byte> data) => ScpReader.ReadHeader(data);
}
