using System.Security.Cryptography;

namespace GWGUI.Emulation.Amiga.Common.Machines.Common.Dictionaries;

public sealed class FirmwareCatalog
{
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase) { FirmwareCatalogConstants.Rom, FirmwareCatalogConstants.Bin, FirmwareCatalogConstants.Key };
    private static readonly HashSet<string> EmuTosVampireV2 = new(StringComparer.OrdinalIgnoreCase)
    {
        FirmwareCatalogConstants.Hash4BB3954CA7DC, FirmwareCatalogConstants.HashEBE0A06715B8,
        FirmwareCatalogConstants.Hash04FAEED31162, FirmwareCatalogConstants.HashBA5D8B68D7C8,
        FirmwareCatalogConstants.Hash05AB598DD594, FirmwareCatalogConstants.Hash12A73C156CFB,
        FirmwareCatalogConstants.HashE56F618A7F24, FirmwareCatalogConstants.Hash1D20C6429F37,
        FirmwareCatalogConstants.Hash0A2D51BD6C91, FirmwareCatalogConstants.HashEF120B4CF52F,
        FirmwareCatalogConstants.Hash74EA08259BC8, FirmwareCatalogConstants.Hash596991AFC605
    };
    private static readonly HashSet<string> EmuTosVampireV4 = new(StringComparer.OrdinalIgnoreCase)
    {
        FirmwareCatalogConstants.HashC0EFA914FCDF, FirmwareCatalogConstants.Hash91E22BA8399D,
        FirmwareCatalogConstants.Hash41026D228E4A, FirmwareCatalogConstants.Hash83FB05605F89,
        FirmwareCatalogConstants.Hash6286201AF555, FirmwareCatalogConstants.Hash43469EB034F1,
        FirmwareCatalogConstants.HashE3EA0AC84E9C, FirmwareCatalogConstants.Hash60989B1EFE5E,
        FirmwareCatalogConstants.Hash63B0B870B1A7
    };
    private static readonly IReadOnlyDictionary<string, (string Version, FirmwareType Type, string[] Models)> Known =
        new Dictionary<string, (string, FirmwareType, string[])>(StringComparer.OrdinalIgnoreCase)
        {
            [FirmwareCatalogConstants.HashD8DBFF05F1D3] = (FirmwareCatalogConstants.Value10Rev30, FirmwareType.Kickstart, [FirmwareCatalogConstants.A1000]),
            [FirmwareCatalogConstants.Hash0B8442C311CA] = (FirmwareCatalogConstants.Value11Rev31034NTSC, FirmwareType.Kickstart, [FirmwareCatalogConstants.A1000]),
            [FirmwareCatalogConstants.Hash1FA1F93D3D7B] = (FirmwareCatalogConstants.Value11Rev32034PAL, FirmwareType.Kickstart, [FirmwareCatalogConstants.A1000]),
            [FirmwareCatalogConstants.Hash68C9C0826F6C] = (FirmwareCatalogConstants.Value12Rev33166, FirmwareType.Kickstart, [FirmwareCatalogConstants.A1000]),
            [FirmwareCatalogConstants.Hash85AD74194E87] = (FirmwareCatalogConstants.Value12Rev33180, FirmwareType.Kickstart, [FirmwareCatalogConstants.A1000, FirmwareCatalogConstants.A500, FirmwareCatalogConstants.A2000]),
            [FirmwareCatalogConstants.Hash82A21C1890CA] = (FirmwareCatalogConstants.Value13Rev34005, FirmwareType.Kickstart, [FirmwareCatalogConstants.A1000, FirmwareCatalogConstants.A500, FirmwareCatalogConstants.A2000, FirmwareCatalogConstants.CDTV]),
            [FirmwareCatalogConstants.Hash0CBBAACBABCB] = (FirmwareCatalogConstants.Value13Rev34005A3000SuperKickstart, FirmwareType.Kickstart, [FirmwareCatalogConstants.A3000]),
            [FirmwareCatalogConstants.Hash9596B08C4267] = (FirmwareCatalogConstants.Value20Rev36143A3000SuperKickstart, FirmwareType.Kickstart, [FirmwareCatalogConstants.A3000]),
            [FirmwareCatalogConstants.Hash93E6657E563D] = (FirmwareCatalogConstants.Value202Rev36207A3000SuperKickstart, FirmwareType.Kickstart, [FirmwareCatalogConstants.A3000]),
            [FirmwareCatalogConstants.HashDC10D7BDD1B6] = (FirmwareCatalogConstants.Value204Rev37175, FirmwareType.Kickstart, [FirmwareCatalogConstants.A500PLUS]),
            [FirmwareCatalogConstants.HashC5FD2322C53D] = (FirmwareCatalogConstants.Value204Rev37175A3000, FirmwareType.Kickstart, [FirmwareCatalogConstants.A3000]),
            [FirmwareCatalogConstants.Hash72FFCE8541F1] = (FirmwareCatalogConstants.Value205Rev37299, FirmwareType.Kickstart, [FirmwareCatalogConstants.A600]),
            [FirmwareCatalogConstants.HashFA4ACC75B49E] = (FirmwareCatalogConstants.Value205Rev37300, FirmwareType.Kickstart, [FirmwareCatalogConstants.A600]),
            [FirmwareCatalogConstants.Hash465646C9B672] = (FirmwareCatalogConstants.Value205Rev37350, FirmwareType.Kickstart, [FirmwareCatalogConstants.A600]),
            [FirmwareCatalogConstants.HashE40A5DFB3D01] = (FirmwareCatalogConstants.Value31Rev40063, FirmwareType.Kickstart, [FirmwareCatalogConstants.A500, FirmwareCatalogConstants.A600, FirmwareCatalogConstants.A2000]),
            [FirmwareCatalogConstants.HashB7CC148386AA] = (FirmwareCatalogConstants.Value30Rev39106, FirmwareType.Kickstart, [FirmwareCatalogConstants.A1200OG, FirmwareCatalogConstants.A1200]),
            [FirmwareCatalogConstants.Hash646773759326] = (FirmwareCatalogConstants.Value31Rev40068, FirmwareType.Kickstart, [FirmwareCatalogConstants.A1200OG, FirmwareCatalogConstants.A1200]),
            [FirmwareCatalogConstants.Hash9B8BDD5A3FD3] = (FirmwareCatalogConstants.Value30Rev39106, FirmwareType.Kickstart, [FirmwareCatalogConstants.A4000]),
            [FirmwareCatalogConstants.Hash413590E50098] = (FirmwareCatalogConstants.Value31Rev40068, FirmwareType.Kickstart, [FirmwareCatalogConstants.A3000]),
            [FirmwareCatalogConstants.Hash9BDEDDE6A4F3] = (FirmwareCatalogConstants.Value31Rev40068, FirmwareType.Kickstart, [FirmwareCatalogConstants.A4000]),
            [FirmwareCatalogConstants.HashE873C43040B4] = (FirmwareCatalogConstants.Value31Rev40070, FirmwareType.Kickstart, [FirmwareCatalogConstants.A4000]),
            [FirmwareCatalogConstants.Hash89DA1838A244] = (FirmwareCatalogConstants.CDTVExtended10, FirmwareType.ExtendedRom, [FirmwareCatalogConstants.CDTV]),
            [FirmwareCatalogConstants.HashD98112F18792] = (FirmwareCatalogConstants.CDTVA570Extended230, FirmwareType.ExtendedRom, [FirmwareCatalogConstants.CDTV]),
            [FirmwareCatalogConstants.HashD1145AB3A0F8] = (FirmwareCatalogConstants.CDTVExtended27, FirmwareType.ExtendedRom, [FirmwareCatalogConstants.CDTV]),
            [FirmwareCatalogConstants.HashF2F241BF0941] = (FirmwareCatalogConstants.CD32Combined31Rev40060, FirmwareType.Kickstart, [FirmwareCatalogConstants.CD32, FirmwareCatalogConstants.CD32FR]),
            [FirmwareCatalogConstants.Hash5F8924D013DD] = (FirmwareCatalogConstants.CD3231Rev40060, FirmwareType.Kickstart, [FirmwareCatalogConstants.CD32, FirmwareCatalogConstants.CD32FR]),
            [FirmwareCatalogConstants.HashBB72565701B1] = (FirmwareCatalogConstants.CD32Extended31Rev40060, FirmwareType.ExtendedRom, [FirmwareCatalogConstants.CD32, FirmwareCatalogConstants.CD32FR])
        };
    private readonly string _directory;

    public FirmwareCatalog(string directory) => _directory = Path.GetFullPath(directory);

    public IReadOnlyList<Firmware> Scan()
    {
        Directory.CreateDirectory(_directory);
        return Directory.EnumerateFiles(_directory, FirmwareCatalogConstants.Value, SearchOption.AllDirectories)
            .Where(path => Extensions.Contains(Path.GetExtension(path)))
            .Select(CreateEntry)
            .OrderBy(entry => entry.Path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static Firmware Inspect(string path) => CreateEntry(Path.GetFullPath(path));

    private static Firmware CreateEntry(string path)
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
        var type = Path.GetExtension(path).Equals(FirmwareCatalogConstants.Key, StringComparison.OrdinalIgnoreCase) ? FirmwareType.RomKey
            : known ? identity.Type
            : alternative is not null ? FirmwareType.Kickstart
            : Path.GetFileName(path).Contains(FirmwareCatalogConstants.Ext, StringComparison.OrdinalIgnoreCase) ? FirmwareType.ExtendedRom
            : detected is not null ? FirmwareType.Kickstart
            : FirmwareType.Unknown;
        var name = known ? KnownName(identity.Type, identity.Version)
            : alternative?.Name
            ?? (type == FirmwareType.Kickstart ? FirmwareCatalogConstants.Kickstart : null);
        return new Firmware(file.FullName, file.Length, md5, sha256, file.LastWriteTimeUtc,
            type, known || alternative is not null, known, name,
            known ? identity.Version : alternative?.Version ?? detected?.Version,
            known ? identity.Models : alternative?.Models ?? detected?.Models ?? []);
    }

    private static (string Name, string Version, string[] Models)? TryReadAlternativeSystem(FileInfo file, string md5)
    {
        if (file.Length is <= 0 or > 1_048_576) return null;
        var text = System.Text.Encoding.ASCII.GetString(File.ReadAllBytes(file.FullName));
        foreach (var product in new[] { FirmwareCatalogConstants.EmuTOS, FirmwareCatalogConstants.Serena })
        {
            if (!text.Contains(product, StringComparison.OrdinalIgnoreCase)) continue;
            var match = System.Text.RegularExpressions.Regex.Matches(text,
                    FirmwareCatalogConstants.Value09Version0909090209)
                .Cast<System.Text.RegularExpressions.Match>()
                .FirstOrDefault(candidate => candidate.Groups[FirmwareCatalogConstants.Version].Value.Split('.')
                    .Any(component => component != FirmwareCatalogConstants.Value0));
            var version = match is not null ? $"{product} {match.Groups[FirmwareCatalogConstants.Version].Value}" : product;
            if (product == FirmwareCatalogConstants.EmuTOS)
            {
                if (EmuTosVampireV2.Contains(md5)) version += FirmwareCatalogConstants.VampireV2;
                else if (EmuTosVampireV4.Contains(md5)) version += FirmwareCatalogConstants.VampireV4;
                return (product, version, ModelCatalog.All.Select(model => model.Id).ToArray());
            }
            return (product, version, []);
        }
        return null;
    }

    private static bool TryIdentifyKnown(FileInfo file, string md5,
        out (string Version, FirmwareType Type, string[] Models) identity)
    {
        if (Known.TryGetValue(md5, out identity)) return true;
        if (file.Length != 524_288) return false;

        var bytes = File.ReadAllBytes(file.FullName);
        var first = bytes.AsSpan(0, 262_144);
        if (!first.SequenceEqual(bytes.AsSpan(262_144, 262_144))) return false;
        var canonicalMd5 = Convert.ToHexString(MD5.HashData(first)).ToLowerInvariant();
        return Known.TryGetValue(canonicalMd5, out identity);
    }

    private static string KnownName(FirmwareType type, string version)
    {
        if (type == FirmwareType.Kickstart) return FirmwareCatalogConstants.Kickstart;
        if (version.StartsWith(FirmwareCatalogConstants.CD32, StringComparison.OrdinalIgnoreCase)) return FirmwareCatalogConstants.CD32;
        if (version.StartsWith(FirmwareCatalogConstants.CDTVA570, StringComparison.OrdinalIgnoreCase)) return FirmwareCatalogConstants.CDTVA570;
        if (version.StartsWith(FirmwareCatalogConstants.CDTV, StringComparison.OrdinalIgnoreCase)) return FirmwareCatalogConstants.CDTV;
        return FirmwareCatalogConstants.ROM;
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
            <= 32 => new[] { FirmwareCatalogConstants.A1000 },
            <= 34 => new[] { FirmwareCatalogConstants.A1000, FirmwareCatalogConstants.A500, FirmwareCatalogConstants.A2000 },
            36 => new[] { FirmwareCatalogConstants.A3000 },
            37 => new[] { FirmwareCatalogConstants.A500PLUS, FirmwareCatalogConstants.A600, FirmwareCatalogConstants.A3000 },
            39 => new[] { FirmwareCatalogConstants.A1200, FirmwareCatalogConstants.A4000 },
            40 when revision == 60 => new[] { FirmwareCatalogConstants.CD32 },
            40 when revision == 63 => new[] { FirmwareCatalogConstants.A500, FirmwareCatalogConstants.A600, FirmwareCatalogConstants.A2000 },
            40 when revision == 68 => new[] { FirmwareCatalogConstants.A1200, FirmwareCatalogConstants.A3000, FirmwareCatalogConstants.A4000 },
            40 when revision == 70 => new[] { FirmwareCatalogConstants.A4000 },
            >= 40 => new[] { FirmwareCatalogConstants.A500, FirmwareCatalogConstants.A600, FirmwareCatalogConstants.A1200, FirmwareCatalogConstants.A2000, FirmwareCatalogConstants.A3000, FirmwareCatalogConstants.A4000 },
            _ => []
        };
        return ($"{MarketingVersion(version, revision)} rev {version}.{revision:D3}", models);
    }

    private static string MarketingVersion(int version, int revision) => (version, revision) switch
    {
        (31 or 32, _) => FirmwareCatalogConstants.Value11,
        (33, _) => FirmwareCatalogConstants.Value12,
        (34, _) => FirmwareCatalogConstants.Value13,
        (36, <= 199) => FirmwareCatalogConstants.Value20,
        (36, _) => FirmwareCatalogConstants.Value202,
        (37, <= 299) => FirmwareCatalogConstants.Value204,
        (37, _) => FirmwareCatalogConstants.Value205,
        (39, _) => FirmwareCatalogConstants.Value30,
        (40, _) => FirmwareCatalogConstants.Value31,
        _ => $"{version}.{revision:D3}"
    };
}
