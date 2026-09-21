namespace GWGUI.MediaFileSystems.Exploration;

/// <summary>Construit le catalogue ordonné des lecteurs de systèmes de fichiers.</summary>
public static class FileSystemReaderCatalog
{
    /// <summary>Crée une nouvelle collection contenant les lecteurs par défaut dans leur ordre de détection.</summary>
    public static IReadOnlyList<IFileSystemReader> CreateDefault() => Array.AsReadOnly<IFileSystemReader>(
    [
        new GWGUI.MediaFileSystems.FileSystems.Amiga.AmigaDosFileSystemReader(),
        new GWGUI.MediaFileSystems.FileSystems.Amiga.FlatArchive.AmigaFlatResourceArchiveReader(),
        new GWGUI.MediaFileSystems.FileSystems.Acorn.Adfs.AcornAdfsFileSystemReader(),
        new GWGUI.MediaFileSystems.FileSystems.Acorn.BbcDfs.BbcDfsFileSystemReader(),
        new GWGUI.MediaFileSystems.FileSystems.Coherent.CoherentFileSystemReader(),
        new GWGUI.MediaFileSystems.FileSystems.Dec.Rt11.Rt11FileSystemReader(),
        new GWGUI.MediaFileSystems.FileSystems.Ucsd.UcsdFileSystemReader(),
        new GWGUI.MediaFileSystems.FileSystems.Apple.Dos.AppleDosFileSystemReader(),
        new GWGUI.MediaFileSystems.FileSystems.Apple.ProDos.ProDosFileSystemReader(),
        new GWGUI.MediaFileSystems.FileSystems.Apple.Macintosh.Mfs.MacMfsFileSystemReader(),
        new GWGUI.MediaFileSystems.FileSystems.Apple.Macintosh.Hfs.MacHfsFileSystemReader(),
        new GWGUI.MediaFileSystems.FileSystems.Apple.Lisa.LisaFileSystemReader(),
        new GWGUI.MediaFileSystems.FileSystems.Cpm.AmstradCpmFileSystemReader(),
        new GWGUI.MediaFileSystems.FileSystems.Cpm.CpmFileSystemReader(),
        new GWGUI.MediaFileSystems.FileSystems.Commodore.Dos.CommodoreDosFileSystemReader(),
        new GWGUI.MediaFileSystems.FileSystems.Fat12.Fat12FileSystemReader(),
        new GWGUI.MediaFileSystems.FileSystems.Atari.Dos.AtariDosFileSystemReader(),
        new GWGUI.MediaFileSystems.FileSystems.Atari.KFile.AtariKFileFileSystemReader(),
        new GWGUI.MediaFileSystems.FileSystems.Atari.ClkGraphicsLibrary.AtariClkGraphicsLibraryFileSystemReader(),
        new GWGUI.MediaFileSystems.FileSystems.Atari.BootDisk.AtariBootDiskFileSystemReader()
    ]);
}

