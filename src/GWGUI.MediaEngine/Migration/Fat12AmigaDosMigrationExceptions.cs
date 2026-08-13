namespace GWGUI.MediaEngine.Migration;

public static class Fat12AmigaDosMigrationExceptions
{
    public static InvalidDataException UnsupportedDirection(string sourceFileSystemId, string targetFileSystemId) => new($"Migration from '{sourceFileSystemId}' to '{targetFileSystemId}' is not a supported FAT12/AmigaDOS direction.");
}
