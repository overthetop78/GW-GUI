namespace GWGUI.MediaEngine.FileSystems.Amiga;

public static class AmigaDosVolumeWriterExceptions
{
    public static InvalidDataException UnsupportedVariant(AmigaDosVariant variant) => new($"The AmigaDOS variant '{variant}' cannot be created by this writer.");

    public static InvalidDataException UnsupportedGeometry(string formatId) => new($"The image format '{formatId}' is not a supported AmigaDOS geometry.");

    public static InvalidDataException DiskFull() => new("The target AmigaDOS volume does not have enough free blocks.");

    public static InvalidDataException InvalidEntry(string path) => new($"The migration entry '{path}' cannot be represented on AmigaDOS.");
}
