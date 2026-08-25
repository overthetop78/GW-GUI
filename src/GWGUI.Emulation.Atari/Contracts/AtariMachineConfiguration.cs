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
        EmulationVideoRenderer videoRenderer = EmulationVideoRenderer.Direct3D11,
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
        VideoRenderer = videoRenderer;
        Folders = folders ?? new AtariFolderConfiguration();
        AtariConfigurationFunctions.Validate(SchemaVersion, Model, Firmwares, Media, Input);
    }

    public string ModuleId => AtariMachineConfigurationConstants.Atari;
    public int SchemaVersion { get; }
    public Guid Id { get; }
    public AtariMachineModel Model { get; }
    public string MachineId => Model.ToString();
    public AtariMachineFamily Family { get; }
    public AtariEmulator Core { get; }
    public IReadOnlyList<AtariFirmwareConfiguration> Firmwares { get; }
    public IReadOnlyList<AtariMediaConfiguration> Media { get; }
    public IReadOnlyDictionary<string, string> Options { get; }
    public AtariInputConfiguration Input { get; }
    public bool AudioEnabled { get; }
    public EmulationVideoRenderer VideoRenderer { get; }
    public AtariFolderConfiguration Folders { get; }
}
