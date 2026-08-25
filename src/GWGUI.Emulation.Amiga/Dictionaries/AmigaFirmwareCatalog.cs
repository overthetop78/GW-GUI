using System.Security.Cryptography;

namespace GWGUI.Emulation.Amiga.Dictionaries;

public sealed class AmigaFirmwareCatalog
{
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase) { AmigaFirmwareCatalogConstants.Rom, AmigaFirmwareCatalogConstants.Bin, AmigaFirmwareCatalogConstants.Key };
    private static readonly HashSet<string> EmuTosVampireV2 = new(StringComparer.OrdinalIgnoreCase)
    {
        AmigaFirmwareCatalogConstants.Hash4BB3954CA7DC, AmigaFirmwareCatalogConstants.HashEBE0A06715B8,
        AmigaFirmwareCatalogConstants.Hash04FAEED31162, AmigaFirmwareCatalogConstants.HashBA5D8B68D7C8,
        AmigaFirmwareCatalogConstants.Hash05AB598DD594, AmigaFirmwareCatalogConstants.Hash12A73C156CFB,
        AmigaFirmwareCatalogConstants.HashE56F618A7F24, AmigaFirmwareCatalogConstants.Hash1D20C6429F37,
        AmigaFirmwareCatalogConstants.Hash0A2D51BD6C91, AmigaFirmwareCatalogConstants.HashEF120B4CF52F,
        AmigaFirmwareCatalogConstants.Hash74EA08259BC8, AmigaFirmwareCatalogConstants.Hash596991AFC605
    };
    private static readonly HashSet<string> EmuTosVampireV4 = new(StringComparer.OrdinalIgnoreCase)
    {
        AmigaFirmwareCatalogConstants.HashC0EFA914FCDF, AmigaFirmwareCatalogConstants.Hash91E22BA8399D,
        AmigaFirmwareCatalogConstants.Hash41026D228E4A, AmigaFirmwareCatalogConstants.Hash83FB05605F89,
        AmigaFirmwareCatalogConstants.Hash6286201AF555, AmigaFirmwareCatalogConstants.Hash43469EB034F1,
        AmigaFirmwareCatalogConstants.HashE3EA0AC84E9C, AmigaFirmwareCatalogConstants.Hash60989B1EFE5E,
        AmigaFirmwareCatalogConstants.Hash63B0B870B1A7
    };
    private static readonly IReadOnlyDictionary<string, (string Version, AmigaFirmwareType Type, string[] Models)> Known =
        new Dictionary<string, (string, AmigaFirmwareType, string[])>(StringComparer.OrdinalIgnoreCase)
        {
            [AmigaFirmwareCatalogConstants.HashD8DBFF05F1D3] = (AmigaFirmwareCatalogConstants.Value10Rev30, AmigaFirmwareType.Kickstart, [AmigaFirmwareCatalogConstants.A1000]),
            [AmigaFirmwareCatalogConstants.Hash0B8442C311CA] = (AmigaFirmwareCatalogConstants.Value11Rev31034NTSC, AmigaFirmwareType.Kickstart, [AmigaFirmwareCatalogConstants.A1000]),
            [AmigaFirmwareCatalogConstants.Hash1FA1F93D3D7B] = (AmigaFirmwareCatalogConstants.Value11Rev32034PAL, AmigaFirmwareType.Kickstart, [AmigaFirmwareCatalogConstants.A1000]),
            [AmigaFirmwareCatalogConstants.Hash68C9C0826F6C] = (AmigaFirmwareCatalogConstants.Value12Rev33166, AmigaFirmwareType.Kickstart, [AmigaFirmwareCatalogConstants.A1000]),
            [AmigaFirmwareCatalogConstants.Hash85AD74194E87] = (AmigaFirmwareCatalogConstants.Value12Rev33180, AmigaFirmwareType.Kickstart, [AmigaFirmwareCatalogConstants.A1000, AmigaFirmwareCatalogConstants.A500, AmigaFirmwareCatalogConstants.A2000]),
            [AmigaFirmwareCatalogConstants.Hash82A21C1890CA] = (AmigaFirmwareCatalogConstants.Value13Rev34005, AmigaFirmwareType.Kickstart, [AmigaFirmwareCatalogConstants.A1000, AmigaFirmwareCatalogConstants.A500, AmigaFirmwareCatalogConstants.A2000, AmigaFirmwareCatalogConstants.CDTV]),
            [AmigaFirmwareCatalogConstants.Hash0CBBAACBABCB] = (AmigaFirmwareCatalogConstants.Value13Rev34005A3000SuperKickstart, AmigaFirmwareType.Kickstart, [AmigaFirmwareCatalogConstants.A3000]),
            [AmigaFirmwareCatalogConstants.Hash9596B08C4267] = (AmigaFirmwareCatalogConstants.Value20Rev36143A3000SuperKickstart, AmigaFirmwareType.Kickstart, [AmigaFirmwareCatalogConstants.A3000]),
            [AmigaFirmwareCatalogConstants.Hash93E6657E563D] = (AmigaFirmwareCatalogConstants.Value202Rev36207A3000SuperKickstart, AmigaFirmwareType.Kickstart, [AmigaFirmwareCatalogConstants.A3000]),
            [AmigaFirmwareCatalogConstants.HashDC10D7BDD1B6] = (AmigaFirmwareCatalogConstants.Value204Rev37175, AmigaFirmwareType.Kickstart, [AmigaFirmwareCatalogConstants.A500PLUS]),
            [AmigaFirmwareCatalogConstants.HashC5FD2322C53D] = (AmigaFirmwareCatalogConstants.Value204Rev37175A3000, AmigaFirmwareType.Kickstart, [AmigaFirmwareCatalogConstants.A3000]),
            [AmigaFirmwareCatalogConstants.Hash72FFCE8541F1] = (AmigaFirmwareCatalogConstants.Value205Rev37299, AmigaFirmwareType.Kickstart, [AmigaFirmwareCatalogConstants.A600]),
            [AmigaFirmwareCatalogConstants.HashFA4ACC75B49E] = (AmigaFirmwareCatalogConstants.Value205Rev37300, AmigaFirmwareType.Kickstart, [AmigaFirmwareCatalogConstants.A600]),
            [AmigaFirmwareCatalogConstants.Hash465646C9B672] = (AmigaFirmwareCatalogConstants.Value205Rev37350, AmigaFirmwareType.Kickstart, [AmigaFirmwareCatalogConstants.A600]),
            [AmigaFirmwareCatalogConstants.HashE40A5DFB3D01] = (AmigaFirmwareCatalogConstants.Value31Rev40063, AmigaFirmwareType.Kickstart, [AmigaFirmwareCatalogConstants.A500, AmigaFirmwareCatalogConstants.A600, AmigaFirmwareCatalogConstants.A2000]),
            [AmigaFirmwareCatalogConstants.HashB7CC148386AA] = (AmigaFirmwareCatalogConstants.Value30Rev39106, AmigaFirmwareType.Kickstart, [AmigaFirmwareCatalogConstants.A1200OG, AmigaFirmwareCatalogConstants.A1200]),
            [AmigaFirmwareCatalogConstants.Hash646773759326] = (AmigaFirmwareCatalogConstants.Value31Rev40068, AmigaFirmwareType.Kickstart, [AmigaFirmwareCatalogConstants.A1200OG, AmigaFirmwareCatalogConstants.A1200]),
            [AmigaFirmwareCatalogConstants.Hash9B8BDD5A3FD3] = (AmigaFirmwareCatalogConstants.Value30Rev39106, AmigaFirmwareType.Kickstart, [AmigaFirmwareCatalogConstants.A4000]),
            [AmigaFirmwareCatalogConstants.Hash413590E50098] = (AmigaFirmwareCatalogConstants.Value31Rev40068, AmigaFirmwareType.Kickstart, [AmigaFirmwareCatalogConstants.A3000]),
            [AmigaFirmwareCatalogConstants.Hash9BDEDDE6A4F3] = (AmigaFirmwareCatalogConstants.Value31Rev40068, AmigaFirmwareType.Kickstart, [AmigaFirmwareCatalogConstants.A4000]),
            [AmigaFirmwareCatalogConstants.HashE873C43040B4] = (AmigaFirmwareCatalogConstants.Value31Rev40070, AmigaFirmwareType.Kickstart, [AmigaFirmwareCatalogConstants.A4000]),
            [AmigaFirmwareCatalogConstants.Hash89DA1838A244] = (AmigaFirmwareCatalogConstants.CDTVExtended10, AmigaFirmwareType.ExtendedRom, [AmigaFirmwareCatalogConstants.CDTV]),
            [AmigaFirmwareCatalogConstants.HashD98112F18792] = (AmigaFirmwareCatalogConstants.CDTVA570Extended230, AmigaFirmwareType.ExtendedRom, [AmigaFirmwareCatalogConstants.CDTV]),
            [AmigaFirmwareCatalogConstants.HashD1145AB3A0F8] = (AmigaFirmwareCatalogConstants.CDTVExtended27, AmigaFirmwareType.ExtendedRom, [AmigaFirmwareCatalogConstants.CDTV]),
            [AmigaFirmwareCatalogConstants.HashF2F241BF0941] = (AmigaFirmwareCatalogConstants.CD32Combined31Rev40060, AmigaFirmwareType.Kickstart, [AmigaFirmwareCatalogConstants.CD32, AmigaFirmwareCatalogConstants.CD32FR]),
            [AmigaFirmwareCatalogConstants.Hash5F8924D013DD] = (AmigaFirmwareCatalogConstants.CD3231Rev40060, AmigaFirmwareType.Kickstart, [AmigaFirmwareCatalogConstants.CD32, AmigaFirmwareCatalogConstants.CD32FR]),
            [AmigaFirmwareCatalogConstants.HashBB72565701B1] = (AmigaFirmwareCatalogConstants.CD32Extended31Rev40060, AmigaFirmwareType.ExtendedRom, [AmigaFirmwareCatalogConstants.CD32, AmigaFirmwareCatalogConstants.CD32FR])
        };
    private readonly string _directory;

    public AmigaFirmwareCatalog(string directory) => _directory = Path.GetFullPath(directory);

    public IReadOnlyList<AmigaFirmware> Scan()
    {
        Directory.CreateDirectory(_directory);
        return Directory.EnumerateFiles(_directory, AmigaFirmwareCatalogConstants.Value, SearchOption.AllDirectories)
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
        var type = Path.GetExtension(path).Equals(AmigaFirmwareCatalogConstants.Key, StringComparison.OrdinalIgnoreCase) ? AmigaFirmwareType.RomKey
            : known ? identity.Type
            : alternative is not null ? AmigaFirmwareType.Kickstart
            : Path.GetFileName(path).Contains(AmigaFirmwareCatalogConstants.Ext, StringComparison.OrdinalIgnoreCase) ? AmigaFirmwareType.ExtendedRom
            : detected is not null ? AmigaFirmwareType.Kickstart
            : AmigaFirmwareType.Unknown;
        var name = known ? KnownName(identity.Type, identity.Version)
            : alternative?.Name
            ?? (type == AmigaFirmwareType.Kickstart ? AmigaFirmwareCatalogConstants.Kickstart : null);
        return new AmigaFirmware(file.FullName, file.Length, md5, sha256, file.LastWriteTimeUtc,
            type, known || alternative is not null, known, name,
            known ? identity.Version : alternative?.Version ?? detected?.Version,
            known ? identity.Models : alternative?.Models ?? detected?.Models ?? []);
    }

    private static (string Name, string Version, string[] Models)? TryReadAlternativeSystem(FileInfo file, string md5)
    {
        if (file.Length is <= 0 or > 1_048_576) return null;
        var text = System.Text.Encoding.ASCII.GetString(File.ReadAllBytes(file.FullName));
        foreach (var product in new[] { AmigaFirmwareCatalogConstants.EmuTOS, AmigaFirmwareCatalogConstants.Serena })
        {
            if (!text.Contains(product, StringComparison.OrdinalIgnoreCase)) continue;
            var match = System.Text.RegularExpressions.Regex.Matches(text,
                    AmigaFirmwareCatalogConstants.Value09Version0909090209)
                .Cast<System.Text.RegularExpressions.Match>()
                .FirstOrDefault(candidate => candidate.Groups[AmigaFirmwareCatalogConstants.Version].Value.Split('.')
                    .Any(component => component != AmigaFirmwareCatalogConstants.Value0));
            var version = match is not null ? $"{product} {match.Groups[AmigaFirmwareCatalogConstants.Version].Value}" : product;
            if (product == AmigaFirmwareCatalogConstants.EmuTOS)
            {
                if (EmuTosVampireV2.Contains(md5)) version += AmigaFirmwareCatalogConstants.VampireV2;
                else if (EmuTosVampireV4.Contains(md5)) version += AmigaFirmwareCatalogConstants.VampireV4;
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
        if (type == AmigaFirmwareType.Kickstart) return AmigaFirmwareCatalogConstants.Kickstart;
        if (version.StartsWith(AmigaFirmwareCatalogConstants.CD32, StringComparison.OrdinalIgnoreCase)) return AmigaFirmwareCatalogConstants.CD32;
        if (version.StartsWith(AmigaFirmwareCatalogConstants.CDTVA570, StringComparison.OrdinalIgnoreCase)) return AmigaFirmwareCatalogConstants.CDTVA570;
        if (version.StartsWith(AmigaFirmwareCatalogConstants.CDTV, StringComparison.OrdinalIgnoreCase)) return AmigaFirmwareCatalogConstants.CDTV;
        return AmigaFirmwareCatalogConstants.ROM;
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
            <= 32 => new[] { AmigaFirmwareCatalogConstants.A1000 },
            <= 34 => new[] { AmigaFirmwareCatalogConstants.A1000, AmigaFirmwareCatalogConstants.A500, AmigaFirmwareCatalogConstants.A2000 },
            36 => new[] { AmigaFirmwareCatalogConstants.A3000 },
            37 => new[] { AmigaFirmwareCatalogConstants.A500PLUS, AmigaFirmwareCatalogConstants.A600, AmigaFirmwareCatalogConstants.A3000 },
            39 => new[] { AmigaFirmwareCatalogConstants.A1200, AmigaFirmwareCatalogConstants.A4000 },
            40 when revision == 60 => new[] { AmigaFirmwareCatalogConstants.CD32 },
            40 when revision == 63 => new[] { AmigaFirmwareCatalogConstants.A500, AmigaFirmwareCatalogConstants.A600, AmigaFirmwareCatalogConstants.A2000 },
            40 when revision == 68 => new[] { AmigaFirmwareCatalogConstants.A1200, AmigaFirmwareCatalogConstants.A3000, AmigaFirmwareCatalogConstants.A4000 },
            40 when revision == 70 => new[] { AmigaFirmwareCatalogConstants.A4000 },
            >= 40 => new[] { AmigaFirmwareCatalogConstants.A500, AmigaFirmwareCatalogConstants.A600, AmigaFirmwareCatalogConstants.A1200, AmigaFirmwareCatalogConstants.A2000, AmigaFirmwareCatalogConstants.A3000, AmigaFirmwareCatalogConstants.A4000 },
            _ => []
        };
        return ($"{MarketingVersion(version, revision)} rev {version}.{revision:D3}", models);
    }

    private static string MarketingVersion(int version, int revision) => (version, revision) switch
    {
        (31 or 32, _) => AmigaFirmwareCatalogConstants.Value11,
        (33, _) => AmigaFirmwareCatalogConstants.Value12,
        (34, _) => AmigaFirmwareCatalogConstants.Value13,
        (36, <= 199) => AmigaFirmwareCatalogConstants.Value20,
        (36, _) => AmigaFirmwareCatalogConstants.Value202,
        (37, <= 299) => AmigaFirmwareCatalogConstants.Value204,
        (37, _) => AmigaFirmwareCatalogConstants.Value205,
        (39, _) => AmigaFirmwareCatalogConstants.Value30,
        (40, _) => AmigaFirmwareCatalogConstants.Value31,
        _ => $"{version}.{revision:D3}"
    };
}
