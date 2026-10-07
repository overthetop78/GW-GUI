using System.IO;
using System.Security.Cryptography;

namespace GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Dictionaries;

public sealed class AmigaFirmwareCatalog
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
            [FirmwareCatalogConstants.HashD8DBFF05F1D3] = (FirmwareCatalogConstants.Kickstart10Revision30, FirmwareType.Kickstart, [FirmwareCatalogConstants.A1000]),
            [FirmwareCatalogConstants.Hash0B8442C311CA] = (FirmwareCatalogConstants.Kickstart11Revision31034NTSC, FirmwareType.Kickstart, [FirmwareCatalogConstants.A1000]),
            [FirmwareCatalogConstants.Hash1FA1F93D3D7B] = (FirmwareCatalogConstants.Kickstart11Revision32034PAL, FirmwareType.Kickstart, [FirmwareCatalogConstants.A1000]),
            [FirmwareCatalogConstants.Hash68C9C0826F6C] = (FirmwareCatalogConstants.Kickstart12Revision33166, FirmwareType.Kickstart, [FirmwareCatalogConstants.A1000]),
            [FirmwareCatalogConstants.Hash85AD74194E87] = (FirmwareCatalogConstants.Kickstart12Revision33180, FirmwareType.Kickstart, [FirmwareCatalogConstants.A1000, FirmwareCatalogConstants.A500, FirmwareCatalogConstants.A2000]),
            [FirmwareCatalogConstants.Hash82A21C1890CA] = (FirmwareCatalogConstants.Kickstart13Revision34005, FirmwareType.Kickstart, [FirmwareCatalogConstants.A1000, FirmwareCatalogConstants.A500, FirmwareCatalogConstants.A2000, FirmwareCatalogConstants.CDTV]),
            [FirmwareCatalogConstants.Hash0CBBAACBABCB] = (FirmwareCatalogConstants.Kickstart13Revision34005A3000SuperKickstart, FirmwareType.Kickstart, [FirmwareCatalogConstants.A3000]),
            [FirmwareCatalogConstants.Hash9596B08C4267] = (FirmwareCatalogConstants.Kickstart20Revision36143A3000SuperKickstart, FirmwareType.Kickstart, [FirmwareCatalogConstants.A3000]),
            [FirmwareCatalogConstants.Hash93E6657E563D] = (FirmwareCatalogConstants.Kickstart202Revision36207A3000SuperKickstart, FirmwareType.Kickstart, [FirmwareCatalogConstants.A3000]),
            [FirmwareCatalogConstants.HashDC10D7BDD1B6] = (FirmwareCatalogConstants.Kickstart204Revision37175, FirmwareType.Kickstart, [FirmwareCatalogConstants.A500PLUS]),
            [FirmwareCatalogConstants.HashC5FD2322C53D] = (FirmwareCatalogConstants.Kickstart204Revision37175A3000, FirmwareType.Kickstart, [FirmwareCatalogConstants.A3000]),
            [FirmwareCatalogConstants.Hash72FFCE8541F1] = (FirmwareCatalogConstants.Kickstart205Revision37299, FirmwareType.Kickstart, [FirmwareCatalogConstants.A600]),
            [FirmwareCatalogConstants.HashFA4ACC75B49E] = (FirmwareCatalogConstants.Kickstart205Revision37300, FirmwareType.Kickstart, [FirmwareCatalogConstants.A600]),
            [FirmwareCatalogConstants.Hash465646C9B672] = (FirmwareCatalogConstants.Kickstart205Revision37350, FirmwareType.Kickstart, [FirmwareCatalogConstants.A600]),
            [FirmwareCatalogConstants.HashE40A5DFB3D01] = (FirmwareCatalogConstants.Kickstart31Revision40063, FirmwareType.Kickstart, [FirmwareCatalogConstants.A500, FirmwareCatalogConstants.A600, FirmwareCatalogConstants.A2000]),
            [FirmwareCatalogConstants.HashB7CC148386AA] = (FirmwareCatalogConstants.Kickstart30Revision39106, FirmwareType.Kickstart, [FirmwareCatalogConstants.A1200]),
            [FirmwareCatalogConstants.Hash646773759326] = (FirmwareCatalogConstants.Kickstart31Revision40068, FirmwareType.Kickstart, [FirmwareCatalogConstants.A1200]),
            [FirmwareCatalogConstants.Hash9B8BDD5A3FD3] = (FirmwareCatalogConstants.Kickstart30Revision39106, FirmwareType.Kickstart, [FirmwareCatalogConstants.A4000]),
            [FirmwareCatalogConstants.Hash413590E50098] = (FirmwareCatalogConstants.Kickstart31Revision40068, FirmwareType.Kickstart, [FirmwareCatalogConstants.A3000]),
            [FirmwareCatalogConstants.Hash9BDEDDE6A4F3] = (FirmwareCatalogConstants.Kickstart31Revision40068, FirmwareType.Kickstart, [FirmwareCatalogConstants.A4000]),
            [FirmwareCatalogConstants.HashE873C43040B4] = (FirmwareCatalogConstants.Kickstart31Revision40070, FirmwareType.Kickstart, [FirmwareCatalogConstants.A4000]),
            [FirmwareCatalogConstants.Hash89DA1838A244] = (FirmwareCatalogConstants.CDTVExtended10, FirmwareType.ExtendedRom, [FirmwareCatalogConstants.CDTV]),
            [FirmwareCatalogConstants.HashD98112F18792] = (FirmwareCatalogConstants.CDTVA570Extended230, FirmwareType.ExtendedRom, [FirmwareCatalogConstants.CDTV]),
            [FirmwareCatalogConstants.HashD1145AB3A0F8] = (FirmwareCatalogConstants.CDTVExtended27, FirmwareType.ExtendedRom, [FirmwareCatalogConstants.CDTV]),
            [FirmwareCatalogConstants.HashF2F241BF0941] = (FirmwareCatalogConstants.CD32Combined31Rev40060, FirmwareType.Kickstart, [FirmwareCatalogConstants.CD32]),
            [FirmwareCatalogConstants.Hash5F8924D013DD] = (FirmwareCatalogConstants.CD3231Rev40060, FirmwareType.Kickstart, [FirmwareCatalogConstants.CD32]),
            [FirmwareCatalogConstants.HashBB72565701B1] = (FirmwareCatalogConstants.CD32Extended31Rev40060, FirmwareType.ExtendedRom, [FirmwareCatalogConstants.CD32])
        };
    private static IReadOnlyList<string> CompatibleAmigaModels { get; } =
    [
        ModelConstants.A500, ModelConstants.A500PLUS, ModelConstants.A600, ModelConstants.A1000,
        ModelConstants.A1200, ModelConstants.A2000, ModelConstants.A3000, ModelConstants.A4000,
        GWGUI.Emulation.Commodore.Common.Machines.AmigaCD32.Constants.ModelConstants.CD32,
        GWGUI.Emulation.Commodore.Common.Machines.CommodoreCDTV.Constants.ModelConstants.CDTV
    ];
    private readonly string _directory;

    public AmigaFirmwareCatalog(string directory) => _directory = Path.GetFullPath(directory);

    public IReadOnlyList<AmigaFirmware> Scan()
    {
        Directory.CreateDirectory(_directory);
        return Directory.EnumerateFiles(_directory, FirmwareCatalogConstants.AllFilesPattern, SearchOption.AllDirectories)
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
        stream.Position = FirmwareCatalogConstants.StreamStart;
        var sha256 = Convert.ToHexString(SHA256.HashData(stream));
        stream.Position = FirmwareCatalogConstants.StreamStart;
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
        return new AmigaFirmware(file.FullName, file.Length, md5, sha256, file.LastWriteTimeUtc,
            type, known || alternative is not null, known, name,
            known ? identity.Version : alternative?.Version ?? detected?.Version,
            known ? identity.Models : alternative?.Models ?? detected?.Models ?? []);
    }

    private static (string Name, string Version, string[] Models)? TryReadAlternativeSystem(FileInfo file, string md5)
    {
        if (file.Length is <= FirmwareCatalogConstants.MinimumFirmwareLength or > FirmwareCatalogConstants.MaximumAlternativeFirmwareLength) return null;
        var text = System.Text.Encoding.ASCII.GetString(File.ReadAllBytes(file.FullName));
        foreach (var product in new[] { FirmwareCatalogConstants.EmuTOS, FirmwareCatalogConstants.Serena })
        {
            if (!text.Contains(product, StringComparison.OrdinalIgnoreCase)) continue;
            var match = System.Text.RegularExpressions.Regex.Matches(text,
                    FirmwareCatalogConstants.AlternativeVersionPattern)
                .Cast<System.Text.RegularExpressions.Match>()
                .FirstOrDefault(candidate => candidate.Groups[FirmwareCatalogConstants.Version].Value.Split(FirmwareCatalogConstants.VersionComponentSeparator)
                    .Any(component => component != FirmwareCatalogConstants.ZeroVersionComponent));
            var version = match is not null ? string.Concat(product, FirmwareCatalogConstants.ProductVersionSeparator, match.Groups[FirmwareCatalogConstants.Version].Value) : product;
            if (product == FirmwareCatalogConstants.EmuTOS)
            {
                if (EmuTosVampireV2.Contains(md5)) version += FirmwareCatalogConstants.VampireV2;
                else if (EmuTosVampireV4.Contains(md5)) version += FirmwareCatalogConstants.VampireV4;
                return (product, version, CompatibleAmigaModels.ToArray());
            }
            return (product, version, []);
        }
        return null;
    }

    private static bool TryIdentifyKnown(FileInfo file, string md5,
        out (string Version, FirmwareType Type, string[] Models) identity)
    {
        if (Known.TryGetValue(md5, out identity)) return true;
        if (file.Length != FirmwareCatalogConstants.RepeatedKickstartLength) return false;

        var bytes = File.ReadAllBytes(file.FullName);
        var first = bytes.AsSpan(BufferConstants.FirstBufferIndex, FirmwareCatalogConstants.CanonicalKickstartLength);
        if (!first.SequenceEqual(bytes.AsSpan(FirmwareCatalogConstants.CanonicalKickstartLength, FirmwareCatalogConstants.CanonicalKickstartLength))) return false;
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
        if (length is not (FirmwareCatalogConstants.CanonicalKickstartLength or FirmwareCatalogConstants.RepeatedKickstartLength or FirmwareCatalogConstants.ExtendedKickstartLength)) return null;
        Span<byte> header = stackalloc byte[FirmwareCatalogConstants.KickstartHeaderSize];
        if (stream.Read(header) != header.Length) return null;
        var version = (header[FirmwareCatalogConstants.VersionHighByteOffset] << FirmwareCatalogConstants.BitsPerByte) | header[FirmwareCatalogConstants.VersionLowByteOffset];
        var revision = (header[FirmwareCatalogConstants.RevisionHighByteOffset] << FirmwareCatalogConstants.BitsPerByte) | header[FirmwareCatalogConstants.RevisionLowByteOffset];
        if (version is < FirmwareCatalogConstants.MinimumNativeVersion or > FirmwareCatalogConstants.MaximumNativeVersion || revision > FirmwareCatalogConstants.MaximumRecognizedRevision) return null;
        var models = version switch
        {
            <= (int)NativeKickstartVersion.Kickstart11Pal => new[] { FirmwareCatalogConstants.A1000 },
            <= (int)NativeKickstartVersion.Kickstart13 => new[] { FirmwareCatalogConstants.A1000, FirmwareCatalogConstants.A500, FirmwareCatalogConstants.A2000 },
            (int)NativeKickstartVersion.Kickstart20 => new[] { FirmwareCatalogConstants.A3000 },
            (int)NativeKickstartVersion.Kickstart204 => new[] { FirmwareCatalogConstants.A500PLUS, FirmwareCatalogConstants.A600, FirmwareCatalogConstants.A3000 },
            (int)NativeKickstartVersion.Kickstart30 => new[] { FirmwareCatalogConstants.A1200, FirmwareCatalogConstants.A4000 },
            (int)NativeKickstartVersion.Kickstart31 when revision == (int)NativeKickstartRevision.Kickstart31Cd32 => new[] { FirmwareCatalogConstants.CD32 },
            (int)NativeKickstartVersion.Kickstart31 when revision == (int)NativeKickstartRevision.Kickstart31A600 => new[] { FirmwareCatalogConstants.A500, FirmwareCatalogConstants.A600, FirmwareCatalogConstants.A2000 },
            (int)NativeKickstartVersion.Kickstart31 when revision == (int)NativeKickstartRevision.Kickstart31A1200 => new[] { FirmwareCatalogConstants.A1200, FirmwareCatalogConstants.A3000, FirmwareCatalogConstants.A4000 },
            (int)NativeKickstartVersion.Kickstart31 when revision == (int)NativeKickstartRevision.Kickstart31A4000 => new[] { FirmwareCatalogConstants.A4000 },
            >= (int)NativeKickstartVersion.Kickstart31 => new[] { FirmwareCatalogConstants.A500, FirmwareCatalogConstants.A600, FirmwareCatalogConstants.A1200, FirmwareCatalogConstants.A2000, FirmwareCatalogConstants.A3000, FirmwareCatalogConstants.A4000 },
            _ => []
        };
        return (string.Format(FirmwareCatalogConstants.DetectedVersionFormat, MarketingVersion(version, revision), version, revision), models);
    }

    private static string MarketingVersion(int version, int revision) => (version, revision) switch
    {
        ((int)NativeKickstartVersion.Kickstart11Ntsc or (int)NativeKickstartVersion.Kickstart11Pal, _) => FirmwareCatalogConstants.VersionLabels[KickstartVersion._1_1],
        ((int)NativeKickstartVersion.Kickstart12, _) => FirmwareCatalogConstants.VersionLabels[KickstartVersion._1_2],
        ((int)NativeKickstartVersion.Kickstart13, _) => FirmwareCatalogConstants.VersionLabels[KickstartVersion._1_3],
        ((int)NativeKickstartVersion.Kickstart20, <= FirmwareCatalogConstants.LastKickstart20Revision) => FirmwareCatalogConstants.VersionLabels[KickstartVersion._2_0],
        ((int)NativeKickstartVersion.Kickstart20, _) => FirmwareCatalogConstants.VersionLabels[KickstartVersion._2_02],
        ((int)NativeKickstartVersion.Kickstart204, <= FirmwareCatalogConstants.LastKickstart204Revision) => FirmwareCatalogConstants.VersionLabels[KickstartVersion._2_04],
        ((int)NativeKickstartVersion.Kickstart204, _) => FirmwareCatalogConstants.VersionLabels[KickstartVersion._2_05],
        ((int)NativeKickstartVersion.Kickstart30, _) => FirmwareCatalogConstants.VersionLabels[KickstartVersion._3_0],
        ((int)NativeKickstartVersion.Kickstart31, _) => FirmwareCatalogConstants.VersionLabels[KickstartVersion._3_1],
        _ => string.Format(FirmwareCatalogConstants.NativeVersionFormat, version, revision)
    };
}
