using System.Text.Json.Serialization;
using GWGUI.Emulation.Wenquxing.Common.Constants;
using GWGUI.Emulation.Wenquxing.Common.Machines.Common.Functions;

namespace GWGUI.Emulation.Wenquxing.Common.Machines.Common.Contracts;
internal sealed record ConfigurationDocument(
    int SchemaVersion,
    Guid Id,
    MachineModel Model,
    Emulator Core,
    FirmwareConfiguration Firmware,
    IReadOnlyList<MediaConfiguration> Media,
    IReadOnlyDictionary<string, string> Options,
    InputConfiguration Input,
    bool AudioEnabled,
    IReadOnlyList<EmulationMediaSlot> ConfiguredSlots
);
public sealed record MachineConfiguration : IEmulationConfiguration
{
    [JsonConstructor]
    public MachineConfiguration(
        int schemaVersion,
        Guid id,
        MachineModel model,
        Emulator core,
        FirmwareConfiguration firmware,
        IReadOnlyList<MediaConfiguration> media,
        IReadOnlyDictionary<string, string> options,
        InputConfiguration input,
        bool audioEnabled,
        IReadOnlyList<EmulationMediaSlot> configuredSlots
    ) : this(
        model,
        firmware,
        media,
        options,
        input,
        id,
        schemaVersion,
        audioEnabled,
        core,
        configuredSlots
    )
    {
    }

    public MachineConfiguration(
        MachineModel model,
        FirmwareConfiguration? firmware = null,
        IReadOnlyList<MediaConfiguration>? media = null,
        IReadOnlyDictionary<string, string>? options = null,
        InputConfiguration? input = null,
        Guid id = default,
        int schemaVersion = ConfigurationStoreConstants.CurrentSchemaVersion,
        bool audioEnabled = true,
        Emulator core = Emulator.None,
        IReadOnlyList<EmulationMediaSlot>? configuredSlots = null
    )
    {
        Model = model;
        Core = core;
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        SchemaVersion = schemaVersion;
        Firmware = firmware is null ? new(new Dictionary<FirmwareRole, string>()) : new(new Dictionary<FirmwareRole, string>(firmware.Images));
        Media = media?.ToArray() ?? [];
        Options = options is null ? new Dictionary<string, string>(StringComparer.Ordinal) : new Dictionary<string, string>(options, StringComparer.Ordinal);
        Input = input ?? new InputConfiguration();
        AudioEnabled = audioEnabled;
        ConfiguredSlots = configuredSlots?.ToArray() ?? [];
        ConfigurationFunctions.Validate(this);
    }

    public string ModuleId => EmulationModuleConstants.ModuleId;
    public string MachineId => Model.ToString();
    public int SchemaVersion { get; init; }
    public Guid Id { get; init; }
    public MachineModel Model { get; }
    public Emulator Core { get; init; }
    public FirmwareConfiguration Firmware { get; init; }
    public IReadOnlyList<MediaConfiguration> Media { get; init; }
    public IReadOnlyDictionary<string, string> Options { get; init; }
    public InputConfiguration Input { get; init; }
    public bool AudioEnabled { get; init; }
    public IReadOnlyList<EmulationMediaSlot> ConfiguredSlots { get; init; }
}