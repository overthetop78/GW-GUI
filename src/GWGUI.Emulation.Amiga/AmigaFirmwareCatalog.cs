using System.Security.Cryptography;

namespace GWGUI.Emulation.Amiga;

public sealed class AmigaFirmwareCatalog
{
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase) { ".rom", ".bin", ".key" };
    private static readonly HashSet<string> EmuTosVampireV2 = new(StringComparer.OrdinalIgnoreCase)
    {
        "4bb3954ca7dc470a42f53741fb2b94cf", "ebe0a06715b896d12a4409f122b24989",
        "04faeed31162a67de0d173e4787ba493", "ba5d8b68d7c80ff9eb838b868ecf687a",
        "05ab598dd594401ac99159e88202cf3c", "12a73c156cfb5a7f99c72c2d624f2201",
        "e56f618a7f243cd475a364fa26c71ce4", "1d20c6429f372b4e963f541a3278c130",
        "0a2d51bd6c91040abd22ae5514cd18be", "ef120b4cf52f22e63d4417dbc85f5beb",
        "74ea08259bc8261438eca76ddb5556c5", "596991afc6052350679247a3da03d4e3"
    };
    private static readonly HashSet<string> EmuTosVampireV4 = new(StringComparer.OrdinalIgnoreCase)
    {
        "c0efa914fcdf9b7cf1480780bf482bb6", "91e22ba8399da0e69ec3a62ac3cc1ec1",
        "41026d228e4aa6b7b9427f073ab3c4e4", "83fb05605f89c51a86ecc43aa3ac1afb",
        "6286201af55513de3a061f92548803df", "43469eb034f1c1a228d3a6ba3ba4adcd",
        "e3ea0ac84e9c9ab52a1fe04a7aa25329", "60989b1efe5e17f9728b962ffed5a17c",
        "63b0b870b1a7ca5479372459ca15ec9a"
    };
    private static readonly IReadOnlyDictionary<string, (string Version, AmigaFirmwareType Type, string[] Models)> Known =
        new Dictionary<string, (string, AmigaFirmwareType, string[])>(StringComparer.OrdinalIgnoreCase)
        {
            ["d8dbff05f1d39d5b687a5dacb76f8535"] = ("1.0 rev 30", AmigaFirmwareType.Kickstart, ["A1000"]),
            ["0b8442c311caa54fb12ec88eaaa9facf"] = ("1.1 rev 31.034 NTSC", AmigaFirmwareType.Kickstart, ["A1000"]),
            ["1fa1f93d3d7b51271dd1356b8b2b45a9"] = ("1.1 rev 32.034 PAL", AmigaFirmwareType.Kickstart, ["A1000"]),
            ["68c9c0826f6c0ca20546d588ee77391c"] = ("1.2 rev 33.166", AmigaFirmwareType.Kickstart, ["A1000"]),
            ["85ad74194e87c08904327de1a9443b7a"] = ("1.2 rev 33.180", AmigaFirmwareType.Kickstart, ["A1000", "A500", "A2000"]),
            ["82a21c1890cae844b3df741f2762d48d"] = ("1.3 rev 34.005", AmigaFirmwareType.Kickstart, ["A1000", "A500", "A2000", "CDTV"]),
            ["0cbbaacbabcb4b5806438039acc0ff02"] = ("1.3 rev 34.005 A3000 SuperKickstart", AmigaFirmwareType.Kickstart, ["A3000"]),
            ["9596b08c42677b0f01c9547c0da85d22"] = ("2.0 rev 36.143 A3000 SuperKickstart", AmigaFirmwareType.Kickstart, ["A3000"]),
            ["93e6657e563ddaf815fb40d93f860877"] = ("2.02 rev 36.207 A3000 SuperKickstart", AmigaFirmwareType.Kickstart, ["A3000"]),
            ["dc10d7bdd1b6f450773dfb558477c230"] = ("2.04 rev 37.175", AmigaFirmwareType.Kickstart, ["A500PLUS"]),
            ["c5fd2322c53d25c0972e6fc54b705d17"] = ("2.04 rev 37.175 A3000", AmigaFirmwareType.Kickstart, ["A3000"]),
            ["72ffce8541f100885da4b68a3bcf10f7"] = ("2.05 rev 37.299", AmigaFirmwareType.Kickstart, ["A600"]),
            ["fa4acc75b49e880679fe02716af24d71"] = ("2.05 rev 37.300", AmigaFirmwareType.Kickstart, ["A600"]),
            ["465646c9b6729f77eea5314d1f057951"] = ("2.05 rev 37.350", AmigaFirmwareType.Kickstart, ["A600"]),
            ["e40a5dfb3d017ba8779faba30cbd1c8e"] = ("3.1 rev 40.063", AmigaFirmwareType.Kickstart, ["A500", "A600", "A2000"]),
            ["b7cc148386aa631136f510cd29e42fc3"] = ("3.0 rev 39.106", AmigaFirmwareType.Kickstart, ["A1200OG", "A1200"]),
            ["646773759326fbac3b2311fd8c8793ee"] = ("3.1 rev 40.068", AmigaFirmwareType.Kickstart, ["A1200OG", "A1200"]),
            ["9b8bdd5a3fd32c2a5a6f5b1aefc799a5"] = ("3.0 rev 39.106", AmigaFirmwareType.Kickstart, ["A4000"]),
            ["413590e50098a056cfec418d3df0212d"] = ("3.1 rev 40.068", AmigaFirmwareType.Kickstart, ["A3000"]),
            ["9bdedde6a4f33555b4a270c8ca53297d"] = ("3.1 rev 40.068", AmigaFirmwareType.Kickstart, ["A4000"]),
            ["e873c43040b4d7a9c65f37cf2da2158f"] = ("3.1 rev 40.070", AmigaFirmwareType.Kickstart, ["A4000"]),
            ["89da1838a24460e4b93f4f0c5d92d48d"] = ("CDTV extended 1.0", AmigaFirmwareType.ExtendedRom, ["CDTV"]),
            ["d98112f18792ee3714df16a6eb421b89"] = ("CDTV/A570 extended 2.30", AmigaFirmwareType.ExtendedRom, ["CDTV"]),
            ["d1145ab3a0f89340f94c9e734762c198"] = ("CDTV extended 2.7", AmigaFirmwareType.ExtendedRom, ["CDTV"]),
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

    public static AmigaFirmware Inspect(string path) => CreateEntry(Path.GetFullPath(path));

    private static AmigaFirmware CreateEntry(string path)
    {
        var file = new FileInfo(path);
        using var stream = file.OpenRead();
        var md5 = Convert.ToHexString(MD5.HashData(stream));
        stream.Position = 0;
        var sha256 = Convert.ToHexString(SHA256.HashData(stream));
        stream.Position = 0;
        var detected = TryReadKickstartVersion(stream, file.Length);
        var known = TryIdentifyKnown(file, md5, out var identity);
        var alternative = known ? null : TryReadAlternativeSystem(file, md5);
        var type = Path.GetExtension(path).Equals(".key", StringComparison.OrdinalIgnoreCase) ? AmigaFirmwareType.RomKey
            : known ? identity.Type
            : alternative is not null ? AmigaFirmwareType.Kickstart
            : Path.GetFileName(path).Contains("ext", StringComparison.OrdinalIgnoreCase) ? AmigaFirmwareType.ExtendedRom
            : detected is not null ? AmigaFirmwareType.Kickstart
            : AmigaFirmwareType.Unknown;
        var name = known ? KnownName(identity.Type, identity.Version)
            : alternative?.Name
            ?? (type == AmigaFirmwareType.Kickstart ? "Kickstart" : null);
        return new AmigaFirmware(file.FullName, file.Length, md5, sha256, file.LastWriteTimeUtc,
            type, known || alternative is not null, known, name,
            known ? identity.Version : alternative?.Version ?? detected?.Version,
            known ? identity.Models : alternative?.Models ?? detected?.Models ?? []);
    }

    private static (string Name, string Version, string[] Models)? TryReadAlternativeSystem(FileInfo file, string md5)
    {
        if (file.Length is <= 0 or > 1_048_576) return null;
        var text = System.Text.Encoding.ASCII.GetString(File.ReadAllBytes(file.FullName));
        foreach (var product in new[] { "EmuTOS", "Serena" })
        {
            if (!text.Contains(product, StringComparison.OrdinalIgnoreCase)) continue;
            var match = System.Text.RegularExpressions.Regex.Matches(text,
                    @"(?<![0-9])(?<version>[0-9]+\.[0-9]+(?:\.[0-9]+){0,2})(?![0-9])")
                .Cast<System.Text.RegularExpressions.Match>()
                .FirstOrDefault(candidate => candidate.Groups["version"].Value.Split('.')
                    .Any(component => component != "0"));
            var version = match is not null ? $"{product} {match.Groups["version"].Value}" : product;
            if (product == "EmuTOS")
            {
                if (EmuTosVampireV2.Contains(md5)) version += " (Vampire V2)";
                else if (EmuTosVampireV4.Contains(md5)) version += " (Vampire V4)";
                return (product, version, AmigaModelCatalog.All.Select(model => model.Id).ToArray());
            }
            return (product, version, []);
        }
        return null;
    }

    private static bool TryIdentifyKnown(FileInfo file, string md5,
        out (string Version, AmigaFirmwareType Type, string[] Models) identity)
    {
        if (Known.TryGetValue(md5, out identity)) return true;
        if (file.Length != 524_288) return false;

        var bytes = File.ReadAllBytes(file.FullName);
        var first = bytes.AsSpan(0, 262_144);
        if (!first.SequenceEqual(bytes.AsSpan(262_144, 262_144))) return false;
        var canonicalMd5 = Convert.ToHexString(MD5.HashData(first)).ToLowerInvariant();
        return Known.TryGetValue(canonicalMd5, out identity);
    }

    private static string KnownName(AmigaFirmwareType type, string version)
    {
        if (type == AmigaFirmwareType.Kickstart) return "Kickstart";
        if (version.StartsWith("CD32", StringComparison.OrdinalIgnoreCase)) return "CD32";
        if (version.StartsWith("CDTV/A570", StringComparison.OrdinalIgnoreCase)) return "CDTV/A570";
        if (version.StartsWith("CDTV", StringComparison.OrdinalIgnoreCase)) return "CDTV";
        return "ROM";
    }

    private static (string Version, string[] Models)? TryReadKickstartVersion(Stream stream, long length)
    {
        if (length is not (262_144 or 524_288 or 1_048_576)) return null;
        Span<byte> header = stackalloc byte[16];
        if (stream.Read(header) != header.Length) return null;
        var version = (header[12] << 8) | header[13];
        var revision = (header[14] << 8) | header[15];
        if (version is < 29 or > 50 || revision > 1000) return null;
        var models = version switch
        {
            <= 32 => new[] { "A1000" },
            <= 34 => new[] { "A1000", "A500", "A2000" },
            36 => new[] { "A3000" },
            37 => new[] { "A500PLUS", "A600", "A3000" },
            39 => new[] { "A1200", "A4000" },
            40 when revision == 60 => new[] { "CD32" },
            40 when revision == 63 => new[] { "A500", "A600", "A2000" },
            40 when revision == 68 => new[] { "A1200", "A3000", "A4000" },
            40 when revision == 70 => new[] { "A4000" },
            >= 40 => new[] { "A500", "A600", "A1200", "A2000", "A3000", "A4000" },
            _ => []
        };
        return ($"{MarketingVersion(version, revision)} rev {version}.{revision:D3}", models);
    }

    private static string MarketingVersion(int version, int revision) => (version, revision) switch
    {
        (31 or 32, _) => "1.1",
        (33, _) => "1.2",
        (34, _) => "1.3",
        (36, <= 199) => "2.0",
        (36, _) => "2.02",
        (37, <= 299) => "2.04",
        (37, _) => "2.05",
        (39, _) => "3.0",
        (40, _) => "3.1",
        _ => $"{version}.{revision:D3}"
    };
}
