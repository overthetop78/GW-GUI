namespace GWGUI.Emulation;

public sealed record FloppyDriveSettings(
    string Model,
    string Speed,
    bool WriteProtected,
    bool RedirectWrites);
