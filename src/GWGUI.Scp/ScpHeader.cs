using System.Buffers.Binary;

namespace GWGUI.Scp;

public sealed record ScpHeader(byte Version, byte DiskType, byte Revolutions, byte StartTrack, byte EndTrack, byte Flags, byte BitCellEncoding, byte Heads, uint Checksum)
{
    public int TrackCount => EndTrack - StartTrack + 1;
}

public static class ScpHeaderReader
{
    public const int HeaderLength = 16;

    public static ScpHeader Read(ReadOnlySpan<byte> data)
    {
        if (data.Length < HeaderLength) throw new InvalidDataException("SCP header is incomplete.");
        if (data[0] != (byte)'S' || data[1] != (byte)'C' || data[2] != (byte)'P')
            throw new InvalidDataException("The file does not contain an SCP signature.");
        if (data[6] < data[5]) throw new InvalidDataException("The SCP track range is invalid.");
        return new ScpHeader(data[3], data[4], data[7], data[5], data[6], data[8], data[9], data[10], BinaryPrimitives.ReadUInt32LittleEndian(data[12..16]));
    }
}
