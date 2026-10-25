using System.IO;
using FirmwareCopy = GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions.FirmwareRuntimeFunctions;
using System.IO.Compression;
using GWGUI.Emulation.Snk.Emulators.FbAlpha2012.Constants;

namespace GWGUI.Emulation.Snk.Emulators.FbAlpha2012.Functions;
internal static class FirmwareRuntimeFunctions
{
    private static readonly IReadOnlyDictionary<string, int> BiosAlternatives = new Dictionary<string, int>(StringComparer.Ordinal)
    {
        ["asia-s3.rom"] = 131072,
        ["sp-s2.sp1"] = 131072,
        ["sp-s.sp1"] = 131072,
        ["sp-u2.sp1"] = 131072,
        ["sp-e.sp1"] = 131072,
        ["vs-bios.rom"] = 131072,
        ["sp-j2.sp1"] = 131072,
        ["sp1.jipan.1024"] = 131072,
        ["sp-45.sp1"] = 524288,
        ["japan-j3.bin"] = 131072,
        ["neo-po.bin"] = 131072,
        ["neo-epo.bin"] = 131072,
        ["neodebug.bin"] = 131072,
        ["sp-1v1_3db8c.bin"] = 131072,
        ["uni-bios_4_0.rom"] = 131072,
        ["uni-bios_3_3.rom"] = 131072,
        ["uni-bios_3_2.rom"] = 131072,
        ["uni-bios_3_1.rom"] = 131072,
        ["uni-bios_3_0.rom"] = 131072,
        ["uni-bios_2_3.rom"] = 131072,
        ["uni-bios_2_3o.rom"] = 131072,
        ["uni-bios_2_2.rom"] = 131072,
        ["uni-bios_2_1.rom"] = 131072,
        ["uni-bios_2_0.rom"] = 131072,
        ["uni-bios_1_3.rom"] = 131072,
        ["uni-bios_1_2.rom"] = 131072,
        ["uni-bios_1_2o.rom"] = 131072,
        ["uni-bios_1_1.rom"] = 131072,
        ["uni-bios_1_0.rom"] = 131072,
        ["neopen.sp1"] = 131072,
    };
    internal static IReadOnlyList<FirmwareCategory> Roles => [FirmwareCategory.SystemBios];

    internal static ValueTask<EmulationFirmwareCandidate> Inspect(string path, MachineModel model, CancellationToken cancellationToken) =>
        FirmwareCopy.InspectArchive(path, model, cancellationToken, static (archive, model) => Valid(archive, model, null));

    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        var directory = systemDirectory;
        var archiveName = RuntimeOptionConstants.SystemArchive;
        var destination = Path.Combine(directory, archiveName);
        FirmwareCopy.PrepareArchive(configuration, destination, archive => {
            var mode = RuntimeOptionFunctions.Prepare(configuration).GetValueOrDefault(RuntimeOptionConstants.NeoGeoMode);
            return Valid(archive, configuration.Model, mode);
        });
    }

    private static bool Valid(ZipArchive archive, MachineModel model, string? mode) =>
        FirmwareCopy.ValidCartridgeArchive(archive, model, mode, BiosAlternatives,
            RuntimeOptionConstants.Aes, RuntimeOptionConstants.Mvs, RuntimeOptionConstants.UniverseBios);
}
