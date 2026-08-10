using System.Buffers.Binary;
using GWGUI.Scp.Images;
using GWGUI.Scp.Recognition.Definitions;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Containers.Apple.DiskCopy;

internal static class DiskCopyReader
{
    public static SectorImage Read(byte[] container)
    {
        const int headerLength = 84;
        if (container.Length < headerLength)
            throw new InvalidDataException("The DiskCopy header is truncated.");
        var dataLength = checked((int)BinaryPrimitives.ReadUInt32BigEndian(container.AsSpan(64)));
        var tagLength = checked((int)BinaryPrimitives.ReadUInt32BigEndian(container.AsSpan(68)));
        if (dataLength <= 0 || headerLength + (long)dataLength + tagLength > container.Length)
            throw new InvalidDataException("The DiskCopy payload is invalid.");
        var payload = container.AsSpan(headerLength, dataLength).ToArray();
        if (AppleDiskImageSignatures.LooksLikeMac(payload) ||
            AppleDiskImageSignatures.LooksLikeProDos(payload))
            return AppleRawImageReader.Read(payload, DiskImageFileExtensions.Image);

        if (tagLength != dataLength / 512 * 12)
            throw new InvalidDataException(
                "The DiskCopy image is neither a recognized Macintosh/ProDOS image nor a tagged Lisa image.");

        var tags = container.AsSpan(headerLength + dataLength, tagLength);
        var blocks = new SectorBlock[dataLength / 512];
        for (var logical = 0; logical < blocks.Length; logical++)
        {
            var address = blocks.Length == 1702
                ? AppleDiskGeometry.LisaFileWareAddress(logical)
                : blocks.Length == 800
                    ? AppleDiskGeometry.AppleMacZonedAddress(logical, 1)
                    : new SectorAddress(logical / 10, 0, logical % 10);
            blocks[logical] = new(logical, address,
                payload.AsSpan(logical * 512, 512).ToArray(),
                Tag: tags.Slice(logical * 12, 12).ToArray());
        }
        var formatId = payload.Length >= 2 * 512 + 16 &&
                       payload.AsSpan(2 * 512, 16).IndexOf("PREBOOT"u8) >= 0
            ? "applelisa.macworks"
            : "applelisa.office";
        var fileWare = blocks.Length == 1702;
        return new(formatId, 512,
            fileWare ? 46 : blocks.Length == 800 ? 80 : Math.Max(1, blocks.Length / 10),
            fileWare ? 2 : 1, fileWare ? 22 : blocks.Length == 800 ? 12 : 10, blocks,
            capacity: dataLength, logicalBlockCount: blocks.Length);
    }
}
