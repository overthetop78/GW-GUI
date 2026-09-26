using GWGUI.Emulation;
using System.Text.Json.Serialization;

namespace GWGUI.Emulation.Atari.Common.Machines.Common.Contracts;

internal sealed record ConfigurationDocument(
    int SchemaVersion,
    Guid Id,
    MachineModel Model,
    Emulator Core,
    IReadOnlyList<FirmwareConfiguration> Firmwares,
    IReadOnlyList<MediaConfiguration> Media,
    IReadOnlyDictionary<string, string> Options,
    InputConfiguration Input,
    FolderConfiguration Folders,
    bool AudioEnabled);

public sealed record FolderConfiguration(
    string? Shared = null,
    string? Floppies = null,
    string? Cassettes = null,
    string? Cartridges = null,
    string? CompactDiscs = null,
    string? HardDisks = null,
    string? States = null,
    string? Captures = null);

public sealed record MachineConfiguration : IEmulationConfiguration
{
    [JsonConstructor]
    public MachineConfiguration(
        int schemaVersion,
        Guid id,
        MachineModel model,
        Emulator core,
        IReadOnlyList<FirmwareConfiguration> firmwares,
        IReadOnlyList<MediaConfiguration> media,
        IReadOnlyDictionary<string, string> options,
        InputConfiguration input,
        bool audioEnabled,
        FolderConfiguration folders)
        : this(model, firmwares, media, options, input, id, schemaVersion, audioEnabled, folders, core)
    {
    }

    public MachineConfiguration(
        MachineModel model,
        IReadOnlyList<FirmwareConfiguration>? firmwares = null,
        IReadOnlyList<MediaConfiguration>? media = null,
        IReadOnlyDictionary<string, string>? options = null,
        InputConfiguration? input = null,
        Guid id = default,
        int schemaVersion = ConfigurationStoreConstants.CurrentSchemaVersion,
        bool audioEnabled = true,
        FolderConfiguration? folders = null,
        Emulator? core = null)
    {
        Model = model;
        Family = ConfigurationFunctions.GetFamily(model);
        Core = core ?? ConfigurationFunctions.GetCore(model);
        if (!EmulatorCatalog.GetAll(model.ToString()).Any(entry =>
                entry.Id == EmulatorCatalog.Get(Core).Id))
            throw new ArgumentException(nameof(core));
        Firmwares = firmwares?.ToArray() ?? [];
        Media = media?.ToArray() ?? [];
        Options = options is null ? new Dictionary<string, string>() : new Dictionary<string, string>(options);
        Input = input ?? new InputConfiguration();
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        SchemaVersion = schemaVersion;
        AudioEnabled = audioEnabled;
        Folders = folders ?? new FolderConfiguration();
        ConfigurationFunctions.Validate(SchemaVersion, Model, Firmwares, Media, Input);
    }

    public string ModuleId => MachineConfigurationConstants.ModuleId;
    public int SchemaVersion { get; init; }
    public Guid Id { get; init; }
    public MachineModel Model { get; }
    public string MachineId => Model.ToString();
    public MachineFamily Family { get; }
    public Emulator Core { get; init; }
    public IReadOnlyList<FirmwareConfiguration> Firmwares { get; init; }
    public IReadOnlyList<MediaConfiguration> Media { get; init; }
    public IReadOnlyDictionary<string, string> Options { get; init; }
    public InputConfiguration Input { get; init; }
    public bool AudioEnabled { get; init; }
    public FolderConfiguration Folders { get; init; }
}
