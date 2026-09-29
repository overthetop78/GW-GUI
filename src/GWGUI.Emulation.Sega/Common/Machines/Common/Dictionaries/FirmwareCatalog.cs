using System.IO;
using System.Security.Cryptography;

namespace GWGUI.Emulation.Sega.Common.Machines.Common.Dictionaries;

public sealed class FirmwareCatalog
{
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        FirmwareCatalogConstants.RomExtension, FirmwareCatalogConstants.BinaryExtension,
        FirmwareCatalogConstants.MasterSystemExtension, FirmwareCatalogConstants.GameGearExtension,
        FirmwareCatalogConstants.ZipExtension
    };

    private static readonly IReadOnlyDictionary<string, (string Name, string Version,
        string[] Models, string[] FileNames)> Known = new Dictionary<string,
        (string, string, string[], string[])>(
        StringComparer.OrdinalIgnoreCase)
    {
        [FirmwareConstants.MegaDriveBiosMd5] = (FirmwareConstants.MegaDriveBiosName,
            FirmwareConstants.MegaDriveBiosFileName, [ModelConstants.MegaDrive],
            [FirmwareConstants.MegaDriveBiosFileName]),
        [FirmwareConstants.MegaCdEuropeBiosMd5] = (FirmwareConstants.MegaCdBiosName,
            FirmwareConstants.RegionEurope, [ModelConstants.MegaDrive],
            [FirmwareConstants.MegaCdEuropeBiosFileName]),
        [FirmwareConstants.MegaCdUnitedStatesBiosMd5] = (FirmwareConstants.MegaCdBiosName,
            FirmwareConstants.RegionUnitedStates, [ModelConstants.MegaDrive],
            [FirmwareConstants.MegaCdUnitedStatesBiosFileName]),
        [FirmwareConstants.MegaCdJapanBiosMd5] = (FirmwareConstants.MegaCdBiosName,
            FirmwareConstants.RegionJapan, [ModelConstants.MegaDrive],
            [FirmwareConstants.MegaCdJapanBiosFileName]),
        [FirmwareConstants.MasterSystemEuropeBiosMd5] = (FirmwareConstants.MasterSystemBiosName,
            FirmwareConstants.RegionEurope, [ModelConstants.MasterSystem, ModelConstants.MarkIII],
            [FirmwareConstants.MasterSystemEuropeBiosFileName,
             FirmwareConstants.MasterSystemUnitedStatesBiosFileName]),
        [FirmwareConstants.MasterSystemJapanBiosMd5] = (FirmwareConstants.MasterSystemBiosName,
            FirmwareConstants.RegionJapan, [ModelConstants.MasterSystem, ModelConstants.MarkIII],
            [FirmwareConstants.MasterSystemJapanBiosFileName]),
        [FirmwareConstants.GameGearBiosMd5] = (FirmwareConstants.GameGearBiosName,
            FirmwareConstants.GameGearBiosFileName, [ModelConstants.GameGear],
            [FirmwareConstants.GameGearBiosFileName]),
        [FirmwareConstants.SaturnBiosMd5] = (FirmwareConstants.SaturnBiosName,
            FirmwareConstants.SaturnBiosFileName, [ModelConstants.Saturn],
            [FirmwareConstants.SaturnBiosFileName]),
        [FirmwareConstants.DreamcastBiosMd5] = (FirmwareConstants.DreamcastBiosName,
            FirmwareConstants.DreamcastBiosFileName, [ModelConstants.Dreamcast],
            [FirmwareConstants.DreamcastBiosRelativeFileName]),
        [FirmwareConstants.NaomiBiosMd5] = (FirmwareConstants.NaomiBiosName,
            FirmwareConstants.NaomiBiosFileName, [ModelConstants.Naomi],
            [FirmwareConstants.NaomiBiosRelativeFileName]),
        [FirmwareConstants.Naomi2BiosMd5] = (FirmwareConstants.Naomi2BiosName,
            FirmwareConstants.Naomi2BiosFileName, [ModelConstants.Naomi2],
            [FirmwareConstants.Naomi2BiosRelativeFileName]),
        [FirmwareConstants.AtomiswaveBiosMd5] = (FirmwareConstants.AtomiswaveBiosName,
            FirmwareConstants.AtomiswaveBiosFileName, [ModelConstants.Atomiswave],
            [FirmwareConstants.AtomiswaveBiosRelativeFileName])
    };

    private readonly string _directory;

    public FirmwareCatalog(string directory) => _directory = Path.GetFullPath(directory);

    public IReadOnlyList<Firmware> Scan()
    {
        Directory.CreateDirectory(_directory);
        return Directory.EnumerateFiles(_directory, FirmwareCatalogConstants.SearchPattern,
                SearchOption.AllDirectories)
            .Where(path => Extensions.Contains(Path.GetExtension(path)))
            .Select(Inspect)
            .OrderBy(entry => entry.Path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static Firmware Inspect(string path)
    {
        var file = new FileInfo(Path.GetFullPath(path));
        using var stream = file.OpenRead();
        var md5 = Convert.ToHexString(MD5.HashData(stream));
        stream.Position = 0;
        var sha256 = Convert.ToHexString(SHA256.HashData(stream));
        var known = Known.TryGetValue(md5, out var identity);
        return new Firmware(file.FullName, file.Length, md5, sha256, file.LastWriteTimeUtc,
            FirmwareType.SystemRom, known, known, known ? identity.Name : null,
            known ? identity.Version : null, known ? identity.Models : [],
            known ? identity.FileNames : []);
    }

    internal static bool TryIdentifyKnown(string md5,
        out (string Name, string Version, string[] Models, string[] FileNames) identity) =>
        Known.TryGetValue(md5, out identity);
}
