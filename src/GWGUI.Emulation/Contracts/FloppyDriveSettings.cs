namespace GWGUI.Emulation.Contracts;

public sealed record FloppyDriveSettings(string Model, string Speed, bool WriteProtected, bool RedirectWrites, IReadOnlyDictionary<string, string>? Options = null);
