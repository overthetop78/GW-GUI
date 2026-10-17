using System.IO;
using System.IO.Compression;
using GWGUI.Emulation.Snk.Emulators.Mame2000.Constants;
using FirmwareCopy = GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions.FirmwareRuntimeFunctions;

namespace GWGUI.Emulation.Snk.Emulators.Mame2000.Functions;

internal static class FirmwareRuntimeFunctions
{
    private const int BiosRomBytes = 131072;
    private const string SystemRom = "neo-geo.rom";
    private const string SoundRom = "ng-sm1.rom";
    private const string FixedRom = "ng-sfix.rom";
    internal static IReadOnlyList<FirmwareCategory> Roles => [FirmwareCategory.SystemBios];
    private static bool IsCompatible(ZipArchive archive) => archive.GetEntry(SystemRom)?.Length == BiosRomBytes && archive.GetEntry(SoundRom)?.Length == BiosRomBytes && archive.GetEntry(FixedRom)?.Length == BiosRomBytes;

    internal static ValueTask<EmulationFirmwareCandidate> Inspect(string path, MachineModel model, CancellationToken cancellationToken) =>
        FirmwareCopy.InspectArchive(path, model, cancellationToken, static (archive, model) => model == MachineModel.NeoGeoMvs && IsCompatible(archive));

    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        if (configuration.Model != MachineModel.NeoGeoMvs) throw new ArgumentException(nameof(configuration));
        var destination = Path.Combine(systemDirectory, RuntimeOptionConstants.SystemArchive);
        FirmwareCopy.PrepareArchive(configuration, destination, archive => IsCompatible(archive));
    }
}
