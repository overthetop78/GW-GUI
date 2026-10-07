namespace GWGUI.Emulation.Commodore.Common.Machines.Common.Contracts;

public record Model(
    string Id,
    string DisplayName,
    IReadOnlyList<CpuModel> CpuModels,
    IReadOnlyList<ChipsetModel> Chipsets,
    int RamKib,
    DriveCapacity FloppyDriveCapacity,
    DriveCapacity HardDriveCapacity,
    int MouseButtonCount,
    int ControllerPortCount,
    bool HasCdDrive,
    bool HasBuiltInFloppyDrive,
    bool HasKeyboard,
    IReadOnlyList<ControllerType> ControllerTypes)
{
    public string Chipset => string.Join(ChipsetDisplayConstants.ComponentSeparator,
        Chipsets.Select(component => component == ChipsetModel.VICII
            ? ChipsetDisplayConstants.VicII : component.ToString()));
    public CpuModel DefaultCpu => CpuModels[BufferConstants.FirstCollectionIndex];
    public int MaximumFloppyDrives => (int)FloppyDriveCapacity;
    public int MaximumHardDrives => (int)HardDriveCapacity;
    public bool SupportsHardDrives => HardDriveCapacity != DriveCapacity.None;
}
