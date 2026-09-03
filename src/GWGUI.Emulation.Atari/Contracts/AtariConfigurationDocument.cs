using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Contracts;

internal sealed record AtariConfigurationDocument(
    int SchemaVersion,
    Guid Id,
    AtariMachineModel Model,
    AtariEmulator Core,
    IReadOnlyList<AtariFirmwareConfiguration> Firmwares,
    IReadOnlyList<AtariMediaConfiguration> Media,
    IReadOnlyDictionary<string, string> Options,
    AtariInputConfiguration Input,
    AtariFolderConfiguration Folders,
    bool AudioEnabled,
    EmulationVideoRenderer VideoRenderer,
    EmulationVideoProcessingConfiguration? VideoProcessing = null);
