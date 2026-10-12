using System.Text.Json.Serialization;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Thomson.Common.Constants;
using GWGUI.Emulation.Thomson.Common.Machines.Common.Enums;
using GWGUI.Emulation.Thomson.Common.Machines.Common.Functions;

namespace GWGUI.Emulation.Thomson.Common.Machines.Common.Contracts;

internal sealed record ConfigurationDocument(
    int SchemaVersion,
    Guid Id,
    MachineModel Model,
    Emulator Core,
    IReadOnlyList<MediaConfiguration> Media,
    IReadOnlyDictionary<string, string> Options,
    InputConfiguration Input,
    bool AudioEnabled,
    IReadOnlyList<EmulationMediaSlot> ConfiguredSlots,
    string? InitialContent = null
);

public sealed record MachineConfiguration : IEmulationConfiguration
{
    [JsonConstructor]
    public MachineConfiguration(
        int schemaVersion,
        Guid id,
        MachineModel model,
        Emulator core,
        IReadOnlyList<MediaConfiguration> media,
        IReadOnlyDictionary<string, string> options,
        InputConfiguration input,
        bool audioEnabled,
        IReadOnlyList<EmulationMediaSlot> configuredSlots,
        string? initialContent = null
    ) : this(model, media, options, input, id, schemaVersion, audioEnabled, core, configuredSlots, initialContent)
    {
    }

    public MachineConfiguration(
        MachineModel model,
        IReadOnlyList<MediaConfiguration>? media = null,
        IReadOnlyDictionary<string, string>? options = null,
        InputConfiguration? input = null,
        Guid id = default,
        int schemaVersion = ConfigurationStoreConstants.CurrentSchemaVersion,
        bool audioEnabled = true,
        Emulator core = Emulator.None,
        IReadOnlyList<EmulationMediaSlot>? configuredSlots = null,
        string? initialContent = null
    )
    {
        Model = model;
        Core = core;
        Media = media?.ToArray() ?? [];
        Options = options is null ? new Dictionary<string, string>(StringComparer.Ordinal) : new Dictionary<string, string>(options, StringComparer.Ordinal);
        Input = input ?? new InputConfiguration();
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        SchemaVersion = schemaVersion;
        AudioEnabled = audioEnabled;
        ConfiguredSlots = configuredSlots?.ToArray() ?? Media.Select(item => item.Slot).Distinct().ToArray();
        InitialContent = string.IsNullOrWhiteSpace(initialContent) ? null : initialContent;
        ConfigurationFunctions.Validate(this);
    }

    public string ModuleId => EmulationModuleConstants.ModuleId;
    public int SchemaVersion { get; init; }
    public Guid Id { get; init; }
    public MachineModel Model { get; }
    public string MachineId => Model.ToString();
    public MachineFamily Family => ConfigurationFunctions.Family(Model);
    public Emulator Core { get; init; }
    public IReadOnlyList<MediaConfiguration> Media { get; init; }
    public IReadOnlyDictionary<string, string> Options { get; init; }
    public InputConfiguration Input { get; init; }
    public bool AudioEnabled { get; init; }
    public IReadOnlyList<EmulationMediaSlot> ConfiguredSlots { get; init; }
    public string? InitialContent { get; init; }
}
