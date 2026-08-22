namespace GWGUI.Domain.Settings.Emulation;

public sealed class EmulationMediaFolderSettings
{
    public string ModuleId { get; set; } = "";
    public string MachineId { get; set; } = "";
    public EmulationMediaFolderCategory Category { get; set; }
    public string Folder { get; set; } = "";
}
