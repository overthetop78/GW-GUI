using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.Geometries.Atari;
using GWGUI.MediaEngine.Geometries.Ibm;
using GWGUI.MediaEngine.Geometries.Msx;

namespace GWGUI.MediaEngine.Conversion.Fat12;

public static class Fat12TargetGeometryCatalog
{
    public static bool TryResolve(string formatId, out Fat12TargetGeometry geometry)
    {
        if (AtariStGeometry.TryFromFormatId(formatId, out var atari))
        {
            geometry = new(formatId, AtariStGeometry.SectorSize, atari.Cylinders, atari.Heads, atari.SectorsPerTrack);
            return true;
        }
        if (IbmPcGeometryCatalog.TryFromFormatId(formatId, out var ibm))
        {
            geometry = new(formatId, FatBootSectorLayout.SectorSize, ibm.Cylinders, ibm.Heads, ibm.SectorsPerTrack);
            return true;
        }
        if (MsxDiskGeometryCatalog.TryFromFormatId(formatId, out var msx))
        {
            geometry = new(formatId, FatBootSectorLayout.SectorSize, msx.Cylinders, msx.Heads, msx.SectorsPerTrack);
            return true;
        }
        geometry = default;
        return false;
    }
}
