namespace GWGUI.MediaEngine.Containers.ImageDisk;

internal enum ImdSection
{
    TrackHeader,
    SectorNumberMap,
    CylinderMap,
    HeadMap,
    SectorSizeMap,
    SectorRecord,
    CompressedValue,
    SectorData
}
