using System.Text.Json.Serialization;
using GWGUI.Emulation.Apple.Common.Constants;
using GWGUI.Emulation.Apple.Common.Machines.Common.Enums;
using GWGUI.Emulation.Apple.Common.Machines.Common.Functions;

namespace GWGUI.Emulation.Apple.Common.Machines.Common.Contracts;



public sealed record FolderConfiguration(string? Shared = null, string? Floppies = null, string? HardDisks = null, string? States = null, string? Captures = null);

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
        if (!Enum.IsDefined(model)) throw new ArgumentOutOfRangeException(nameof(model));
        Model = model;
        Core = core ?? ConfigurationFunctions.DefaultEmulator(model);
        if (!Enum.IsDefined(Core)) throw new ArgumentOutOfRangeException(nameof(core));
        Firmwares = firmwares?.ToArray() ?? [];
        Media = media?.ToArray() ?? [];
        Options = options is null ? new Dictionary<string, string>(StringComparer.Ordinal) : new Dictionary<string, string>(options, StringComparer.Ordinal);
        Input = input ?? new InputConfiguration();
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        SchemaVersion = schemaVersion;
        AudioEnabled = audioEnabled;
        Folders = folders ?? new FolderConfiguration();
        if (new Engine().SelectedAdapter(this) is IEmulatorConfiguration adapter) adapter.InitializeOptions(this, (Dictionary<string, string>)Options);
        ConfigurationFunctions.Validate(this);
    }

    public string ModuleId => EmulationModuleConstants.ModuleId;
    public int SchemaVersion { get; init; }
    public Guid Id { get; init; }
    public MachineModel Model { get; }
    public string MachineId => Model.ToString();
    public MachineFamily Family => Model switch
    {
        MachineModel.Pippin => MachineFamily.Pippin,
        MachineModel.AppleII or MachineModel.AppleIIPlus or MachineModel.AppleIIJPlus
            or MachineModel.AppleIIe or MachineModel.AppleIIeEnhanced => MachineFamily.AppleII,
        _ => MachineFamily.Macintosh
    };
    public Emulator Core { get; init; }
    public IReadOnlyList<FirmwareConfiguration> Firmwares { get; init; }
    public IReadOnlyList<MediaConfiguration> Media { get; init; }
    public IReadOnlyDictionary<string, string> Options { get; init; }
    public InputConfiguration Input { get; init; }
    public bool AudioEnabled { get; init; }
    public FolderConfiguration Folders { get; init; }
}
