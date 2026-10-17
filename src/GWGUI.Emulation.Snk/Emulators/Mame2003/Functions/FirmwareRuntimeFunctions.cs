using System.IO;
using System.IO.Compression;
using GWGUI.Emulation.Snk.Emulators.Mame2003.Constants;
using FirmwareCopy = GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions.FirmwareRuntimeFunctions;

namespace GWGUI.Emulation.Snk.Emulators.Mame2003.Functions;
internal static class FirmwareRuntimeFunctions
{
    private const int BiosRomBytes = 131072;
    private const int ZoomRomBytes = 65536;
    private const string SoundRom = "sm1.sm1";
    private const string FixedRom = "sfix.sfx";
    private const string ZoomRom = "000-lo.lo";
    private static readonly IReadOnlyDictionary<string, string> BiosFiles = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["default"] = "sp-s2.sp1",
        ["euro"] = "sp-s2.sp1",
        ["euro-s1"] = "sp-s.sp1",
        ["us"] = "usa_2slt.bin",
        ["us-e"] = "sp-e.sp1",
        ["asia"] = "asia-s3.rom",
        ["japan"] = "vs-bios.rom",
        ["japan-s2"] = "sp-j2.rom"
    };
    internal static IReadOnlyList<FirmwareCategory> Roles => [FirmwareCategory.SystemBios];

    private static bool IsCompatible(ZipArchive archive) => BiosFiles.Values.Any(file => archive.GetEntry(file)?.Length == BiosRomBytes) && archive.GetEntry(SoundRom)?.Length == BiosRomBytes && archive.GetEntry(FixedRom)?.Length == BiosRomBytes && archive.GetEntry(ZoomRom)?.Length == ZoomRomBytes;
    internal static ValueTask<EmulationFirmwareCandidate> Inspect(string path, MachineModel model, CancellationToken cancellationToken) =>
        FirmwareCopy.InspectArchive(path, model, cancellationToken, static (archive, model) => model == MachineModel.NeoGeoMvs && IsCompatible(archive));

    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        if (configuration.Model != MachineModel.NeoGeoMvs) throw new ArgumentException(nameof(configuration));
        var destination = Path.Combine(systemDirectory, RuntimeOptionConstants.SystemArchive);
        FirmwareCopy.PrepareArchive(configuration, destination, archive => {
            var selected = RuntimeOptionFunctions.Prepare(configuration)[RuntimeOptionConstants.NeogeoBios];
            return IsCompatible(archive) && archive.GetEntry(BiosFiles[selected])?.Length == BiosRomBytes;
        });
    }
}
