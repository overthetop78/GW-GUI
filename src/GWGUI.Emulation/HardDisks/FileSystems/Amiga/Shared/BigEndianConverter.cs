using System.Buffers.Binary;

namespace Hst.Core.Converters;

internal static class BigEndianConverter
{
    public static short ConvertBytesToInt16(byte[] bytes, int offset = 0)
        => BinaryPrimitives.ReadInt16BigEndian(bytes.AsSpan(offset, 2));

    public static ushort ConvertBytesToUInt16(byte[] bytes, int offset = 0)
        => BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(offset, 2));

    public static int ConvertBytesToInt32(byte[] bytes, int offset = 0)
        => BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(offset, 4));

    public static uint ConvertBytesToUInt32(byte[] bytes, int offset = 0)
        => BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(offset, 4));

    public static void ConvertInt16ToBytes(short value, byte[] bytes, int offset = 0)
        => BinaryPrimitives.WriteInt16BigEndian(bytes.AsSpan(offset, 2), value);

    public static void ConvertUInt16ToBytes(ushort value, byte[] bytes, int offset = 0)
        => BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(offset, 2), value);

    public static void ConvertInt32ToBytes(int value, byte[] bytes, int offset = 0)
        => BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan(offset, 4), value);

    public static void ConvertUInt32ToBytes(uint value, byte[] bytes, int offset = 0)
        => BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(offset, 4), value);
}

internal static class SignedByteConverter
{
    public static sbyte ConvertByteToSignedByte(byte[] bytes, int offset = 0)
        => unchecked((sbyte)bytes[offset]);

    public static void ConvertSignedByteToByte(byte[] bytes, int offset, sbyte value)
        => bytes[offset] = unchecked((byte)value);
}
