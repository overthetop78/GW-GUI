namespace GWGUI.Emulation.HardDisks;

/// <summary>A volume's location and format, independent of its container or consumer.</summary>
public sealed record DiskVolumePlan(long OffsetBytes, long LengthBytes, string FileSystemId = "none",
    string Label = "GWGUI", byte? MbrType = null, Guid? GptType = null, long Attributes = 0,
    bool Active = false, FileSystems.CpmVolumeOptions? CpmOptions = null, string? PartitionType = null,
    int SectorsPerCluster = 0, bool MbrLogical = false, bool AhdiLogical = false, int SectorBytes = 512,
    string? PartitionName = null, DiskChsGeometry? BiosGeometry = null)
{
    public string EffectivePartitionName => PartitionName ?? Label;
}
