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

    internal static ValueTask<EmulationFirmwareCandidate> Inspect(string path, MachineModel model, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var absolute = Path.GetFullPath(path);
        var compatibility = EmulationFirmwareCompatibility.Incompatible;
        try
        {
            using var archive = ZipFile.OpenRead(absolute);
            if (model == MachineModel.NeoGeoMvs && IsCompatible(archive)) compatibility = EmulationFirmwareCompatibility.PartiallyCompatible;
        }
        catch (InvalidDataException) { }
        catch (IOException) { }
        return ValueTask.FromResult(new EmulationFirmwareCandidate(absolute, absolute, Path.GetFileName(absolute), null, compatibility, compatibility == EmulationFirmwareCompatibility.Incompatible ? null : FirmwareFunctions.Field(FirmwareCategory.SystemBios)));
    }

    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        if (configuration.Model != MachineModel.NeoGeoMvs) throw new ArgumentException(nameof(configuration));
        var destination = Path.Combine(systemDirectory, RuntimeOptionConstants.SystemArchive);
        FirmwareCopy.CopyVerified(configuration, FirmwareCategory.SystemBios, destination);
        using var archive = ZipFile.OpenRead(destination);
        if (!IsCompatible(archive)) throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareFileUnreadable);
    }
}
