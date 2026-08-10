using System.Buffers.Binary;
using GWGUI.MediaEngine.Recognition.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images;

public sealed class AtrImageReader : ISectorImageReader
{
    public bool CanRead(string path) => Path.GetExtension(path).Equals(DiskImageFileExtensions.Atr, StringComparison.OrdinalIgnoreCase);

    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await ReadValidatedContainerAsync(path, cancellationToken).ConfigureAwait(false);
        var sectorSize = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(4));
        var payload = data.Length - 16; var bootBytes = sectorSize == 128 ? 0 : 3 * 128;
        var sectorCount = (sectorSize == 128 ? 0 : 3) + (payload - bootBytes) / sectorSize;
        var blocks = new List<SectorBlock>(sectorCount); var offset = 16;
        for (var sector = 1; sector <= sectorCount; sector++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var length = sector <= 3 ? 128 : sectorSize;
            blocks.Add(new(sector - 1, new(sector - 1, 0, sector), data.AsSpan(offset, length).ToArray())); offset += length;
        }
        var formatId = (sectorSize, sectorCount) switch
        {
            (128, 720) => DiskImageFormatIds.Atari90,
            (128, 1040) => DiskImageFormatIds.Atari130,
            (256, 720) => DiskImageFormatIds.Atari180,
            _ => DiskImageFormatIds.AtariAtr(sectorSize, sectorCount)
        };
        return new(formatId, sectorSize, sectorCount, 1, 1, blocks, allowVariableBlockSize: sectorSize != 128, capacity: payload);
    }

    public static async Task WriteRawPayloadAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default)
    {
        var data = await ReadValidatedContainerAsync(sourcePath, cancellationToken).ConfigureAwait(false);
        await File.WriteAllBytesAsync(destinationPath, data.AsMemory(16), cancellationToken).ConfigureAwait(false);
    }

    private static async Task<byte[]> ReadValidatedContainerAsync(string path, CancellationToken cancellationToken)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        if (data.Length < 16 || BinaryPrimitives.ReadUInt16LittleEndian(data) != 0x0296) throw new InvalidDataException("The ATR header is invalid.");
        var sectorSize = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(4));
        if (sectorSize is not (128 or 256 or 512)) throw new InvalidDataException("The ATR sector size is not supported.");
        var declared = ((long)BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(6)) << 16 | BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(2))) * 16;
        if (declared != data.Length - 16) throw new InvalidDataException("The ATR payload length does not match its header.");
        var payload = data.Length - 16; var bootBytes = sectorSize == 128 ? 0 : 3 * 128;
        if (payload < bootBytes || (payload - bootBytes) % sectorSize != 0) throw new InvalidDataException("The ATR sector data is truncated.");
        return data;
    }
}
