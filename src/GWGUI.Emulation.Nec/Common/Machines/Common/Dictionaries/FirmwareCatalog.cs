using System.IO;
using System.Security.Cryptography;
using GWGUI.Emulation.Nec.Common.Machines.PcEngine.Constants;
using GWGUI.Emulation.Nec.Common.Machines.CoreGrafx.Constants;
using GWGUI.Emulation.Nec.Common.Machines.PcEngineDuo.Constants;
using GWGUI.Emulation.Nec.Common.Machines.PcEngineLt.Constants;
using GWGUI.Emulation.Nec.Common.Machines.SuperGrafx.Constants;
using GWGUI.Emulation.Nec.Common.Machines.LaserActive.Constants;
using GWGUI.Emulation.Nec.Emulators.Geargrafx.Constants;

namespace GWGUI.Emulation.Nec.Common.Machines.Common.Dictionaries;

public sealed class FirmwareCatalog
{
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
        if (name.Equals(GeargrafxFirmwareConstants.PacN1, StringComparison.OrdinalIgnoreCase)
            || name.Equals(GeargrafxFirmwareConstants.PceLp1, StringComparison.OrdinalIgnoreCase)
            || name.Equals(GeargrafxFirmwareConstants.PacN10, StringComparison.OrdinalIgnoreCase))
            return new Firmware(file.FullName, file.Length, md5, sha256,
                file.LastWriteTimeUtc, FirmwareType.SystemRom,
                IsKnown: true, IsOfficial: false,
                Name: Path.GetFileNameWithoutExtension(name), Version: null,
                CompatibleModels: [LaserActiveMachineConstants.Id]);
        if (name.Equals(GeargrafxFirmwareConstants.SystemCard1, StringComparison.OrdinalIgnoreCase)
            || name.Equals(GeargrafxFirmwareConstants.SystemCard2, StringComparison.OrdinalIgnoreCase)
            || name.Equals(GeargrafxFirmwareConstants.GameExpress, StringComparison.OrdinalIgnoreCase))
            return new Firmware(file.FullName, file.Length, md5, sha256,
                file.LastWriteTimeUtc, FirmwareType.SystemRom,
                IsKnown: true, IsOfficial: false,
                Name: Path.GetFileNameWithoutExtension(name), Version: null,
                CompatibleModels: [PcEngineMachineConstants.Id, CoreGrafxMachineConstants.Id,
                    PcEngineDuoMachineConstants.Id, PcEngineLtMachineConstants.Id,
                    SuperGrafxMachineConstants.Id, LaserActiveMachineConstants.Id]);
        return md5 switch
        {
            FirmwareCatalogConstants.SystemCard3Md5 => new Firmware(file.FullName, file.Length,
                md5, sha256, file.LastWriteTimeUtc, FirmwareType.SystemRom,
                IsKnown: true, IsOfficial: true, Name: "System Card 3", Version: "3.00",
                CompatibleModels: [PcEngineMachineConstants.Id, CoreGrafxMachineConstants.Id,
                    PcEngineDuoMachineConstants.Id, PcEngineLtMachineConstants.Id,
                    SuperGrafxMachineConstants.Id, LaserActiveMachineConstants.Id]),
            FirmwareCatalogConstants.PcFxBiosMd5 => new Firmware(file.FullName, file.Length,
                md5, sha256, file.LastWriteTimeUtc, FirmwareType.SystemRom,
                IsKnown: true, IsOfficial: true, Name: "PC-FX BIOS", Version: "1.00",
                CompatibleModels: ["PcFx"]),
            _ => new Firmware(file.FullName, file.Length, md5, sha256, file.LastWriteTimeUtc,
                FirmwareType.Unknown, IsKnown: false, IsOfficial: false,
                Name: null, Version: null, CompatibleModels: [])
        };
    }
}
