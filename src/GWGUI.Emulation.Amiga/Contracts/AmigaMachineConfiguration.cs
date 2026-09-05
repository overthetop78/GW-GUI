namespace GWGUI.Emulation.Amiga.Contracts;

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
    AmigaAudioConfiguration? Audio = null)
    : GWGUI.Emulation.Interfaces.IEmulationConfiguration
{
    public string ModuleId => AmigaMachineConfigurationConstants.Amiga;
    string GWGUI.Emulation.Interfaces.IEmulationConfiguration.MachineId => Model;
    public static AmigaMachineConfiguration A500(string kickstartPath, string? diskPath = null) =>
        new(AmigaMachineConfigurationConstants.A500, kickstartPath, diskPath, Options: new Dictionary<string, string>
        {
            [AmigaMachineConfigurationConstants.OptionModel] = AmigaMachineConfigurationConstants.A500,
            [AmigaMachineConfigurationConstants.OptionVideoStandard] = AmigaMachineConfigurationConstants.PAL,
            [AmigaMachineConfigurationConstants.OptionFloppyMultidrive] = AmigaMachineConfigurationConstants.Disabled,
            [AmigaMachineConfigurationConstants.OptionFloppyWriteProtection] = AmigaMachineConfigurationConstants.Disabled
        }, Id: Guid.NewGuid());

    public AmigaMachineConfiguration EnsureId() => Id == Guid.Empty ? this with { Id = Guid.NewGuid() } : this;
}
