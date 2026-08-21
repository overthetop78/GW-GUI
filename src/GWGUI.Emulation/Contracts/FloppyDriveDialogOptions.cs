namespace GWGUI.Emulation;

public sealed record FloppyDriveDialogOptions(
    IReadOnlyList<FloppyDriveModelChoice> Models,
    string ImageDirectory,
    string ImageFilter,
    string DefaultExtension,
    bool CanCreateBlankMedia = true);
