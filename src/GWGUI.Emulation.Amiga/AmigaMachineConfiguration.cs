namespace GWGUI.Emulation.Amiga;

public enum AmigaCoreKind { External }

public sealed record AmigaMachineConfiguration(
    string Model,
    string KickstartPath,
    string? InitialDiskPath = null,
    string? ExtendedRomPath = null,
    string? RomKeyPath = null,
    AmigaCoreKind Core = AmigaCoreKind.External,
    IReadOnlyDictionary<string, string>? Options = null,
    Guid Id = default,
    bool AudioEnabled = true)
{
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
