namespace GWGUI.MediaEngine.FileSystems.Fat12;

internal sealed record Fat12WritableLayout(int TotalSectors, int SectorsPerCluster, int FatCount, int SectorsPerFat, int RootEntries, int RootStart, int RootSectors, int DataStart, int ClusterCount, byte MediaDescriptor)
{
    public int ClusterByteCount => SectorsPerCluster * FatBootSectorLayout.SectorSize;
}
