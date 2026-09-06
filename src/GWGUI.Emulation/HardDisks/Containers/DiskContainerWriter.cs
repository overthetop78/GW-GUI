namespace GWGUI.Emulation.HardDisks.Containers;

public static class DiskContainerWriter
{
    public static void Write(Stream destination, long capacity, DiskContainerKind kind,
        Action<Stream>? initialize = null, bool fixedSize = false)
    {
        switch (kind)
        {
            case DiskContainerKind.Raw:
                RawHardDiskImageWriter.Write(destination, capacity, initialize); break;
            case DiskContainerKind.Gzip: GzipImageWriter.Write(destination, capacity, initialize); break;
            case DiskContainerKind.Vhd: VhdImageWriter.Write(destination, capacity, fixedSize, initialize); break;
            case DiskContainerKind.Vhdx: VhdxImageWriter.Write(destination, capacity, fixedSize, initialize); break;
            case DiskContainerKind.Vdi: VdiImageWriter.Write(destination, capacity, fixedSize, initialize); break;
            case DiskContainerKind.Vmdk: VmdkImageWriter.Write(destination, capacity, initialize); break;
            case DiskContainerKind.Qcow2: Qcow2ImageWriter.Write(destination, capacity, initialize); break;
            case DiskContainerKind.TwoImg: TwoImgImageWriter.Write(destination, capacity, initialize); break;
            case DiskContainerKind.Qed: QedImageWriter.Write(destination, capacity, initialize); break;
            case DiskContainerKind.Chd: ChdImageWriter.Write(destination, capacity, initialize); break;
            case DiskContainerKind.Parallels: ParallelsImageWriter.Write(destination, capacity, initialize); break;
            case DiskContainerKind.Qcow: QcowImageWriter.Write(destination, capacity, initialize); break;
            case DiskContainerKind.Hdi: HdiImageWriter.Write(destination, capacity, initialize); break;
            case DiskContainerKind.Nhd: NhdImageWriter.Write(destination, capacity, initialize); break;
            case DiskContainerKind.Thd: ThdImageWriter.Write(destination, capacity, initialize); break;
            default: throw new ArgumentOutOfRangeException(nameof(kind));
        }
        destination.Flush();
    }
}
