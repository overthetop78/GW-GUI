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
    private const string ArchiveExtension = ".zip";
    private static readonly IReadOnlyList<(string Id, string FileName, int Length)> MvsBios = Array.AsReadOnly(new (string Id, string FileName, int Length)[] { ("euro", "sp-s2.sp1", 131072),
        ("euro-s1", "sp-s.sp1", 131072),
        ("us", "sp-u2.sp1", 131072),
        ("us-e", "sp-e.sp1", 131072),
        ("us-v2", "v2.bin", 131072),
        ("asia", "asia-s3.rom", 131072),
        ("japan", "vs-bios.rom", 131072),
        ("japan-s2", "sp-j2.sp1", 131072),
        ("japan-s1", "sp1.jipan.1024", 131072),
        ("mv1c", "sp-45.sp1", 524288),
        ("japan-j3", "japan-j3.bin", 131072),
        ("japan-hotel", "sp-1v1_3db8c.bin", 131072),
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
        });
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
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };
    private static string? SelectBios(ZipArchive archive, MachineModel model) =>
        FirmwareCopy.SelectModelBios(archive, model, MvsBios, AesBios, CdBios, CdzBios);

    internal static ValueTask<EmulationFirmwareCandidate> Inspect(string path, MachineModel model, CancellationToken cancellationToken) =>
        FirmwareCopy.InspectArchive(path, model, cancellationToken, static (archive, model) => SelectBios(archive, model) is not null);

    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        var directory = Path.Combine(systemDirectory, RuntimeOptionConstants.NativeSystemDirectory, BiosDirectory);
        var destination = Path.Combine(directory, Driver(configuration.Model) + ArchiveExtension);
        FirmwareCopy.PrepareArchive(configuration, destination, archive => SelectBios(archive, configuration.Model) is not null);
    }

    internal static string SelectedBios(MachineConfiguration configuration) =>
        FirmwareCopy.SelectedArchiveBios(configuration, archive => SelectBios(archive, configuration.Model));
}
