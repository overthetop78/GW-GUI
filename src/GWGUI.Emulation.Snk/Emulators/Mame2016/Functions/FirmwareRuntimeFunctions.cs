using System.IO;
using System.IO.Compression;
using GWGUI.Emulation.Snk.Emulators.Mame2016.Constants;
using FirmwareCopy = GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions.FirmwareRuntimeFunctions;

namespace GWGUI.Emulation.Snk.Emulators.Mame2016.Functions;
internal static class FirmwareRuntimeFunctions
{
    internal const string BiosDirectory = "bios";
    private const int CartridgeBiosBytes = 131072;
    private const int LargeBiosBytes = 524288;
    private const int PocketBiosBytes = 65536;
    private const string ZoomRom = "000-lo.lo";
    private const string FixedRom = "sfix.sfix";
    private const string SoundRom = "sm1.sm1";
    private const string ArchiveExtension = ".zip";
    private static readonly IReadOnlyList<(string Id, string FileName, int Length)> MvsBios = Array.AsReadOnly(new (string Id, string FileName, int Length)[] { ("euro", "sp-s2.sp1", 131072), ("euro-s1", "sp-s.sp1", 131072), ("us", "sp-u2.sp1", 131072), ("us-e", "sp-e.sp1", 131072), ("us-v2", "v2.bin", 131072), ("asia", "asia-s3.rom", 131072), ("japan", "vs-bios.rom", 131072), ("japan-s2", "sp-j2.sp1", 131072), ("japan-s1", "sp1.jipan.1024", 131072), ("mv1c", "sp-45.sp1", 524288), ("japan-j3", "japan-j3.bin", 131072), ("japan-hotel", "sp-1v1_3db8c.bin", 131072), ("unibios32", "uni-bios_3_2.rom", 131072), ("unibios31", "uni-bios_3_1.rom", 131072), ("unibios30", "uni-bios_3_0.rom", 131072), ("unibios23", "uni-bios_2_3.rom", 131072), ("unibios23o", "uni-bios_2_3o.rom", 131072), ("unibios22", "uni-bios_2_2.rom", 131072), ("unibios21", "uni-bios_2_1.rom", 131072), ("unibios20", "uni-bios_2_0.rom", 131072), ("unibios13", "uni-bios_1_3.rom", 131072), ("unibios12", "uni-bios_1_2.rom", 131072), ("unibios12o", "uni-bios_1_2o.rom", 131072), ("unibios11", "uni-bios_1_1.rom", 131072), ("unibios10", "uni-bios_1_0.rom", 131072), });
    private static readonly IReadOnlyList<(string Id, string FileName, int Length)> AesBios = Array.AsReadOnly(new (string Id, string FileName, int Length)[] { ("asia", "neo-epo.bin", CartridgeBiosBytes), ("japan", "neo-po.bin", CartridgeBiosBytes), ("devel", "neodebug.rom", CartridgeBiosBytes) });
    private static readonly IReadOnlyList<(string Id, string FileName, int Length)> CdBios = Array.AsReadOnly(new (string Id, string FileName, int Length)[] { ("top", "top-sp1.bin", LargeBiosBytes), ("front", "front-sp1.bin", LargeBiosBytes) });
    private static readonly IReadOnlyList<(string Id, string FileName, int Length)> CdzBios = Array.AsReadOnly(new (string Id, string FileName, int Length)[] { ("", "neocd.bin", LargeBiosBytes) });
    internal static IReadOnlyList<FirmwareCategory> Roles => [FirmwareCategory.SystemBios];

    internal static string Driver(MachineModel model) => model switch
    {
        MachineModel.NeoGeoAes => "aes",
        MachineModel.NeoGeoMvs => "neogeo",
        MachineModel.NeoGeoPocket => "ngp",
        MachineModel.NeoGeoPocketColor => "ngpc",
        MachineModel.NeoGeoCd => "neocd",
        MachineModel.NeoGeoCdz => "neocdz",
        _ => throw new ArgumentOutOfRangeException(nameof(model))};
    private static string? SelectBios(
        ZipArchive archive,
        MachineModel model
    )
    {
        if (model is MachineModel.NeoGeoPocket or MachineModel.NeoGeoPocketColor)
            return archive.GetEntry(model == MachineModel.NeoGeoPocket ? "ngp_bios.ngp" : "ngpcbios.rom")?.Length == PocketBiosBytes ? string.Empty : null;
        if (archive.GetEntry(ZoomRom)?.Length != CartridgeBiosBytes)
            return null;
        if (model == MachineModel.NeoGeoMvs && (archive.GetEntry(FixedRom)?.Length != CartridgeBiosBytes || archive.GetEntry(SoundRom)?.Length != CartridgeBiosBytes))
            return null;
        IEnumerable<(string Id, string FileName, int Length)> alternatives = model switch
        {
            MachineModel.NeoGeoAes => AesBios.Concat(MvsBios.Where(bios => bios.Id.StartsWith("unibios", StringComparison.Ordinal))),
            MachineModel.NeoGeoMvs => MvsBios,
            MachineModel.NeoGeoCd => CdBios,
            MachineModel.NeoGeoCdz => CdzBios,
            _ => []
        };
        return alternatives.FirstOrDefault(bios => archive.GetEntry(bios.FileName)?.Length == bios.Length).Id;
    }

    internal static ValueTask<EmulationFirmwareCandidate> Inspect(
        string path,
        MachineModel model,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        var absolute = Path.GetFullPath(path);
        var compatibility = EmulationFirmwareCompatibility.Incompatible;
        try
        {
            using var archive = ZipFile.OpenRead(absolute);
            if (SelectBios(archive, model)is not null)
                compatibility = EmulationFirmwareCompatibility.PartiallyCompatible;
        }
        catch (InvalidDataException)
        {
        }
        catch (IOException)
        {
        }

        return ValueTask.FromResult(new EmulationFirmwareCandidate(absolute, absolute, Path.GetFileName(absolute), null, compatibility, compatibility == EmulationFirmwareCompatibility.Incompatible ? null : FirmwareFunctions.Field(FirmwareCategory.SystemBios)));
    }

    internal static void Prepare(
        MachineConfiguration configuration,
        string systemDirectory
    )
    {
        var directory = Path.Combine(systemDirectory, RuntimeOptionConstants.NativeSystemDirectory, BiosDirectory);
        Directory.CreateDirectory(directory);
        var destination = Path.Combine(directory, Driver(configuration.Model) + ArchiveExtension);
        FirmwareCopy.CopyVerified(configuration, FirmwareCategory.SystemBios, destination);
        using var archive = ZipFile.OpenRead(destination);
        if (SelectBios(archive, configuration.Model)is null)
            throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareFileUnreadable);
    }

    internal static string SelectedBios(MachineConfiguration configuration)
    {
        var firmware = configuration.Firmwares.SingleOrDefault(item => item.Category == FirmwareCategory.SystemBios) ?? throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareFileUnreadable);
        using var archive = ZipFile.OpenRead(firmware.Path);
        return SelectBios(archive, configuration.Model) ?? throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareFileUnreadable);
    }
}