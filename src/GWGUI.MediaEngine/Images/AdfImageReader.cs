using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Recognition.Definitions;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images;

public sealed class AdfImageReader : ISectorImageReader
{
    public const int AcornDoubleDensityBytes = 819_200;
    public const int AcornDoubleDensityPaddedBytes = AcornDoubleDensityBytes + DataSizeConstants.BytesPerKibibyte;
    public const int DoubleDensityBytes = 901_120;
    public const int HighDensityBytes = 1_802_240;

    public bool CanRead(string path) => Path.GetExtension(path).Equals(DiskImageFileExtensions.Adf, StringComparison.OrdinalIgnoreCase);

    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        if (data.Length is AcornDoubleDensityBytes or AcornDoubleDensityPaddedBytes)
        {
            const int blockSize = 1024;
            const int acornSectorsPerTrack = 5;
            var acornBlocks = new SectorBlock[AcornDoubleDensityBytes / blockSize];
            for (var logical = 0; logical < acornBlocks.Length; logical++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var track = logical / acornSectorsPerTrack;
                acornBlocks[logical] = new(logical, new(track / 2, track % 2, logical % acornSectorsPerTrack),
                    data.AsSpan(logical * blockSize, blockSize).ToArray());
            }
            return new(DiskImageFormatIds.AcornAdfs800, blockSize, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, acornSectorsPerTrack, acornBlocks);
        }
        var sectorsPerTrack = data.Length switch
        {
            DoubleDensityBytes => 11,
            HighDensityBytes => 22,
            _ => throw new InvalidDataException("The ADF image is not an Amiga DD or HD sector image.")
        };
        var blocks = new SectorBlock[data.Length / 512];
        for (var logical = 0; logical < blocks.Length; logical++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var track = logical / sectorsPerTrack;
            blocks[logical] = new(logical, new(track / 2, track % 2, logical % sectorsPerTrack), data.AsSpan(logical * 512, 512).ToArray());
        }
        var formatId = sectorsPerTrack == 22 ? DiskImageFormatIds.AmigaDosHighDensity : DiskImageFormatIds.AmigaDos;
        return new(formatId, 512, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, sectorsPerTrack, blocks);
    }
}
