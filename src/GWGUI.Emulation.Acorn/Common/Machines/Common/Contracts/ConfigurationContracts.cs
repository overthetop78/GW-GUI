using System.Text.Json.Serialization;
using GWGUI.Emulation.Acorn.Emulators.B2.Constants;
using GWGUI.Emulation.Acorn.Emulators.B2.Functions;

namespace GWGUI.Emulation.Acorn.Common.Machines.Common.Contracts;

internal sealed record ConfigurationDocument(int SchemaVersion, Guid Id, MachineModel Model,
    Emulator Core, IReadOnlyList<FirmwareConfiguration> Firmwares, IReadOnlyList<MediaConfiguration> Media,
    IReadOnlyDictionary<string, string> Options, InputConfiguration Input, FolderConfiguration Folders,
    bool AudioEnabled);

public sealed record FolderConfiguration(string? Shared = null, string? Floppies = null,
    string? States = null, string? Captures = null);

public sealed record MachineConfiguration : IEmulationConfiguration
{
    [JsonConstructor]
    public MachineConfiguration(int schemaVersion, Guid id, MachineModel model, Emulator core,
        IReadOnlyList<FirmwareConfiguration> firmwares, IReadOnlyList<MediaConfiguration> media,
        IReadOnlyDictionary<string, string> options, InputConfiguration input, bool audioEnabled,
        FolderConfiguration folders)
        : this(model, firmwares, media, options, input, id, schemaVersion, audioEnabled, folders, core) { }

    public MachineConfiguration(MachineModel model, IReadOnlyList<FirmwareConfiguration>? firmwares = null,
        IReadOnlyList<MediaConfiguration>? media = null, IReadOnlyDictionary<string, string>? options = null,
        InputConfiguration? input = null, Guid id = default,
        int schemaVersion = ConfigurationStoreConstants.CurrentSchemaVersion, bool audioEnabled = true,
        FolderConfiguration? folders = null, Emulator? core = null)
    {
        Model = model;
        Core = core ?? Emulator.B2;
        Firmwares = firmwares?.ToArray() ?? [];
        Media = media?.ToArray() ?? [];
        var selectedOptions = options is null ? new Dictionary<string, string>(StringComparer.Ordinal) : new Dictionary<string, string>(options, StringComparer.Ordinal);
        if (B2ModelFunctions.GetModels(model).Count > 0)
        {
            selectedOptions.TryAdd(OptionConstants.Model, B2ModelFunctions.DefaultValue(model));
            selectedOptions.TryAdd(OptionConstants.AutoBoot, OptionConstants.Enabled);
        }
        Options = selectedOptions;
        Input = input ?? new InputConfiguration();
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        SchemaVersion = schemaVersion;
        AudioEnabled = audioEnabled;
        Folders = folders ?? new FolderConfiguration();
        ConfigurationFunctions.Validate(this);
    }

    public string ModuleId => MachineConfigurationConstants.ModuleId;
    public int SchemaVersion { get; init; }
    public Guid Id { get; init; }
    public MachineModel Model { get; }
    public string MachineId => Model.ToString();
    public MachineFamily Family => MachineFamily.BbcMicro;
    public Emulator Core { get; init; }
    public IReadOnlyList<FirmwareConfiguration> Firmwares { get; init; }
    public IReadOnlyList<MediaConfiguration> Media { get; init; }
    public IReadOnlyDictionary<string, string> Options { get; init; }
    public InputConfiguration Input { get; init; }
    public bool AudioEnabled { get; init; }
    public FolderConfiguration Folders { get; init; }
}
