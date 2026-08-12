namespace GWGUI.MediaEngine.FileSystems;

/// <summary>Construit le catalogue ordonné des lecteurs de systèmes de fichiers fournis par le moteur.</summary>
public static class FileSystemReaderCatalog
{
    /// <summary>Crée une nouvelle collection contenant les lecteurs par défaut dans leur ordre de détection.</summary>
    public static IReadOnlyList<IFileSystemReader> CreateDefault() => Array.AsReadOnly<IFileSystemReader>(
    [
        new Amiga.AmigaDosFileSystemReader(),
        new Acorn.Adfs.AcornAdfsFileSystemReader(),
        new Acorn.BbcDfs.BbcDfsFileSystemReader(),
        new Coherent.CoherentFileSystemReader(),
        new Readers.Rt11FileSystemReader(),
        new Readers.UcsdFileSystemReader(),
        new Apple.InformXzip.AppleInformXzipFileSystemReader(),
        new Apple.Dos.AppleDosFileSystemReader(),
        new Apple.ProDos.ProDosFileSystemReader(),
        new Apple.Macintosh.Mfs.MacMfsFileSystemReader(),
        new Apple.Macintosh.Hfs.MacHfsFileSystemReader(),
        new Apple.Lisa.LisaFileSystemReader(),
        new Cpm.AmstradCpmFileSystemReader(),
        new Cpm.CpmFileSystemReader(),
        new Commodore.Dos.CommodoreDosFileSystemReader(),
        new Fat12.Fat12FileSystemReader(),
        new Atari.Dos.AtariDosFileSystemReader()
    ]);
}
