namespace GWGUI.Emulation.Contracts;

/// <summary>Document commun d’une configuration avec modèle, cœur, firmwares, médias, options, entrées et dossiers.</summary>
/// <remarks>Le schéma et les types concrets restent définis par le module indépendant ; ce contrat ne déplace ni ne convertit les données persistées.</remarks>
public sealed record EmulationConfigurationDocument<TModel, TCore, TFirmware, TMedia, TInput, TFolders>(int SchemaVersion, Guid Id, TModel Model,
    TCore Core, IReadOnlyList<TFirmware> Firmwares, IReadOnlyList<TMedia> Media,
    IReadOnlyDictionary<string, string> Options, TInput Input, TFolders Folders,
    bool AudioEnabled);
