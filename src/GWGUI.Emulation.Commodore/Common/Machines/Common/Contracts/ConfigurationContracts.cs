namespace GWGUI.Emulation.Commodore.Common.Machines.Common.Contracts;

public sealed record MachineConfiguration(
    string Model,
    Emulator Core,
    IReadOnlyDictionary<string, string>? FirmwarePaths = null,
    string? InitialDiskPath = null,
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
    AudioConfiguration? Audio = null,
    IReadOnlyList<EmulationMediaSlot>? ConfiguredMediaSlots = null,
    string? HardDiskDirectory = null)
    : GWGUI.Emulation.Interfaces.IEmulationConfiguration
{
    public string ModuleId => EmulationModuleConstants.ModuleId;
    string GWGUI.Emulation.Interfaces.IEmulationConfiguration.MachineId => Model;

    public string FirmwarePath(string fieldId) => FirmwarePaths?.GetValueOrDefault(fieldId) ?? string.Empty;

    public MachineConfiguration WithFirmwarePath(string fieldId, string? path)
    {
        var paths = new Dictionary<string, string>(FirmwarePaths ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(path)) paths.Remove(fieldId);
        else paths[fieldId] = path;
        return this with { FirmwarePaths = paths };
    }

    public MachineConfiguration EnsureId() => Id == Guid.Empty ? this with { Id = Guid.NewGuid() } : this;
}
