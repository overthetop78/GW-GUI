namespace GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Contracts;

public sealed record AmigaModel(
    string Id,
    string DisplayName,
    string BackendModel,
    IReadOnlyList<CpuModel> CpuModels,
    ChipsetModel ChipsetType,
    RamCapacity ChipMemory,
    RamCapacity SlowMemory,
    RamCapacity FastMemory,
    bool HasCdDrive,
    KickstartVersion RecommendedKickstart,
    DriveCapacity FloppyDriveCapacity = DriveCapacity.FourDrives,
    bool SupportsHardDrives = true,
    DriveCapacity HardDriveCapacity = DriveCapacity.SingleDrive,
    int MouseButtonCount = HardwareConstants.StandardMouseButtons,
    bool SupportsCd32Controller = false,
    int ControllerPortCount = HardwareConstants.StandardControllerPorts,
    bool HasBuiltInFloppyDrive = true,
    bool HasKeyboard = true)
    : Model(Id, DisplayName, CpuModels, [ChipsetType],
        (int)ChipMemory + (int)SlowMemory + (int)FastMemory,
        FloppyDriveCapacity, SupportsHardDrives ? HardDriveCapacity : DriveCapacity.None,
        MouseButtonCount, ControllerPortCount, HasCdDrive, HasBuiltInFloppyDrive,
        HasKeyboard, SupportsCd32Controller ? [PeripheralCategory.Cd32Pad, PeripheralCategory.Joystick, PeripheralCategory.AnalogJoystick, PeripheralCategory.None] : [PeripheralCategory.Joystick, PeripheralCategory.AnalogJoystick, PeripheralCategory.None])
{
    public int ChipMemoryKib => (int)ChipMemory;
    public int SlowMemoryKib => (int)SlowMemory;
    public int FastMemoryMib => (int)FastMemory / MemoryConstants.KibPerMib;
}
