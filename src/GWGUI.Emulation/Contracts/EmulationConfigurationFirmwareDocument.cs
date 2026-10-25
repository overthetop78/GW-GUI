namespace GWGUI.Emulation.Contracts;

/// <summary>Variante du document existant avec une configuration de firmware unique et les slots configurés.</summary>
/// <remarks>Conserve la propriété Firmware au singulier pour relire les fichiers des modules qui utilisent ce schéma.</remarks>
public sealed record EmulationConfigurationFirmwareDocument<TModel, TCore, TFirmware, TMedia, TInput>(int SchemaVersion, Guid Id, TModel Model, TCore Core, TFirmware Firmware, IReadOnlyList<TMedia> Media, IReadOnlyDictionary<string, string> Options, TInput Input, bool AudioEnabled, IReadOnlyList<EmulationMediaSlot> ConfiguredSlots);
