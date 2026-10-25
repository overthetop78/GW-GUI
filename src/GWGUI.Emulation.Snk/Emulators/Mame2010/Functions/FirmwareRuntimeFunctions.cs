using System.IO;
using System.IO.Compression;
using GWGUI.Emulation.Snk.Emulators.Mame2010.Constants;
using FirmwareCopy = GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions.FirmwareRuntimeFunctions;

namespace GWGUI.Emulation.Snk.Emulators.Mame2010.Functions;

internal static class FirmwareRuntimeFunctions
{
    private const int BiosRomBytes = 131072;
    private const string SystemRom = "sp-s2.sp1";
    private const string SoundRom = "sm1.sm1";
    private const string FixedRom = "sfix.sfix";
    private const string ZoomRom = "000-lo.lo";
    internal static IReadOnlyList<FirmwareCategory> Roles => [FirmwareCategory.SystemBios];
    private static bool IsCompatible(ZipArchive archive) => archive.GetEntry(SystemRom)?.Length == BiosRomBytes && archive.GetEntry(SoundRom)?.Length == BiosRomBytes && archive.GetEntry(FixedRom)?.Length == BiosRomBytes && archive.GetEntry(ZoomRom)?.Length == BiosRomBytes;

    internal static ValueTask<EmulationFirmwareCandidate> Inspect(string path, MachineModel model, CancellationToken cancellationToken) =>
        FirmwareCopy.InspectArchive(path, model, cancellationToken, static (archive, model) => model == MachineModel.NeoGeoMvs && IsCompatible(archive));

    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        if (configuration.Model != MachineModel.NeoGeoMvs) throw new ArgumentException(nameof(configuration));
        var destination = Path.Combine(systemDirectory, RuntimeOptionConstants.SystemArchive);
        FirmwareCopy.PrepareArchive(configuration, destination, archive => IsCompatible(archive));
    }
}
