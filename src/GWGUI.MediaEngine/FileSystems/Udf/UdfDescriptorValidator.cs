using System.Buffers.Binary;
using GWGUI.MediaEngine.Constants;

namespace GWGUI.MediaEngine.FileSystems.Udf;

/// <summary>Validates ECMA-167 descriptor tags before their contents are consumed.</summary>
internal static class UdfDescriptorValidator
{
    public static void Validate(ReadOnlySpan<byte> descriptor, ushort expectedTagId, uint? expectedLocation)
    {
        if (descriptor.Length < UdfConstants.DescriptorTagSize)
            throw new InvalidDataException("The UDF descriptor tag is incomplete.");
        var tagId = BinaryPrimitives.ReadUInt16LittleEndian(descriptor[UdfConstants.DescriptorTagIdOffset..]);
        if (tagId != expectedTagId)
            throw new InvalidDataException($"Expected UDF descriptor tag {expectedTagId}, found {tagId}.");
        var location = BinaryPrimitives.ReadUInt32LittleEndian(descriptor[UdfConstants.DescriptorTagLocationOffset..]);
        if (expectedLocation is not null && location != expectedLocation.Value)
            throw new InvalidDataException($"The UDF descriptor tag location {location} does not match block {expectedLocation}.");

        byte checksum = 0;
        for (var index = 0; index < UdfConstants.DescriptorTagSize; index++)
        {
            if (index != UdfConstants.DescriptorTagChecksumOffset) checksum += descriptor[index];
        }
        if (checksum != descriptor[UdfConstants.DescriptorTagChecksumOffset])
            throw new InvalidDataException("The UDF descriptor tag checksum is invalid.");

        var crcLength = BinaryPrimitives.ReadUInt16LittleEndian(descriptor[UdfConstants.DescriptorTagCrcLengthOffset..]);
        if (crcLength > descriptor.Length - UdfConstants.DescriptorTagSize)
            throw new InvalidDataException("The UDF descriptor CRC length exceeds its block.");
        var expectedCrc = BinaryPrimitives.ReadUInt16LittleEndian(descriptor[UdfConstants.DescriptorTagCrcOffset..]);
        var actualCrc = ComputeCrc16(descriptor.Slice(UdfConstants.DescriptorTagSize, crcLength));
        if (actualCrc != expectedCrc)
            throw new InvalidDataException("The UDF descriptor CRC is invalid.");
    }

    private static ushort ComputeCrc16(ReadOnlySpan<byte> value)
    {
        ushort crc = 0;
        foreach (var current in value)
        {
            crc ^= (ushort)(current << 8);
            for (var bit = 0; bit < 8; bit++)
                crc = (ushort)((crc & 0x8000) != 0 ? (crc << 1) ^ 0x1021 : crc << 1);
        }
        return crc;
    }
}
