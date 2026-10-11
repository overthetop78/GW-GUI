using System.IO;
using System.IO.Compression;
using GWGUI.Emulation.Snk.Emulators.HbMame.Constants;
using FirmwareCopy = GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions.FirmwareRuntimeFunctions;

namespace GWGUI.Emulation.Snk.Emulators.HbMame.Functions;

internal static class FirmwareRuntimeFunctions
{
    private const string BiosDirectory = "bios";
    private const string ZoomRom = "000-lo.lo";
    private const string FixedRom = "sfix.sfix";
    private const string SoundRom = "sm1.sm1";
    private const string DefaultBiosRom = "sp-s2.sp1";
    private const string DefaultBiosId = "euro";
    private const int AuxiliaryRomBytes = 131072;
    private static readonly IReadOnlyList<(string Id, string FileName, int Length)> BiosAlternatives = Array.AsReadOnly(new (string Id, string FileName, int Length)[]
    {
        ("unibios40", "uni-bios_4_0.rom", 131072),
        ("unibios33", "uni-bios_3_3.rom", 131072),
        ("unibios32", "uni-bios_3_2.rom", 131072),
        ("unibios31", "uni-bios_3_1.rom", 131072),
        ("unibios30", "uni-bios_3_0.rom", 131072),
        ("unibios23", "uni-bios_2_3.rom", 131072),
        ("unibios23o", "uni-bios_2_3o.rom", 131072),
        ("unibios22", "uni-bios_2_2.rom", 131072),
        ("unibios21", "uni-bios_2_1.rom", 131072),
        ("unibios20", "uni-bios_2_0.rom", 131072),
        ("unibios13", "uni-bios_1_3.rom", 131072),
        ("unibios12", "uni-bios_1_2.rom", 131072),
        ("unibios12o", "uni-bios_1_2o.rom", 131072),
        ("unibios11", "uni-bios_1_1.rom", 131072),
        ("unibios10", "uni-bios_1_0.rom", 131072),
        ("euro", "sp-s2.sp1", 131072),
        ("euro-s1", "sp-s.sp1", 131072),
        ("asia-mv1c", "sp-45.sp1", 524288),
        ("asia-mv1b", "sp-s3.sp1", 131072),
        ("us", "sp-u2.sp1", 131072),
        ("us-e", "sp-e.sp1", 131072),
        ("us-v2", "sp1-u2", 131072),
        ("us-u4", "sp1-u4.bin", 131072),
        ("us-u3", "sp1-u3.bin", 131072),
        ("japan", "vs-bios.rom", 131072),
        ("japan-s2", "sp-j2.sp1", 131072),
        ("japan-s1", "sp1.jipan.1024", 131072),
        ("japan-mv1b", "japan-j3.bin", 131072),
        ("japan-j3a", "sp1-j3.bin", 131072),
        ("japan-mv1c", "sp-j3.sp1", 524288),
        ("japan-hotel", "sp-1v1_3db8c.bin", 131072),
    });

    internal static IReadOnlyList<FirmwareCategory> Roles => [FirmwareCategory.SystemBios];

    internal static ValueTask<EmulationFirmwareCandidate> Inspect(string path, MachineModel model, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var absolute = Path.GetFullPath(path);
        var compatibility = EmulationFirmwareCompatibility.Incompatible;
        try
        {
            using var archive = ZipFile.OpenRead(absolute);
            if (model == MachineModel.NeoGeoMvs && SelectBios(archive) is not null) compatibility = EmulationFirmwareCompatibility.PartiallyCompatible;
        }
        catch (InvalidDataException) { }
        catch (IOException) { }
        return ValueTask.FromResult(new EmulationFirmwareCandidate(absolute, absolute, Path.GetFileName(absolute), null, compatibility, compatibility == EmulationFirmwareCompatibility.Incompatible ? null : FirmwareFunctions.Field(FirmwareCategory.SystemBios)));
    }

    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        var directory = Path.Combine(systemDirectory, RuntimeOptionConstants.NativeSystemDirectory, BiosDirectory);
        Directory.CreateDirectory(directory);
        var destination = Path.Combine(directory, RuntimeOptionConstants.SystemArchive);
        FirmwareCopy.CopyVerified(configuration, FirmwareCategory.SystemBios, destination);
        using var archive = ZipFile.OpenRead(destination);
        if (SelectBios(archive) is null) throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareFileUnreadable);
    }

    internal static string SelectedBios(MachineConfiguration configuration)
    {
        var firmware = configuration.Firmwares.SingleOrDefault(item => item.Category == FirmwareCategory.SystemBios) ?? throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareFileUnreadable);
        using var archive = ZipFile.OpenRead(firmware.Path);
        return SelectBios(archive) ?? throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareFileUnreadable);
    }

    private static string? SelectBios(ZipArchive archive)
    {
        if (archive.GetEntry(ZoomRom)?.Length != AuxiliaryRomBytes || archive.GetEntry(FixedRom)?.Length != AuxiliaryRomBytes || archive.GetEntry(SoundRom)?.Length != AuxiliaryRomBytes || archive.GetEntry(DefaultBiosRom)?.Length != AuxiliaryRomBytes) return null;
        return BiosAlternatives.FirstOrDefault(bios => bios.Id == DefaultBiosId && archive.GetEntry(bios.FileName)?.Length == bios.Length).Id;
    }
}
