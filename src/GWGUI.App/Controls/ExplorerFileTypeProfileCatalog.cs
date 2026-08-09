namespace GWGUI.App.Controls;

internal sealed record ExplorerFileTypeProfile(
    IReadOnlySet<string> Text,
    IReadOnlySet<string> Images,
    IReadOnlySet<string> Audio,
    IReadOnlySet<string> Archives,
    IReadOnlySet<string> Programs,
    IReadOnlySet<string> DiskImages);

internal static class ExplorerFileTypeProfileCatalog
{
    private static readonly ExplorerFileTypeProfile Empty = Make();
    private static readonly IReadOnlyDictionary<ExplorerFileSystemFamily, ExplorerFileTypeProfile> Profiles =
        new Dictionary<ExplorerFileSystemFamily, ExplorerFileTypeProfile>
        {
            [ExplorerFileSystemFamily.Amiga] = Make(
                text: [".txt", ".nfo", ".doc", ".guide", ".readme", ".asm", ".s", ".c", ".h", ".bas", ".ini", ".cfg"],
                images: [".info", ".iff", ".ilbm", ".lbm"],
                audio: [".mod", ".med", ".xm", ".8svx"],
                archives: [".lha", ".lzh", ".zip", ".arc", ".zoo", ".dms", ".tar", ".gz"],
                programs: [".library", ".device", ".handler"],
                disks: [".adf", ".scp", ".hfe", ".ipf", ".dms"]),
            [ExplorerFileSystemFamily.IbmPc] = Make(
                text: [".txt", ".nfo", ".doc", ".readme", ".asm", ".c", ".h", ".bas", ".ini", ".cfg", ".xml", ".html"],
                images: [".bmp", ".gif", ".jpg", ".jpeg", ".png", ".pcx"],
                audio: [".wav", ".voc", ".mid", ".midi", ".s3m", ".xm"],
                archives: [".zip", ".arc", ".arj", ".lha", ".lzh", ".zoo", ".tar", ".gz"],
                programs: [".exe", ".com", ".bat", ".cmd"],
                disks: [".ima", ".img", ".scp", ".hfe", ".td0", ".imd", ".86f"]),
            [ExplorerFileSystemFamily.AtariSt] = Make(
                text: [".txt", ".doc", ".asc", ".inf", ".cfg", ".asm", ".s", ".c", ".h", ".bas"],
                images: [".neo", ".pi1", ".pi2", ".pi3", ".pc1", ".pc2", ".pc3", ".deg", ".iff"],
                audio: [".mod", ".snd", ".ym"],
                archives: [".zip", ".arc", ".lzh", ".lha", ".zoo"],
                programs: [".prg", ".ttp", ".tos", ".app", ".gtp", ".acc"],
                disks: [".st", ".msa", ".scp", ".hfe", ".stx", ".ipf"]),
            [ExplorerFileSystemFamily.Atari8Bit] = Make(
                text: [".txt", ".doc", ".lst", ".asm", ".bas"],
                images: [".mic", ".pic"], audio: [".sap"], archives: [".arc"],
                programs: [".com", ".xex", ".exe"], disks: [".atr", ".xfd", ".scp"]),
            [ExplorerFileSystemFamily.AppleDos] = Make(
                text: [".txt"], images: [".pic"], disks: [".d13", ".dsk", ".do", ".po", ".2mg", ".nib", ".woz", ".scp"]),
            [ExplorerFileSystemFamily.ProDos] = Make(
                text: [".txt"], images: [".pic"], archives: [".shk"], programs: [".sys"],
                disks: [".dsk", ".po", ".2mg", ".nib", ".woz", ".scp"]),
            [ExplorerFileSystemFamily.Macintosh] = Make(
                text: [".txt"], images: [".pict", ".pct"], audio: [".snd", ".aiff", ".aif"],
                archives: [".sit", ".cpt", ".hqx", ".bin"], programs: [".app"],
                disks: [".image", ".dc42", ".dsk", ".img", ".scp"]),
            [ExplorerFileSystemFamily.Lisa] = Make(
                text: [".txt"], archives: [".sit"], disks: [".image", ".dc42", ".img", ".scp"]),
            [ExplorerFileSystemFamily.Commodore] = Make(
                text: [".txt", ".seq"], images: [".koa", ".art", ".iff"], audio: [".sid", ".mus"],
                archives: [".arc", ".sda"], programs: [".prg"], disks: [".d64", ".d71", ".d81", ".g64", ".scp"]),
            [ExplorerFileSystemFamily.Cpm] = Make(
                text: [".txt", ".doc", ".asm", ".mac", ".lib", ".bas", ".for", ".c", ".h"],
                archives: [".arc", ".lbr"], programs: [".com", ".cmd", ".sub"], disks: [".dsk", ".edsk", ".scp"]),
            [ExplorerFileSystemFamily.BbcMicro] = Make(
                text: [".txt"], archives: [".zip"], disks: [".ssd", ".dsd", ".adl", ".adm", ".adf", ".scp"]),
            [ExplorerFileSystemFamily.Dec] = Make(
                text: [".txt", ".mac", ".for", ".bas", ".cmd", ".com"], archives: [".bup"],
                programs: [".sav", ".lda", ".rel", ".obj", ".sys"], disks: [".img", ".dsk", ".scp"]),
            [ExplorerFileSystemFamily.Msx] = Make(
                text: [".txt", ".doc", ".bas", ".asc"], images: [".sc2", ".sc5", ".sc7", ".sc8"],
                audio: [".mgs", ".bgm"], archives: [".lzh", ".pma"], programs: [".com", ".bas", ".rom"],
                disks: [".dsk", ".scp"]),
            [ExplorerFileSystemFamily.Ucsd] = Make(
                text: [".text"], images: [".foto", ".graf"], programs: [".code"], disks: [".td0", ".img", ".dsk", ".scp"])
        };

    public static ExplorerFileTypeProfile For(ExplorerFileSystemFamily family) =>
        Profiles.TryGetValue(family, out var profile) ? profile : Empty;

    private static ExplorerFileTypeProfile Make(
        string[]? text = null,
        string[]? images = null,
        string[]? audio = null,
        string[]? archives = null,
        string[]? programs = null,
        string[]? disks = null) =>
        new(Set(text), Set(images), Set(audio), Set(archives), Set(programs), Set(disks));

    private static IReadOnlySet<string> Set(IEnumerable<string>? values) =>
        new HashSet<string>(values ?? [], StringComparer.OrdinalIgnoreCase);
}
