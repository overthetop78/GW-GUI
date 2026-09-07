using GWGUI.Emulation.HardDisks.FileSystems;
using GWGUI.Emulation.HardDisks.Partitioning;

namespace GWGUI.Emulation.HardDisks;

public static class HardDiskPreparationWriter
{
    public static void Prepare(Stream disk, HardDiskPreparation preparation, string label = "GWGUI", string? formatId = null)
    {
        switch (preparation)
        {
            case HardDiskPreparation.Blank: return;
            case HardDiskPreparation.AtariAhdiFat16:
                using (var partition = string.Equals(formatId, "atari-ide", StringComparison.OrdinalIgnoreCase)
                           ? AtariIdePartitionWriter.Create(disk)
                           : AtariAhdiPartitionWriter.Create(disk))
                    AtariFat16VolumeFormatter.Format(partition);
                break;
            case HardDiskPreparation.AmigaOfs: AmigaDosVolumeFormatter.Format(disk, label, false); break;
            case HardDiskPreparation.AmigaFfs: AmigaDosVolumeFormatter.Format(disk, label, true); break;
            case HardDiskPreparation.AmigaRdbOfs:
            case HardDiskPreparation.AmigaRdbFfs:
                using (var partition = AmigaRdbPartitionWriter.Create(disk, preparation == HardDiskPreparation.AmigaRdbFfs))
                    AmigaDosVolumeFormatter.Format(partition, label, preparation == HardDiskPreparation.AmigaRdbFfs);
                break;
            case HardDiskPreparation.FatVolume: FatVolumeFormatter.Format(disk, label); break;
            case HardDiskPreparation.NtfsVolume: NtfsVolumeFormatter.Format(disk, label); break;
            case HardDiskPreparation.MbrFat:
            case HardDiskPreparation.MbrNtfs:
            case HardDiskPreparation.GptFat:
            case HardDiskPreparation.GptNtfs:
                var ntfs = preparation is HardDiskPreparation.MbrNtfs or HardDiskPreparation.GptNtfs;
                var type = ntfs ? DiscUtils.Partitions.WellKnownPartitionType.WindowsNtfs : DiscUtils.Partitions.WellKnownPartitionType.WindowsFat;
                var entry = preparation is HardDiskPreparation.MbrFat or HardDiskPreparation.MbrNtfs
                    ? MbrPartitionWriter.Create(disk, type) : GptPartitionWriter.Create(disk, type);
                using (var partition = entry.Open())
                {
                    if (ntfs) NtfsVolumeFormatter.Format(partition, label, entry.FirstSector);
                    else FatVolumeFormatter.Format(partition, label, entry.FirstSector);
                }
                break;
            default: throw new ArgumentOutOfRangeException(nameof(preparation));
        }
    }
}
