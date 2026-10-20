namespace GWGUI.Emulation.Contracts;

internal sealed record EmulationConfigurationDocument<TModel, TCore, TFirmware, TMedia, TInput, TFolders>(int SchemaVersion, Guid Id, TModel Model,
    TCore Core, IReadOnlyList<TFirmware> Firmwares, IReadOnlyList<TMedia> Media,
    IReadOnlyDictionary<string, string> Options, TInput Input, TFolders Folders,
    bool AudioEnabled);
