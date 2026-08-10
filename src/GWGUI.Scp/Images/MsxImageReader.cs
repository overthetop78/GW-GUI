using GWGUI.Scp.Recognition.Definitions;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images;

public sealed class MsxImageReader : ISectorImageReader
{
    public bool CanRead(string path) => Path.GetExtension(path).Equals(DiskImageFileExtensions.Dsk, StringComparison.OrdinalIgnoreCase);

    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        if (!LooksLikeMsx(data)) throw new InvalidDataException("The image does not contain an MSX-DOS boot sector.");
        var (format, cylinders, heads, sectors) = data.Length switch
        {
            184_320 => ("msx.1d", 40, 1, 9),
            368_640 when data[21] == 0xf8 => ("msx.1dd", 80, 1, 9),
            368_640 => ("msx.2d", 40, 2, 9),
            737_280 => ("msx.2dd", 80, 2, 9),
            _ => throw new InvalidDataException("The MSX disk geometry is not supported.")
        };
        var blocks = new SectorBlock[data.Length / 512];
        for (var logical = 0; logical < blocks.Length; logical++)
        {
            var track = logical / sectors;
            blocks[logical] = new(logical, new(track / heads, track % heads, logical % sectors + 1),
                data.AsSpan(logical * 512, 512).ToArray());
        }
        return new(format, 512, cylinders, heads, sectors, blocks);
    }

    public static bool LooksLikeMsx(ReadOnlySpan<byte> data)
    {
        if (data.Length < 512 || data.Length % 512 != 0) return false;
        var oem = System.Text.Encoding.ASCII.GetString(data.Slice(3, 8));
        return oem.StartsWith("MSX", StringComparison.OrdinalIgnoreCase)
            && data[11] == 0 && data[12] == 2
            && data[13] > 0 && data[16] > 0;
    }
}
