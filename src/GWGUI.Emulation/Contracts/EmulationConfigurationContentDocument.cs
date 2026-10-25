namespace GWGUI.Emulation.Contracts;

/// <summary>Document de configuration conservant les lecteurs configurés et un contenu initial facultatif.</summary>
/// <remarks>Les types génériques appartiennent au module. Le module valide SchemaVersion et résout ses chemins lors du chargement ; les noms et valeurs par défaut de ce document font partie du format JSON existant.</remarks>
public sealed record EmulationConfigurationContentDocument<TModel, TCore, TFirmware, TMedia, TInput, TFolders>(int SchemaVersion, Guid Id, TModel Model, TCore Core, IReadOnlyList<TFirmware> Firmwares, IReadOnlyList<TMedia> Media, IReadOnlyDictionary<string, string> Options, TInput Input, TFolders Folders, bool AudioEnabled, IReadOnlyList<EmulationMediaSlot> ConfiguredSlots, string? InitialContent = null);
