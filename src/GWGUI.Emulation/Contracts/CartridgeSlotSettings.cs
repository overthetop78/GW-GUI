namespace GWGUI.Emulation.Contracts;

public sealed record CartridgeSlotSettings(bool OptionEnabled = false, IReadOnlyDictionary<string, string>? Options = null);
