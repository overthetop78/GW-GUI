namespace GWGUI.Emulation.Contracts;

internal sealed record EmulationConfigurationFirmwareDocument<TModel, TCore, TFirmware, TMedia, TInput>(int SchemaVersion, Guid Id, TModel Model, TCore Core, TFirmware Firmware, IReadOnlyList<TMedia> Media, IReadOnlyDictionary<string, string> Options, TInput Input, bool AudioEnabled, IReadOnlyList<EmulationMediaSlot> ConfiguredSlots);
