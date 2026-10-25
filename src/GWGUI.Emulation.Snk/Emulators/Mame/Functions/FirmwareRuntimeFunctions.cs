using System.IO;
using System.IO.Compression;
using GWGUI.Emulation.Snk.Emulators.Mame.Constants;
using FirmwareCopy = GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions.FirmwareRuntimeFunctions;

namespace GWGUI.Emulation.Snk.Emulators.Mame.Functions;
internal static class FirmwareRuntimeFunctions
{
    internal const string BiosDirectory = "bios";
    private const int CartridgeBiosBytes = 131072;
    private const int LargeBiosBytes = 524288;
    private const string ArchiveExtension = ".zip";
    private static readonly IReadOnlyList<(string Id, string FileName, int Length)> MvsBios = Array.AsReadOnly(new (string Id, string FileName, int Length)[] { ("unibios40", "uni-bios_4_0.rom", CartridgeBiosBytes),
        ("unibios33", "uni-bios_3_3.rom", CartridgeBiosBytes),
        ("unibios32", "uni-bios_3_2.rom", CartridgeBiosBytes),
        ("unibios31", "uni-bios_3_1.rom", CartridgeBiosBytes),
        ("unibios30", "uni-bios_3_0.rom", CartridgeBiosBytes),
        ("unibios23", "uni-bios_2_3.rom", CartridgeBiosBytes),
        ("unibios23o", "uni-bios_2_3o.rom", CartridgeBiosBytes),
        ("unibios22", "uni-bios_2_2.rom", CartridgeBiosBytes),
        ("unibios21", "uni-bios_2_1.rom", CartridgeBiosBytes),
        ("unibios20", "uni-bios_2_0.rom", CartridgeBiosBytes),
        ("unibios13", "uni-bios_1_3.rom", CartridgeBiosBytes),
        ("unibios12", "uni-bios_1_2.rom", CartridgeBiosBytes),
        ("unibios12o", "uni-bios_1_2o.rom", CartridgeBiosBytes),
        ("unibios11", "uni-bios_1_1.rom", CartridgeBiosBytes),
        ("unibios10", "uni-bios_1_0.rom", CartridgeBiosBytes),
        ("euro", "sp-s2.sp1", CartridgeBiosBytes),
        ("euro-s1", "sp-s.sp1", CartridgeBiosBytes),
        ("asia-mv1c", "sp-45.sp1", LargeBiosBytes),
        ("asia-mv1b", "sp-s3.sp1", CartridgeBiosBytes),
        ("us", "sp-u2.sp1", CartridgeBiosBytes),
        ("us-e", "sp-e.sp1", CartridgeBiosBytes),
        ("us-v2", "sp1-u2", CartridgeBiosBytes),
        ("us-u4", "sp1-u4.bin", CartridgeBiosBytes),
        ("us-u3", "sp1-u3.bin", CartridgeBiosBytes),
        ("japan", "vs-bios.rom", CartridgeBiosBytes),
        ("japan-s2", "sp-j2.sp1", CartridgeBiosBytes),
        ("japan-s1", "sp1.jipan.1024", CartridgeBiosBytes),
        ("japan-mv1b", "japan-j3.bin", CartridgeBiosBytes),
        ("japan-j3a", "sp1-j3.bin", CartridgeBiosBytes),
        ("japan-mv1c", "sp-j3.sp1", LargeBiosBytes),
        ("japan-hotel", "sp-1v1_3db8c.bin", CartridgeBiosBytes),
        });
    private static readonly IReadOnlyList<(string Id, string FileName, int Length)> AesBios = Array.AsReadOnly(new (string Id, string FileName, int Length)[] { ("asia", "neo-epo.bin", CartridgeBiosBytes), ("japan", "neo-po.bin", CartridgeBiosBytes), ("devel", "neodebug.rom", CartridgeBiosBytes) });
    private static readonly IReadOnlyList<(string Id, string FileName, int Length)> CdBios = Array.AsReadOnly(new (string Id, string FileName, int Length)[] { ("top", "top-sp1.bin", LargeBiosBytes),
        ("topproto", "prototype_neocd_ver_0.02.bin", LargeBiosBytes),
        ("front", "front-sp1.bin", LargeBiosBytes),
        ("unibios32", "uni-bioscd32.rom", LargeBiosBytes),
        ("unibios33", "uni-bioscd33.rom", LargeBiosBytes) });
    private static readonly IReadOnlyList<(string Id, string FileName, int Length)> CdzBios = Array.AsReadOnly(new (string Id, string FileName, int Length)[] { ("official", "neocd.bin", LargeBiosBytes),
        ("unibios32", "uni-bioscd32.rom", LargeBiosBytes),
        ("unibios33", "uni-bioscd33.rom", LargeBiosBytes) });
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
