using System.IO;
using GWGUI.Scp.FileSystems;
using GWGUI.Scp.Images;

namespace GWGUI.App.Controls;

public enum ExplorerFileSystemFamily
{
    Unknown,
    Amiga,
    IbmPc,
    AtariSt,
    Atari8Bit,
    AppleDos,
    ProDos,
    Macintosh,
    Lisa,
    Commodore,
    Cpm,
    BbcMicro,
    Dec,
    Msx,
    Ucsd
}

public static class ExplorerFileIconClassifier
{
    private sealed record Profile(
        IReadOnlySet<string> Text,
        IReadOnlySet<string> Images,
        IReadOnlySet<string> Audio,
        IReadOnlySet<string> Archives,
        IReadOnlySet<string> Programs,
        IReadOnlySet<string> DiskImages);

    private static readonly Profile Empty = Make();
    private static readonly IReadOnlyDictionary<ExplorerFileSystemFamily, Profile> Profiles =
        new Dictionary<ExplorerFileSystemFamily, Profile>
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
                images: [".mic", ".pic"],
                audio: [".sap"],
                archives: [".arc"],
                programs: [".com", ".xex", ".exe"],
                disks: [".atr", ".xfd", ".scp"]),
            [ExplorerFileSystemFamily.AppleDos] = Make(
                text: [".txt"], images: [".pic"], audio: [], archives: [], programs: [],
                disks: [".d13", ".dsk", ".do", ".po", ".2mg", ".nib", ".woz", ".scp"]),
            [ExplorerFileSystemFamily.ProDos] = Make(
                text: [".txt"], images: [".pic"], audio: [], archives: [".shk"], programs: [".sys"],
                disks: [".dsk", ".po", ".2mg", ".nib", ".woz", ".scp"]),
            [ExplorerFileSystemFamily.Macintosh] = Make(
                text: [".txt"], images: [".pict", ".pct"], audio: [".snd", ".aiff", ".aif"],
                archives: [".sit", ".cpt", ".hqx", ".bin"], programs: [".app"],
                disks: [".image", ".dc42", ".dsk", ".img", ".scp"]),
            [ExplorerFileSystemFamily.Lisa] = Make(
                text: [".txt"], images: [], audio: [], archives: [".sit"], programs: [],
                disks: [".image", ".dc42", ".img", ".scp"]),
            [ExplorerFileSystemFamily.Commodore] = Make(
                text: [".txt", ".seq"], images: [".koa", ".art", ".iff"], audio: [".sid", ".mus"],
                archives: [".arc", ".sda"], programs: [".prg"], disks: [".d64", ".d71", ".d81", ".g64", ".scp"]),
            [ExplorerFileSystemFamily.Cpm] = Make(
                text: [".txt", ".doc", ".asm", ".mac", ".lib", ".bas", ".for", ".c", ".h"],
                images: [], audio: [], archives: [".arc", ".lbr"], programs: [".com", ".cmd", ".sub"],
                disks: [".dsk", ".edsk", ".scp"]),
            [ExplorerFileSystemFamily.BbcMicro] = Make(
                text: [".txt"], images: [], audio: [], archives: [".zip"], programs: [],
                disks: [".ssd", ".dsd", ".adl", ".adm", ".adf", ".scp"]),
            [ExplorerFileSystemFamily.Dec] = Make(
                text: [".txt", ".mac", ".for", ".bas", ".cmd", ".com"], images: [], audio: [], archives: [".bup"],
                programs: [".sav", ".lda", ".rel", ".obj", ".sys"], disks: [".img", ".dsk", ".scp"]),
            [ExplorerFileSystemFamily.Msx] = Make(
                text: [".txt", ".doc", ".bas", ".asc"], images: [".sc2", ".sc5", ".sc7", ".sc8"],
                audio: [".mgs", ".bgm"], archives: [".lzh", ".pma"], programs: [".com", ".bas", ".rom"],
                disks: [".dsk", ".scp"]),
            [ExplorerFileSystemFamily.Ucsd] = Make(
                text: [".text"], images: [".foto", ".graf"], audio: [], archives: [], programs: [".code"],
                disks: [".td0", ".img", ".dsk", ".scp"])
        };

    public static ExplorerFileSystemFamily FamilyFor(ExploredDiskImage document)
    {
        var format = document.Image.FormatId;
        var fileSystem = document.Volume.FileSystem;
        if (fileSystem.Contains("CP/M", StringComparison.OrdinalIgnoreCase)) return ExplorerFileSystemFamily.Cpm;
        if (format.StartsWith("acorn.dfs", StringComparison.OrdinalIgnoreCase)) return ExplorerFileSystemFamily.BbcMicro;
        if (format.StartsWith("dec.", StringComparison.OrdinalIgnoreCase)) return ExplorerFileSystemFamily.Dec;
        if (format.StartsWith("msx.", StringComparison.OrdinalIgnoreCase)) return ExplorerFileSystemFamily.Msx;
        if (format.StartsWith("ucsd.", StringComparison.OrdinalIgnoreCase)) return ExplorerFileSystemFamily.Ucsd;
        if (format.StartsWith("amiga.", StringComparison.OrdinalIgnoreCase)) return ExplorerFileSystemFamily.Amiga;
        if (format.StartsWith("ibm.", StringComparison.OrdinalIgnoreCase)) return ExplorerFileSystemFamily.IbmPc;
        if (format.StartsWith("atarist.", StringComparison.OrdinalIgnoreCase)) return ExplorerFileSystemFamily.AtariSt;
        if (format.StartsWith("atari.", StringComparison.OrdinalIgnoreCase)) return ExplorerFileSystemFamily.Atari8Bit;
        if (format.StartsWith("apple2.dos", StringComparison.OrdinalIgnoreCase)) return ExplorerFileSystemFamily.AppleDos;
        if (format.StartsWith("apple2.appledos", StringComparison.OrdinalIgnoreCase)) return ExplorerFileSystemFamily.AppleDos;
        if (format.StartsWith("apple2.prodos", StringComparison.OrdinalIgnoreCase) || format.StartsWith("apple3.", StringComparison.OrdinalIgnoreCase)) return ExplorerFileSystemFamily.ProDos;
        if (format.StartsWith("applemac", StringComparison.OrdinalIgnoreCase) || format.StartsWith("mac.", StringComparison.OrdinalIgnoreCase)) return ExplorerFileSystemFamily.Macintosh;
        if (format.StartsWith("applelisa", StringComparison.OrdinalIgnoreCase)) return ExplorerFileSystemFamily.Lisa;
        if (format.StartsWith("commodore.", StringComparison.OrdinalIgnoreCase)) return ExplorerFileSystemFamily.Commodore;
        return ExplorerFileSystemFamily.Unknown;
    }

    public static ExplorerIconKind IconFor(FileSystemEntry entry, ExplorerFileSystemFamily family = ExplorerFileSystemFamily.Unknown)
    {
        if (entry.Kind == FileSystemEntryKind.Directory) return ExplorerIconKind.Folder;
        if (entry.Kind == FileSystemEntryKind.Link) return ExplorerIconKind.Link;
        var extension = Path.GetExtension(entry.Name);
        var profile = Profiles.TryGetValue(family, out var selected) ? selected : Empty;

        var metadata = MetadataIcon(entry, family);
        if (metadata is not null) return metadata.Value;
        if (IsAmigaExecutable(entry.Content) && family == ExplorerFileSystemFamily.Amiga) return ExplorerIconKind.Program;
        if (IsDosExecutable(entry.Content) && family == ExplorerFileSystemFamily.IbmPc) return ExplorerIconKind.Program;
        if (IsAtariExecutable(entry.Content) && family == ExplorerFileSystemFamily.AtariSt) return ExplorerIconKind.Program;
        if (HasFormType(entry.Content, "ILBM")) return ExplorerIconKind.Image;
        if (HasFormType(entry.Content, "8SVX")) return ExplorerIconKind.Audio;
        if (profile.Programs.Contains(extension)) return ExplorerIconKind.Program;
        if (profile.Images.Contains(extension)) return ExplorerIconKind.Image;
        if (profile.Audio.Contains(extension)) return ExplorerIconKind.Audio;
        if (profile.Archives.Contains(extension)) return ExplorerIconKind.Archive;
        if (profile.DiskImages.Contains(extension)) return ExplorerIconKind.DiskImage;
        if (profile.Text.Contains(extension) || LooksLikeText(entry.Content)) return ExplorerIconKind.Text;
        return ExplorerIconKind.File;
    }

    public static string TypeResourceKeyFor(ExplorerIconKind kind) => kind switch
    {
        ExplorerIconKind.Folder => "Explorer.Directory",
        ExplorerIconKind.Text => "Explorer.Type.Text",
        ExplorerIconKind.Image => "Explorer.Type.Image",
        ExplorerIconKind.Audio => "Explorer.Type.Audio",
        ExplorerIconKind.Archive => "Explorer.Type.Archive",
        ExplorerIconKind.Program => "Explorer.Type.Program",
        ExplorerIconKind.DiskImage => "Explorer.Type.DiskImage",
        ExplorerIconKind.Link => "Explorer.Link",
        _ => "Explorer.File"
    };

    private static ExplorerIconKind? MetadataIcon(FileSystemEntry entry, ExplorerFileSystemFamily family)
    {
        var type = entry.Comment.Trim();
        if (family == ExplorerFileSystemFamily.Commodore && type.StartsWith("PRG", StringComparison.OrdinalIgnoreCase)) return ExplorerIconKind.Program;
        if (family == ExplorerFileSystemFamily.AppleDos && type is "Text") return ExplorerIconKind.Text;
        if (family == ExplorerFileSystemFamily.AppleDos && type is "Integer BASIC" or "Applesoft BASIC") return ExplorerIconKind.Program;
        if (family == ExplorerFileSystemFamily.ProDos && type is "Text") return ExplorerIconKind.Text;
        if (family == ExplorerFileSystemFamily.ProDos && type is "BASIC" or "System") return ExplorerIconKind.Program;
        if (family == ExplorerFileSystemFamily.Macintosh)
        {
            if (type.Equals("APPL", StringComparison.OrdinalIgnoreCase)) return ExplorerIconKind.Program;
            if (type.Equals("TEXT", StringComparison.OrdinalIgnoreCase)) return ExplorerIconKind.Text;
            if (type.Equals("PICT", StringComparison.OrdinalIgnoreCase)) return ExplorerIconKind.Image;
            if (type.Equals("snd", StringComparison.OrdinalIgnoreCase) || type.Equals("AIFF", StringComparison.OrdinalIgnoreCase)) return ExplorerIconKind.Audio;
        }
        if (family == ExplorerFileSystemFamily.Ucsd)
        {
            if (type.Equals("UCSD code file", StringComparison.OrdinalIgnoreCase)) return ExplorerIconKind.Program;
            if (type.Equals("UCSD text file", StringComparison.OrdinalIgnoreCase)) return ExplorerIconKind.Text;
            if (type is "UCSD graphics file" or "UCSD photo file") return ExplorerIconKind.Image;
        }
        return null;
    }

    private static Profile Make(string[]? text = null, string[]? images = null, string[]? audio = null,
        string[]? archives = null, string[]? programs = null, string[]? disks = null) => new(
        Set(text), Set(images), Set(audio), Set(archives), Set(programs), Set(disks));

    private static IReadOnlySet<string> Set(IEnumerable<string>? values) => new HashSet<string>(values ?? [], StringComparer.OrdinalIgnoreCase);
    private static bool IsAmigaExecutable(IReadOnlyList<byte>? data) => data is { Count: >= 4 } && data[0] == 0 && data[1] == 0 && data[2] == 3 && data[3] == 0xF3;
    private static bool IsDosExecutable(IReadOnlyList<byte>? data) => data is { Count: >= 2 } && data[0] == (byte)'M' && data[1] == (byte)'Z';
    private static bool IsAtariExecutable(IReadOnlyList<byte>? data) => data is { Count: >= 2 } && data[0] == 0x60 && data[1] == 0x1A;
    private static bool HasFormType(IReadOnlyList<byte>? data, string type) => data is { Count: >= 12 } &&
        data[0] == (byte)'F' && data[1] == (byte)'O' && data[2] == (byte)'R' && data[3] == (byte)'M' &&
        data.Skip(8).Take(4).SequenceEqual(System.Text.Encoding.ASCII.GetBytes(type));

    private static bool LooksLikeText(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: > 0 }) return false;
        var sample = data.Take(Math.Min(data.Count, 512)).ToArray();
        var printable = sample.Count(value => value is 9 or 10 or 13 || value >= 32 && value < 127);
        return printable >= sample.Length * 0.9;
    }
}
