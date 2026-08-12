using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.Geometries.Apple;

namespace GWGUI.MediaEngine.Images;

internal static class AppleSectorImageFactory
{
    public static SectorImage CreateLinear(byte[] data, string formatId, int blockSize, int cylinders, int heads, int sectorsPerTrack)
    {
        var count = data.Length / blockSize;
        var blocks = new SectorBlock[count];
        for (var logical = 0; logical < count; logical++)
        {
            var perCylinder = heads * sectorsPerTrack;
            blocks[logical] = new(logical,
                new(logical / perCylinder, logical / sectorsPerTrack % heads, logical % sectorsPerTrack),
                data.AsSpan(logical * blockSize, blockSize).ToArray());
        }
        return new(formatId, blockSize, cylinders, heads, sectorsPerTrack, blocks, capacity: data.Length, logicalBlockCount: count);
    }

    public static SectorImage CreateAppleMacZoned(byte[] data, string formatId, int heads)
    {
        var count = data.Length / 512;
        var blocks = new SectorBlock[count];
        for (var logical = 0; logical < count; logical++)
            blocks[logical] = new(logical, MacintoshGcrGeometry.Address(logical, heads),
                data.AsSpan(logical * 512, 512).ToArray());
        return new(formatId, MacintoshGcrGeometry.BlockSize, MacintoshGcrGeometry.CylinderCount, heads, MacintoshGcrGeometry.MaximumSectorsPerTrack, blocks, capacity: data.Length, logicalBlockCount: count);
    }

}
