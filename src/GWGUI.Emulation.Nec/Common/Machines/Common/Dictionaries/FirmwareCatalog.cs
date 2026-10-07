using System.IO;
using System.Security.Cryptography;
using GWGUI.Emulation.Nec.Common.Machines.PcEngine.Constants;
using GWGUI.Emulation.Nec.Common.Machines.CoreGrafx.Constants;
using GWGUI.Emulation.Nec.Common.Machines.PcEngineDuo.Constants;
using GWGUI.Emulation.Nec.Common.Machines.PcEngineLt.Constants;
using GWGUI.Emulation.Nec.Common.Machines.SuperGrafx.Constants;
using GWGUI.Emulation.Nec.Common.Machines.LaserActive.Constants;
using GWGUI.Emulation.Nec.Common.Machines.PcFx.Constants;
using GWGUI.Emulation.Nec.Emulators.Geargrafx.Constants;

namespace GWGUI.Emulation.Nec.Common.Machines.Common.Dictionaries;

public sealed class FirmwareCatalog
{
    private static readonly string[] PcEngineCdModels =
    [
        PcEngineMachineConstants.Id, CoreGrafxMachineConstants.Id,
        PcEngineDuoMachineConstants.Id, PcEngineLtMachineConstants.Id,
        SuperGrafxMachineConstants.Id, LaserActiveMachineConstants.Id,
        Machines.PcEngineCd.Constants.MachineConstants.Id,
        Machines.PcEngineSuperCd.Constants.MachineConstants.Id,
        Machines.PcEngineArcadeCard.Constants.MachineConstants.Id
    ];

    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        FirmwareCatalogConstants.RomExtension,
        FirmwareCatalogConstants.PcEngineExtension,
        FirmwareCatalogConstants.BinaryExtension
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
        var name = file.Name;
        var identified = md5 switch
        {
            FirmwareCatalogConstants.SystemCard1Md5 => Pce("CD-ROM System Card", "1.0"),
            FirmwareCatalogConstants.SystemCard2Md5 => Pce("CD-ROM System Card", "2.0"),
            FirmwareCatalogConstants.SystemCard21Md5 => Pce("CD-ROM System Card", "2.1"),
            FirmwareCatalogConstants.SystemCard2UsMd5 => Pce("TurboGrafx CD System Card", "2.0"),
            FirmwareCatalogConstants.SystemCard3Md5 => Pce("Super CD-ROM System Card", "3.0", true),
            FirmwareCatalogConstants.SystemCard3UsMd5 => Pce("TurboGrafx CD Super System Card", "3.0"),
            FirmwareCatalogConstants.GamesExpressBlueMd5 => Pce("Games Express CD Card (Blue)", null),
            FirmwareCatalogConstants.GamesExpressGreenMd5 => Pce("Games Express CD Card (Green)", null,
                verified: false),
            FirmwareCatalogConstants.PcFxBiosMd5 => PcFx("PC-FX BIOS", "1.00", true),
            FirmwareCatalogConstants.PcFxFrenchBiosMd5 => PcFx("PC-FX BIOS (FR)", "1.00",
                verified: false),
            FirmwareCatalogConstants.PcFxBios101Md5 => PcFx("PC-FX BIOS", "1.01",
                verified: false),
            FirmwareCatalogConstants.PcFxGaBiosMd5 => PcFx("PC-FX GA BIOS", null,
                verified: false),
            FirmwareCatalogConstants.PcFxScsiMd5 => new Firmware(file.FullName, file.Length,
                md5, sha256, file.LastWriteTimeUtc, FirmwareType.SystemRom,
                IsKnown: true, IsOfficial: false, Name: "PC-FX SCSI ROM", Version: null,
                CompatibleModels: []),
            _ => new Firmware(file.FullName, file.Length, md5, sha256, file.LastWriteTimeUtc,
                FirmwareType.Unknown, IsKnown: false, IsOfficial: false,
                Name: null, Version: null, CompatibleModels: [])
        };
        if (identified.IsKnown) return identified;
        if (name.Equals(GeargrafxFirmwareConstants.PacN1, StringComparison.OrdinalIgnoreCase)
            || name.Equals(GeargrafxFirmwareConstants.PceLp1, StringComparison.OrdinalIgnoreCase)
            || name.Equals(GeargrafxFirmwareConstants.PacN10, StringComparison.OrdinalIgnoreCase))
            return new Firmware(file.FullName, file.Length, md5, sha256,
                file.LastWriteTimeUtc, FirmwareType.SystemRom,
                IsKnown: true, IsOfficial: false,
                Name: Path.GetFileNameWithoutExtension(name), Version: null,
                CompatibleModels: [LaserActiveMachineConstants.Id]);
        return identified;

        Firmware Pce(string firmwareName, string? version, bool official = false,
            bool verified = true) => new(file.FullName, file.Length, md5, sha256,
            file.LastWriteTimeUtc, FirmwareType.SystemRom, IsKnown: true,
            IsOfficial: official, Name: firmwareName, Version: version,
            CompatibleModels: PcEngineCdModels, IsVerified: verified);

        Firmware PcFx(string firmwareName, string? version, bool official = false,
            bool verified = true) => new(file.FullName, file.Length, md5, sha256,
            file.LastWriteTimeUtc, FirmwareType.SystemRom, IsKnown: true,
            IsOfficial: official, Name: firmwareName, Version: version,
            CompatibleModels: [PcFxMachineConstants.Id], IsVerified: verified);
    }
}
