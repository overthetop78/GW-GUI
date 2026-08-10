using System.Buffers.Binary;
using GWGUI.Scp.Images;
using GWGUI.Scp.Recognition.Definitions;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Containers.Apple.TwoImg;

internal static class TwoImgReader
{
    public static SectorImage Read(byte[] container)
    {
        if (container.Length < 64) throw new InvalidDataException("The 2IMG header is truncated.");
        var headerLength = checked((int)BinaryPrimitives.ReadUInt16LittleEndian(container.AsSpan(8)));
        var imageFormat = BinaryPrimitives.ReadUInt32LittleEndian(container.AsSpan(12));
        var dataOffset = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(container.AsSpan(24)));
        var dataLength = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(container.AsSpan(28)));
        if (headerLength < 64 || dataOffset < headerLength || dataLength <= 0 ||
            dataOffset > container.Length - dataLength)
            throw new InvalidDataException("The 2IMG data range is invalid.");
        if (imageFormat == 2)
            return AppleNibbleImageDecoder.ReadNib(container.AsSpan(dataOffset, dataLength));
        if (imageFormat > 2)
            throw new NotSupportedException("The 2IMG image format is not supported.");
        return AppleRawImageReader.Read(container.AsSpan(dataOffset, dataLength).ToArray(),
            imageFormat == 0 ? DiskImageFileExtensions.Do : DiskImageFileExtensions.Po);
    }
}
