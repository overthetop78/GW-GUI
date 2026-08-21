namespace GWGUI.Emulation.Amiga;

public sealed record AmigaMachineConfiguration(
    string Model,
    string KickstartPath,
    string? InitialDiskPath = null,
    string? ExtendedRomPath = null,
    string? RomKeyPath = null,
    AmigaEmulator Core = AmigaEmulator.External,
    IReadOnlyDictionary<string, string>? Options = null,
    Guid Id = default,
    bool AudioEnabled = true,
    IReadOnlyList<AmigaControllerType>? Controllers = null,
    AmigaInputConfiguration? Input = null,
    IReadOnlyList<AmigaFloppyConfiguration>? Floppies = null,
    bool MountFloppiesInSeparateDrives = false,
    int SchemaVersion = 3,
    string? ValidatedCoreSha256 = null,
    IReadOnlyList<AmigaMediaConfiguration>? Media = null,
    AmigaAudioConfiguration? Audio = null,
    GWGUI.Emulation.EmulationVideoRenderer VideoRenderer = GWGUI.Emulation.EmulationVideoRenderer.Direct3D11)
    : GWGUI.Emulation.IEmulationConfiguration
{
    public string ModuleId => "amiga";
    string GWGUI.Emulation.IEmulationConfiguration.MachineId => Model;
    public static AmigaMachineConfiguration A500(string kickstartPath, string? diskPath = null) =>
        new("A500", kickstartPath, diskPath, Options: new Dictionary<string, string>
        {
            ["puae_model"] = "A500",
            ["puae_video_standard"] = "PAL",
            ["puae_floppy_multidrive"] = "disabled",
            ["puae_floppy_write_protection"] = "disabled"
        }, Id: Guid.NewGuid());

    public AmigaMachineConfiguration EnsureId() => Id == Guid.Empty ? this with { Id = Guid.NewGuid() } : this;
}
