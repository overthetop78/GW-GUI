namespace GWGUI.Emulation.Amiga.Contracts;

public sealed record AmigaModel(
    string Id,
    string DisplayName,
    string BackendModel,
    IReadOnlyList<string> CpuModels,
    string Chipset,
    int ChipMemoryKib,
    int SlowMemoryKib,
    int FastMemoryMib,
    bool HasCdDrive,
    string RecommendedKickstart,
    int MaximumFloppyDrives = 4,
    bool SupportsHardDrives = true,
    int MaximumHardDrives = 1,
    int MouseButtonCount = 2,
    bool SupportsCd32Controller = false,
    int ControllerPortCount = 2,
    bool HasBuiltInFloppyDrive = true)
{
    public string DefaultCpu => CpuModels[0];
}
