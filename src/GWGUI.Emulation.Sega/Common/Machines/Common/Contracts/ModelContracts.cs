namespace GWGUI.Emulation.Sega.Common.Machines.Common.Contracts;

public sealed record Model(
    string Id,
    string DisplayName,
    string BackendModel,
    int RamKib,
    bool HasKeyboard,
    int BuiltInFloppyDriveCount,
    int MaximumFloppyDriveCount,
    bool HasBuiltInCassetteDrive,
    bool SupportsCassetteDrive,
    bool HasBuiltInCartridgeSlot,
    bool SupportsCartridgeSlot,
    int ControllerPortCount = 2,
    int MouseButtonCount = 2,
    bool HasBuiltInCompactDiscDrive = false,
    bool SupportsCompactDiscDrive = false,
    IReadOnlyList<string>? CpuModels = null,
    string? VideoChip = null,
    string? AudioChip = null,
    int RomKib = 0,
    bool HasBuiltInSegaCardSlot = false,
    bool SupportsSegaCardSlot = false,
    bool SupportsThreeDGlasses = false,
    string? CpuFrequency = null,
    bool SupportsRamConfiguration = false,
    int? RamBytes = null)
{
    public IReadOnlyList<string> Processors => CpuModels ?? [];
}
