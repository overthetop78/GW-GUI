namespace GWGUI.MediaEngine.FileSystems.Fat12;

public static class Fat12VolumeWriterExceptions
{
    public static InvalidDataException UnsupportedGeometry(string formatId) => new($"The image format '{formatId}' is not a supported FAT12 geometry.");

    public static InvalidDataException DiskFull() => new("The target FAT12 volume does not have enough free clusters or directory entries.");

    public static InvalidDataException InvalidEntry(string path) => new($"The migration entry '{path}' cannot be represented as a FAT12 short-name entry.");
}
