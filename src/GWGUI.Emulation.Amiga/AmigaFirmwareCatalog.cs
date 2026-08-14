using System.Security.Cryptography;

namespace GWGUI.Emulation.Amiga;

public enum AmigaFirmwareType { Kickstart, ExtendedRom, RomKey, Unknown }
public sealed record AmigaFirmware(string Path, long Size, string Md5, string Sha256, DateTime LastWriteTimeUtc,
    AmigaFirmwareType Type, bool IsKnown, string? Version, IReadOnlyList<string> CompatibleModels);

public sealed class AmigaFirmwareCatalog
{
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase) { ".rom", ".bin", ".key" };
    private static readonly IReadOnlyDictionary<string, (string Version, AmigaFirmwareType Type, string[] Models)> Known =
        new Dictionary<string, (string, AmigaFirmwareType, string[])>(StringComparer.OrdinalIgnoreCase)
        {
            ["0b8442c311caa54fb12ec88eaaa9facf"] = ("1.1 rev 31.034 NTSC", AmigaFirmwareType.Kickstart, ["A1000"]),
            ["1fa1f93d3d7b51271dd1356b8b2b45a9"] = ("1.1 rev 32.034 PAL", AmigaFirmwareType.Kickstart, ["A1000"]),
            ["85ad74194e87c08904327de1a9443b7a"] = ("1.2 rev 33.180", AmigaFirmwareType.Kickstart, ["A500OG", "A500", "A2000OG"]),
            ["82a21c1890cae844b3df741f2762d48d"] = ("1.3 rev 34.005", AmigaFirmwareType.Kickstart, ["A500OG", "A500", "A2000OG", "CDTV"]),
            ["dc10d7bdd1b6f450773dfb558477c230"] = ("2.04 rev 37.175", AmigaFirmwareType.Kickstart, ["A500PLUS"]),
            ["465646c9b6729f77eea5314d1f057951"] = ("2.05 rev 37.350", AmigaFirmwareType.Kickstart, ["A600"]),
            ["e40a5dfb3d017ba8779faba30cbd1c8e"] = ("3.1 rev 40.063", AmigaFirmwareType.Kickstart, ["A600", "A2000"]),
            ["b7cc148386aa631136f510cd29e42fc3"] = ("3.0 rev 39.106", AmigaFirmwareType.Kickstart, ["A1200OG", "A1200"]),
            ["646773759326fbac3b2311fd8c8793ee"] = ("3.1 rev 40.068", AmigaFirmwareType.Kickstart, ["A1200OG", "A1200"]),
            ["9b8bdd5a3fd32c2a5a6f5b1aefc799a5"] = ("3.0 rev 39.106", AmigaFirmwareType.Kickstart, ["A4030", "A4040"]),
            ["9bdedde6a4f33555b4a270c8ca53297d"] = ("3.1 rev 40.068", AmigaFirmwareType.Kickstart, ["A4030", "A4040"]),
            ["89da1838a24460e4b93f4f0c5d92d48d"] = ("CDTV extended 1.0", AmigaFirmwareType.ExtendedRom, ["CDTV"]),
            ["f2f241bf094168cfb9e7805dc2856433"] = ("CD32 combined 3.1 rev 40.060", AmigaFirmwareType.Kickstart, ["CD32", "CD32FR"]),
            ["5f8924d013dd57a89cf349f4cdedc6b1"] = ("CD32 3.1 rev 40.060", AmigaFirmwareType.Kickstart, ["CD32", "CD32FR"]),
            ["bb72565701b1b6faece07d68ea5da639"] = ("CD32 extended 3.1 rev 40.060", AmigaFirmwareType.ExtendedRom, ["CD32", "CD32FR"])
        };
    private readonly string _directory;

    public AmigaFirmwareCatalog(string directory) => _directory = Path.GetFullPath(directory);

    public IReadOnlyList<AmigaFirmware> Scan()
    {
        Directory.CreateDirectory(_directory);
        return Directory.EnumerateFiles(_directory, "*", SearchOption.AllDirectories)
            .Where(path => Extensions.Contains(Path.GetExtension(path)))
            .Select(CreateEntry)
            .OrderBy(entry => entry.Path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static AmigaFirmware CreateEntry(string path)
    {
        var file = new FileInfo(path);
        using var stream = file.OpenRead();
        var md5 = Convert.ToHexString(MD5.HashData(stream));
        stream.Position = 0;
        var sha256 = Convert.ToHexString(SHA256.HashData(stream));
        var known = Known.TryGetValue(md5, out var identity);
        var type = Path.GetExtension(path).Equals(".key", StringComparison.OrdinalIgnoreCase) ? AmigaFirmwareType.RomKey
            : known ? identity.Type
            : Path.GetFileName(path).Contains("ext", StringComparison.OrdinalIgnoreCase) ? AmigaFirmwareType.ExtendedRom
            : AmigaFirmwareType.Unknown;
        return new AmigaFirmware(file.FullName, file.Length, md5, sha256, file.LastWriteTimeUtc,
            type, known, known ? identity.Version : null, known ? identity.Models : []);
    }
}
