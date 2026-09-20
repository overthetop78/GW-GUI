namespace GWGUI.MediaEngine.FileSystems;

/// <summary>Construit le catalogue ordonné des lecteurs de systèmes de fichiers fournis par le moteur.</summary>
public static class FileSystemReaderCatalog
{
    /// <summary>Crée une nouvelle collection contenant les lecteurs par défaut dans leur ordre de détection.</summary>
    public static IReadOnlyList<IFileSystemReader> CreateDefault() => Array.AsReadOnly<IFileSystemReader>(
    [
        new MediaFileSystemsReaderAdapter(new GWGUI.MediaFileSystems.FileSystems.Amiga.AmigaDosFileSystemReader()),
        new MediaFileSystemsReaderAdapter(new GWGUI.MediaFileSystems.FileSystems.Amiga.FlatArchive.AmigaFlatResourceArchiveReader()),
        new MediaFileSystemsReaderAdapter(new GWGUI.MediaFileSystems.FileSystems.Acorn.Adfs.AcornAdfsFileSystemReader()),
        new MediaFileSystemsReaderAdapter(new GWGUI.MediaFileSystems.FileSystems.Acorn.BbcDfs.BbcDfsFileSystemReader()),
        new MediaFileSystemsReaderAdapter(new GWGUI.MediaFileSystems.FileSystems.Coherent.CoherentFileSystemReader()),
        new MediaFileSystemsReaderAdapter(new GWGUI.MediaFileSystems.FileSystems.Dec.Rt11.Rt11FileSystemReader()),
        new MediaFileSystemsReaderAdapter(new GWGUI.MediaFileSystems.FileSystems.Ucsd.UcsdFileSystemReader()),
        new MediaFileSystemsReaderAdapter(new GWGUI.MediaFileSystems.FileSystems.Apple.Dos.AppleDosFileSystemReader()),
        new MediaFileSystemsReaderAdapter(new GWGUI.MediaFileSystems.FileSystems.Apple.ProDos.ProDosFileSystemReader()),
        new MediaFileSystemsReaderAdapter(new GWGUI.MediaFileSystems.FileSystems.Apple.Macintosh.Mfs.MacMfsFileSystemReader()),
        new MediaFileSystemsReaderAdapter(new GWGUI.MediaFileSystems.FileSystems.Apple.Macintosh.Hfs.MacHfsFileSystemReader()),
        new MediaFileSystemsReaderAdapter(new GWGUI.MediaFileSystems.FileSystems.Apple.Lisa.LisaFileSystemReader()),
        new MediaFileSystemsReaderAdapter(new GWGUI.MediaFileSystems.FileSystems.Cpm.AmstradCpmFileSystemReader()),
        new MediaFileSystemsReaderAdapter(new GWGUI.MediaFileSystems.FileSystems.Cpm.CpmFileSystemReader()),
        new MediaFileSystemsReaderAdapter(new GWGUI.MediaFileSystems.FileSystems.Commodore.Dos.CommodoreDosFileSystemReader()),
        new MediaFileSystemsReaderAdapter(new GWGUI.MediaFileSystems.FileSystems.Fat12.Fat12FileSystemReader()),
        new MediaFileSystemsReaderAdapter(new GWGUI.MediaFileSystems.FileSystems.Atari.Dos.AtariDosFileSystemReader()),
        new MediaFileSystemsReaderAdapter(new GWGUI.MediaFileSystems.FileSystems.Atari.ClkGraphicsLibrary.AtariClkGraphicsLibraryFileSystemReader())
    ]);
}
