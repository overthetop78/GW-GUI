namespace GWGUI.Emulation.Amiga.Common.Contracts;

public sealed record MachineConfiguration(
    string Model,
    string KickstartPath,
    string? InitialDiskPath = null,
    string? ExtendedRomPath = null,
    string? RomKeyPath = null,
    Emulator Core = Emulator.External,
    IReadOnlyDictionary<string, string>? Options = null,
    Guid Id = default,
    bool AudioEnabled = true,
    IReadOnlyList<ControllerType>? Controllers = null,
    InputConfiguration? Input = null,
    IReadOnlyList<FloppyConfiguration>? Floppies = null,
    bool MountFloppiesInSeparateDrives = false,
    int SchemaVersion = 3,
    string? ValidatedCoreSha256 = null,
    IReadOnlyList<MediaConfiguration>? Media = null,
    AudioConfiguration? Audio = null)
    : GWGUI.Emulation.Interfaces.IEmulationConfiguration
{
    public string ModuleId => MachineConfigurationConstants.Amiga;
    string GWGUI.Emulation.Interfaces.IEmulationConfiguration.MachineId => Model;
    public static MachineConfiguration A500(string kickstartPath, string? diskPath = null) =>
        new(MachineConfigurationConstants.A500, kickstartPath, diskPath, Options: new Dictionary<string, string>
        {
            [MachineConfigurationConstants.OptionModel] = MachineConfigurationConstants.A500,
            [MachineConfigurationConstants.OptionVideoStandard] = MachineConfigurationConstants.PAL,
            [MachineConfigurationConstants.OptionFloppyMultidrive] = MachineConfigurationConstants.Disabled,
            [MachineConfigurationConstants.OptionFloppyWriteProtection] = MachineConfigurationConstants.Disabled
        }, Id: Guid.NewGuid());

    public MachineConfiguration EnsureId() => Id == Guid.Empty ? this with { Id = Guid.NewGuid() } : this;
}
