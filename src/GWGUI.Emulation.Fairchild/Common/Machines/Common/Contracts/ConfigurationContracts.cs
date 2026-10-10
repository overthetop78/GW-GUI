using System.Text.Json.Serialization;

namespace GWGUI.Emulation.Fairchild.Common.Machines.Common.Contracts;

internal sealed record ConfigurationDocument(int SchemaVersion, Guid Id, MachineModel Model,
    Emulator Core, IReadOnlyList<FirmwareConfiguration> Firmwares, IReadOnlyList<MediaConfiguration> Media,
    IReadOnlyDictionary<string, string> Options, InputConfiguration Input, FolderConfiguration Folders,
    bool AudioEnabled, IReadOnlyList<EmulationMediaSlot> ConfiguredSlots);

public sealed record FolderConfiguration(string? Shared = null, string? Cartridges = null,
    string? Saves = null, string? States = null, string? Captures = null);

public sealed record MachineConfiguration : IEmulationConfiguration
{
    [JsonConstructor]
    public MachineConfiguration(int schemaVersion, Guid id, MachineModel model, Emulator core,
        IReadOnlyList<FirmwareConfiguration> firmwares, IReadOnlyList<MediaConfiguration> media,
        IReadOnlyDictionary<string, string> options, InputConfiguration input, bool audioEnabled,
        FolderConfiguration folders, IReadOnlyList<EmulationMediaSlot> configuredSlots)
        : this(model, firmwares, media, options, input, id, schemaVersion, audioEnabled, folders, core, configuredSlots) { }

    public MachineConfiguration(MachineModel model, IReadOnlyList<FirmwareConfiguration>? firmwares = null,
        IReadOnlyList<MediaConfiguration>? media = null, IReadOnlyDictionary<string, string>? options = null,
        InputConfiguration? input = null, Guid id = default,
        int schemaVersion = ConfigurationStoreConstants.CurrentSchemaVersion, bool audioEnabled = true,
        FolderConfiguration? folders = null, Emulator? core = null,
        IReadOnlyList<EmulationMediaSlot>? configuredSlots = null)
    {
        Model = model;
        Core = core ?? ConfigurationFunctions.DefaultEmulator(model);
        Firmwares = firmwares?.ToArray() ?? [];
        Media = media?.ToArray() ?? [];
        Options = options is null ? new Dictionary<string, string>(StringComparer.Ordinal)
            : new Dictionary<string, string>(options, StringComparer.Ordinal);
        Input = input ?? new InputConfiguration();
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        SchemaVersion = schemaVersion;
        AudioEnabled = audioEnabled;
        Folders = folders ?? new FolderConfiguration();
        ConfiguredSlots = configuredSlots?.ToArray() ?? [EmulationMediaSlot.Cartridge0];
        ConfigurationFunctions.Validate(this);
    }

    public string ModuleId => EmulationModuleConstants.ModuleId;
    public int SchemaVersion { get; init; }
    public Guid Id { get; init; }
    public MachineModel Model { get; }
    public string MachineId => Model.ToString();
    public MachineFamily Family => MachineFamily.ChannelF;
    public Emulator Core { get; init; }
    public IReadOnlyList<FirmwareConfiguration> Firmwares { get; init; }
    public IReadOnlyList<MediaConfiguration> Media { get; init; }
    public IReadOnlyDictionary<string, string> Options { get; init; }
    public InputConfiguration Input { get; init; }
    public bool AudioEnabled { get; init; }
    public FolderConfiguration Folders { get; init; }
    public IReadOnlyList<EmulationMediaSlot> ConfiguredSlots { get; init; }
}
