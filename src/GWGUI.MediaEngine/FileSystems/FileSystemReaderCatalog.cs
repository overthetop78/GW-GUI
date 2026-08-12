namespace GWGUI.MediaEngine.FileSystems;

/// <summary>Construit le catalogue ordonné des lecteurs de systèmes de fichiers fournis par le moteur.</summary>
public static class FileSystemReaderCatalog
{
    /// <summary>Crée une nouvelle collection contenant les lecteurs par défaut dans leur ordre de détection.</summary>
    public static IReadOnlyList<IFileSystemReader> CreateDefault() => Array.AsReadOnly<IFileSystemReader>(
    [
        new Amiga.AmigaDosFileSystemReader(),
        new Acorn.Adfs.AcornAdfsFileSystemReader(),
        new Readers.BbcDfsFileSystemReader(),
        new Readers.CoherentFileSystemReader(),
        new Readers.Rt11FileSystemReader(),
        new Readers.UcsdFileSystemReader(),
        new Readers.AppleInformXzipFileSystemReader(),
        new Apple.Dos.AppleDosFileSystemReader(),
        new Readers.ProDosFileSystemReader(),
        new Readers.MacMfsFileSystemReader(),
        new Readers.MacHfsFileSystemReader(),
        new Readers.LisaFileSystemReader(),
        new Cpm.AmstradCpmFileSystemReader(),
        new Cpm.CpmFileSystemReader(),
        new Readers.CommodoreDosFileSystemReader(),
        new Readers.Fat12FileSystemReader(),
        new Readers.AtariDosFileSystemReader()
    ]);
}
