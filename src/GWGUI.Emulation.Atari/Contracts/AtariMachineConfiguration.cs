using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Contracts;

public sealed record AtariMachineConfiguration : IEmulationConfiguration
{
    public AtariMachineConfiguration(
        AtariMachineModel model,
        IReadOnlyList<AtariFirmwareConfiguration>? firmwares = null,
        IReadOnlyList<AtariMediaConfiguration>? media = null,
        IReadOnlyDictionary<string, string>? options = null,
        AtariInputConfiguration? input = null,
        Guid id = default,
        int schemaVersion = AtariConstants.CurrentConfigurationSchemaVersion,
        bool audioEnabled = true,
        AtariFolderConfiguration? folders = null)
    {
        Model = model;
        Family = AtariConfigurationFunctions.GetFamily(model);
        Core = AtariConfigurationFunctions.GetCore(model);
        Firmwares = firmwares?.ToArray() ?? [];
        Media = media?.ToArray() ?? [];
        Options = options is null ? new Dictionary<string, string>() : new Dictionary<string, string>(options);
        Input = input ?? new AtariInputConfiguration();
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        SchemaVersion = schemaVersion;
        AudioEnabled = audioEnabled;
        Folders = folders ?? new AtariFolderConfiguration();
        AtariConfigurationFunctions.Validate(SchemaVersion, Model, Firmwares, Media, Input);
    }

    public string ModuleId => AtariMachineConfigurationConstants.Atari;
    public int SchemaVersion { get; init; }
    public Guid Id { get; init; }
    public AtariMachineModel Model { get; }
    public string MachineId => Model.ToString();
    public AtariMachineFamily Family { get; }
    public AtariEmulator Core { get; }
    public IReadOnlyList<AtariFirmwareConfiguration> Firmwares { get; init; }
    public IReadOnlyList<AtariMediaConfiguration> Media { get; init; }
    public IReadOnlyDictionary<string, string> Options { get; init; }
    public AtariInputConfiguration Input { get; init; }
    public bool AudioEnabled { get; init; }
    public AtariFolderConfiguration Folders { get; init; }
}
