namespace GWGUI.Emulation.Amiga.Common.Machines.Common.Contracts;

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
    int SchemaVersion = ConfigurationStoreConstants.CurrentSchemaVersion,
    string? ValidatedCoreSha256 = null,
    IReadOnlyList<MediaConfiguration>? Media = null,
    AudioConfiguration? Audio = null)
    : GWGUI.Emulation.Interfaces.IEmulationConfiguration
{
    public string ModuleId => MachineConfigurationConstants.ModuleId;
    string GWGUI.Emulation.Interfaces.IEmulationConfiguration.MachineId => Model;
    public static MachineConfiguration A500(string kickstartPath, string? diskPath = null) =>
        new(MachineConfigurationConstants.A500, kickstartPath, diskPath, Options: new Dictionary<string, string>
        {
            [SettingsConstants.OptionModel] = MachineConfigurationConstants.A500,
            [SettingsConstants.OptionVideoStandard] = MachineConfigurationConstants.PAL,
            [SettingsConstants.OptionFloppyMultidrive] = MachineConfigurationConstants.Disabled,
            [SettingsConstants.OptionFloppyWriteProtection] = MachineConfigurationConstants.Disabled
        }, Id: Guid.NewGuid());

    public MachineConfiguration EnsureId() => Id == Guid.Empty ? this with { Id = Guid.NewGuid() } : this;
}
