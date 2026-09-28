namespace GWGUI.Emulation.Sega.Common.Machines.Common.Contracts;

public sealed record MachineConfiguration(
    string Model,
    string EmulatorId,
    IReadOnlyDictionary<string, string>? Options = null,
    Guid Id = default,
    bool AudioEnabled = true,
    IReadOnlyList<ControllerType>? Controllers = null,
    InputConfiguration? Input = null,
    int SchemaVersion = ConfigurationStoreConstants.CurrentSchemaVersion,
    IReadOnlyList<MediaConfiguration>? Media = null,
    AudioConfiguration? Audio = null)
    : GWGUI.Emulation.Interfaces.IEmulationConfiguration
{
    public string ModuleId => MachineConfigurationConstants.ModuleId;
    string GWGUI.Emulation.Interfaces.IEmulationConfiguration.MachineId => Model;
    public string MegaCdModel => Options?.GetValueOrDefault(SettingsConstants.MegaCdModel,
        ModelConstants.MegaCdI) ?? ModelConstants.MegaCdI;
    public bool MegaCdEnabled => Options?.TryGetValue(SettingsConstants.MegaCdEnabled,
        out var megaCdEnabled) == true && (bool.TryParse(megaCdEnabled, out var enabled)
            && enabled || megaCdEnabled.Equals(SettingsDescriptionFunctionsConstants.Enabled,
                StringComparison.OrdinalIgnoreCase));
    public bool ThirtyTwoXEnabled => Options?.TryGetValue(SettingsConstants.MegaDriveThirtyTwoX,
        out var value) == true && (bool.TryParse(value, out var enabled) && enabled
            || value.Equals(SettingsDescriptionFunctionsConstants.Enabled,
                StringComparison.OrdinalIgnoreCase));
    public MachineConfiguration EnsureId() => Id == Guid.Empty ? this with { Id = Guid.NewGuid() } : this;
}
