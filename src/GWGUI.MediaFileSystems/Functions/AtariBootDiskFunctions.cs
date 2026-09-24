using GWGUI.MediaFileSystems.Constants;
using GWGUI.MediaFileSystems.Interfaces;

namespace GWGUI.MediaFileSystems.Functions;

/// <summary>Fonctions de reconnaissance des programmes chargés directement depuis les secteurs de démarrage Atari.</summary>
internal static class AtariBootDiskFunctions
{
    public static bool IsCompleteBootProgram(IMediaSectorImage image)
    {
        if (image.BlockCount <= 0
            || !image.TryGetBlock(0, out var firstSector)
            || firstSector.Data.Count != AtariBootDiskConstants.BootSectorSize
            || firstSector.Data.Count < AtariBootDiskConstants.HeaderLength)
            return false;

        var bootSectorCount = firstSector.Data[AtariBootDiskConstants.SectorCountOffset];
        if (bootSectorCount == 0 || bootSectorCount > image.BlockCount)
            return false;

        var loadAddress = ReadUInt16(firstSector.Data, AtariBootDiskConstants.LoadAddressOffset);
        var loadedLength = bootSectorCount * AtariBootDiskConstants.BootSectorSize;
        if (loadAddress + loadedLength > AtariBootDiskConstants.AddressSpaceLength)
            return false;

        for (var logicalBlock = 0; logicalBlock < bootSectorCount; logicalBlock++)
        {
            if (!image.TryGetBlock(logicalBlock, out var sector)
                || sector.Data.Count < AtariBootDiskConstants.BootSectorSize)
                return false;
        }

        return true;
    }

    private static ushort ReadUInt16(IReadOnlyList<byte> data, int offset) =>
        (ushort)(data[offset] | data[offset + 1] << (sizeof(byte) * 8));
}
