using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari;

internal sealed record AtariConfigurationDocument(
    int SchemaVersion,
    Guid Id,
    AtariMachineModel Model,
    AtariCoreKind Core,
    IReadOnlyList<AtariFirmwareConfiguration> Firmwares,
    IReadOnlyList<AtariMediaConfiguration> Media,
    IReadOnlyDictionary<string, string> Options,
    AtariInputConfiguration Input,
    AtariFolderConfiguration Folders,
    bool AudioEnabled,
    EmulationVideoRenderer VideoRenderer);
