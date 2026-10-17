using System.IO;
using System.IO.Compression;
using GWGUI.Emulation.Snk.Emulators.Mame2015.Constants;
using FirmwareCopy = GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions.FirmwareRuntimeFunctions;

namespace GWGUI.Emulation.Snk.Emulators.Mame2015.Functions;

internal static class FirmwareRuntimeFunctions
{
    private const string BiosDirectory = "bios";
    private const string ZoomRom = "000-lo.lo";
    private const string FixedRom = "sfix.sfix";
    private const string SoundRom = "sm1.sm1";
    private const int AuxiliaryRomBytes = 131072;
    private static readonly IReadOnlyList<(string Id, string FileName, int Length)> BiosAlternatives = Array.AsReadOnly(new (string Id, string FileName, int Length)[]
    {
        ("euro", "sp-s2.sp1", 131072),
        ("euro-s1", "sp-s.sp1", 131072),
        ("us", "sp-u2.sp1", 131072),
        ("us-e", "sp-e.sp1", 131072),
        ("asia", "asia-s3.rom", 131072),
        ("japan", "vs-bios.rom", 131072),
        ("japan-s2", "sp-j2.sp1", 131072),
        ("japan-s1", "sp1.jipan.1024", 131072),
        ("mv1c", "sp-45.sp1", 524288),
        ("japan-j3", "japan-j3.bin", 131072),
        ("japan-hotel", "sp-1v1_3db8c.bin", 131072),
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
    });

    internal static IReadOnlyList<FirmwareCategory> Roles => [FirmwareCategory.SystemBios];

    internal static ValueTask<EmulationFirmwareCandidate> Inspect(string path, MachineModel model, CancellationToken cancellationToken) =>
        FirmwareCopy.InspectArchive(path, model, cancellationToken, static (archive, model) => model == MachineModel.NeoGeoMvs && SelectBios(archive) is not null);

    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        var directory = Path.Combine(systemDirectory, RuntimeOptionConstants.NativeSystemDirectory, BiosDirectory);
        var destination = Path.Combine(directory, RuntimeOptionConstants.SystemArchive);
        FirmwareCopy.PrepareArchive(configuration, destination, archive => SelectBios(archive) is not null);
    }

    internal static string SelectedBios(MachineConfiguration configuration) =>
        FirmwareCopy.SelectedArchiveBios(configuration, archive => SelectBios(archive));

    private static string? SelectBios(ZipArchive archive)
    {
        if (archive.GetEntry(ZoomRom)?.Length != AuxiliaryRomBytes || archive.GetEntry(FixedRom)?.Length != AuxiliaryRomBytes || archive.GetEntry(SoundRom)?.Length != AuxiliaryRomBytes) return null;
        return BiosAlternatives.FirstOrDefault(bios => archive.GetEntry(bios.FileName)?.Length == bios.Length).Id;
    }
}
