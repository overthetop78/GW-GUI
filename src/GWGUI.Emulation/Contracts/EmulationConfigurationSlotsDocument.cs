namespace GWGUI.Emulation.Contracts;

/// <summary>Document de configuration avec liste de firmwares, dossiers et lecteurs configurés.</summary>
/// <remarks>Les slots décrivent les lecteurs configurés, indépendamment des médias effectivement insérés. Le module conserve la responsabilité du schéma et de la conversion des chemins.</remarks>
public sealed record EmulationConfigurationSlotsDocument<TModel, TCore, TFirmware, TMedia, TInput, TFolders>(int SchemaVersion, Guid Id, TModel Model,
    TCore Core, IReadOnlyList<TFirmware> Firmwares, IReadOnlyList<TMedia> Media,
    IReadOnlyDictionary<string, string> Options, TInput Input, TFolders Folders,
    bool AudioEnabled, IReadOnlyList<EmulationMediaSlot> ConfiguredSlots);
