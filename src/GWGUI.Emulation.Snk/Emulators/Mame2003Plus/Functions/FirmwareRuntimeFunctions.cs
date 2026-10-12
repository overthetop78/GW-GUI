using System.IO;
using System.IO.Compression;
using GWGUI.Emulation.Snk.Emulators.Mame2003Plus.Constants;
using FirmwareCopy = GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions.FirmwareRuntimeFunctions;

namespace GWGUI.Emulation.Snk.Emulators.Mame2003Plus.Functions;
internal static class FirmwareRuntimeFunctions
{
    private const int BiosRomBytes = 131072;
    private const int ZoomRomBytes = 65536;
    private const string SoundRom = "mame.sm1";
    private const string FixedRom = "sfix.sfx";
    private const string ZoomRom = "mamelo.lo";
    private static readonly IReadOnlyDictionary<string, string> BiosFiles = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["default"] = "sp-s2.sp1",
        ["euro"] = "sp-s2.sp1",
        ["euro-s1"] = "sp-s.sp1",
        ["us"] = "usa_2slt.bin",
        ["us-e"] = "sp-e.sp1",
        ["asia"] = "asia-s3.rom",
        ["japan"] = "vs-bios.rom",
        ["japan-s2"] = "sp-j2.sp1",
        ["unibios40"] = "uni-bios_4_0.rom",
        ["unibios33"] = "uni-bios_3_3.rom",
        ["unibios20"] = "uni-bios_2_0.rom",
        ["unibios13"] = "uni-bios_1_3.rom",
        ["unibios11"] = "uni-bios_1_1.rom",
        ["unibios10"] = "uni-bios_1_0.rom",
        ["debug"] = "neodebug.rom",
        ["asia-aes"] = "neo-epo.bin"
    };
    internal static IReadOnlyList<FirmwareCategory> Roles => [FirmwareCategory.SystemBios];

    private static bool IsCompatible(ZipArchive archive) => BiosFiles.Values.Any(file => archive.GetEntry(file)?.Length == BiosRomBytes) && archive.GetEntry(SoundRom)?.Length == BiosRomBytes && archive.GetEntry(FixedRom)?.Length == BiosRomBytes && archive.GetEntry(ZoomRom)?.Length == ZoomRomBytes;
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
            if (model is MachineModel.NeoGeoMvs or MachineModel.NeoGeoAes && IsCompatible(archive) && (model != MachineModel.NeoGeoAes || archive.GetEntry(BiosFiles[RuntimeOptionFunctions.AesBios])?.Length == BiosRomBytes))
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
        if (configuration.Model is not (MachineModel.NeoGeoMvs or MachineModel.NeoGeoAes))
            throw new ArgumentException(nameof(configuration));
        var destination = Path.Combine(systemDirectory, RuntimeOptionConstants.SystemArchive);
        FirmwareCopy.CopyVerified(configuration, FirmwareCategory.SystemBios, destination);
        using var archive = ZipFile.OpenRead(destination);
        var selected = RuntimeOptionFunctions.Prepare(configuration)[RuntimeOptionConstants.NeogeoBios];
        if (!IsCompatible(archive) || archive.GetEntry(BiosFiles[selected])?.Length != BiosRomBytes)
            throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareFileUnreadable);
    }
}