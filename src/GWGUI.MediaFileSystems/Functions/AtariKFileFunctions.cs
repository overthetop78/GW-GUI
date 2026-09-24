using GWGUI.MediaFileSystems.Constants;
using GWGUI.MediaFileSystems.Interfaces;

namespace GWGUI.MediaFileSystems.Functions;

/// <summary>Fonctions de reconnaissance des disquettes Atari K-file utilisant KBoot.</summary>
internal static class AtariKFileFunctions
{
    public static bool IsKBootFile(IMediaSectorImage image)
    {
        if (image.BlockSize != AtariKFileConstants.SectorSize
            || image.BlockCount <= AtariKFileConstants.BootSectorCount
            || !TryReadBootSectors(image, out var header))
            return false;

        if (header[AtariKFileConstants.BootFlagOffset] != 0
            || header[AtariKFileConstants.BootSectorCountOffset] != AtariKFileConstants.BootSectorCount
            || ReadUInt16(header, AtariKFileConstants.BootLoadAddressOffset) != AtariKFileConstants.BootLoadAddress
            || header[AtariKFileConstants.JumpOpcodeOffset] != AtariKFileConstants.JumpOpcode
            || header[AtariKFileConstants.JumpAddressLowOffset] != AtariKFileConstants.JumpAddressLow
            || header[AtariKFileConstants.JumpAddressHighOffset] != AtariKFileConstants.JumpAddressHigh)
            return false;

        var payloadLength = PayloadLength(header);
        if (payloadLength <= 0
            || payloadLength > (long)(image.BlockCount - AtariKFileConstants.BootSectorCount) * image.BlockSize)
            return false;

        return TryReadPayload(image, payloadLength, out var payload) && HasAtariExecutableHeader(payload);
    }

    private static bool TryReadBootSectors(IMediaSectorImage image, out IReadOnlyList<byte> header)
    {
        header = [];
        for (var logicalBlock = 0; logicalBlock < AtariKFileConstants.BootSectorCount; logicalBlock++)
        {
            if (!image.TryGetBlock(logicalBlock, out var block)
                || block.Data.Count != AtariKFileConstants.SectorSize)
                return false;

            if (logicalBlock == 0)
                header = block.Data;
        }

        return true;
    }

    private static int PayloadLength(IReadOnlyList<byte> header) =>
        header[AtariKFileConstants.PayloadLengthOffset]
        | header[AtariKFileConstants.PayloadLengthOffset + 1] << (sizeof(byte) * 8)
        | header[AtariKFileConstants.PayloadLengthOffset + 2] << (sizeof(byte) * 16);

    private static bool TryReadPayload(IMediaSectorImage image, int payloadLength, out byte[] payload)
    {
        payload = new byte[payloadLength];
        var written = 0;
        for (var logicalBlock = AtariKFileConstants.BootSectorCount; written < payloadLength; logicalBlock++)
        {
            if (logicalBlock >= image.BlockCount || !image.TryGetBlock(logicalBlock, out var block)) return false;
            var count = Math.Min(block.Data.Count, payloadLength - written);
            block.Data.Take(count).ToArray().CopyTo(payload, written);
            written += count;
        }
        return true;
    }

    private static bool HasAtariExecutableHeader(IReadOnlyList<byte> data) =>
        data.Count >= AtariKFileConstants.MinimumExecutableLength
        && ReadUInt16(data, 0) == AtariKFileConstants.ExecutableMarker;

    private static int ReadUInt16(IReadOnlyList<byte> data, int offset) =>
        data[offset] | data[offset + 1] << (sizeof(byte) * 8);
}
