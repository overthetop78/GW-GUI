using System.IO;
using System.IO.Compression;
using GWGUI.Emulation.Snk.Emulators.FbNeo.Constants;
using FirmwareCopy = GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions.FirmwareRuntimeFunctions;

namespace GWGUI.Emulation.Snk.Emulators.FbNeo.Functions;
internal static class FirmwareRuntimeFunctions
{
    private const string NativeDirectory = "fbneo";
    private const string PocketArchive = "ngp.zip";
    private const string PocketBios = "SNK Neo-Geo Pocket BIOS (1998)(SNK)(en-ja).bin";
    private const string PocketColorBios = "SNK Neo-Geo Pocket Color BIOS (1999)(SNK)(en-ja).bin";
    private const string ZoomRom = "000-lo.lo";
    private const string FixedRom = "sfix.sfix";
    private const string SoundRom = "sm1.sm1";
    private const string CdRom = "neocd.bin";
    private const string JapanAesRom = "neo-po.bin";
    private const string AsiaAesRom = "neo-epo.bin";
    private const string UniverseBiosPrefix = "uni-bios_";
    private const int RomBankBytes = 131072;
    private const int CdBiosBytes = 524288;
    private const int PocketBiosBytes = 65536;
    private static readonly IReadOnlyDictionary<string, int> BiosAlternatives = new Dictionary<string, int>(StringComparer.Ordinal)
    {
        ["sp-s3.sp1"] = 131072,
        ["sp-s2.sp1"] = 131072,
        ["sp-s.sp1"] = 131072,
        ["sp-u2.sp1"] = 131072,
        ["sp1-u2"] = 131072,
        ["sp-e.sp1"] = 131072,
        ["sp1-u4.bin"] = 131072,
        ["sp1-u3.bin"] = 131072,
        ["vs-bios.rom"] = 131072,
        ["sp-j2.sp1"] = 131072,
        ["sp1.jipan.1024"] = 131072,
        ["sp-45.sp1"] = 524288,
        ["sp-j3.sp1"] = 524288,
        ["japan-j3.bin"] = 131072,
        ["sp1-j3.bin"] = 131072,
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
            if (Valid(archive, model, null))
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
        var directory = Path.Combine(systemDirectory, NativeDirectory);
        Directory.CreateDirectory(directory);
        var archiveName = configuration.Family switch
        {
            MachineFamily.NeoGeoPocket => PocketArchive,
            MachineFamily.NeoGeoCd => RuntimeOptionConstants.CdArchive,
            _ => RuntimeOptionConstants.SystemArchive
        };
        var destination = Path.Combine(directory, archiveName);
        FirmwareCopy.CopyVerified(configuration, FirmwareCategory.SystemBios, destination);
        using var archive = ZipFile.OpenRead(destination);
        var mode = RuntimeOptionFunctions.Prepare(configuration).GetValueOrDefault(RuntimeOptionConstants.NeogeoMode);
        if (!Valid(archive, configuration.Model, mode))
            throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareFileUnreadable);
    }

    private static bool Valid(
        ZipArchive archive,
        MachineModel model,
        string? mode
    )
    {
        bool Has(string name, int length) => archive.GetEntry(name)?.Length == length;
        if (model is MachineModel.NeoGeoPocket or MachineModel.NeoGeoPocketColor)
            return Has(model == MachineModel.NeoGeoPocket ? PocketBios : PocketColorBios, PocketBiosBytes);
        if (!Has(ZoomRom, RomBankBytes))
            return false;
        if (model == MachineModel.NeoGeoCdz)
            return Has(CdRom, CdBiosBytes);
        if (model is not (MachineModel.NeoGeoAes or MachineModel.NeoGeoMvs) || !Has(FixedRom, RomBankBytes) || !Has(SoundRom, RomBankBytes))
            return false;
        return BiosAlternatives.Any(bios => Has(bios.Key, bios.Value) && (mode switch
        {
            RuntimeOptionConstants.AesEurope => bios.Key == AsiaAesRom,
            RuntimeOptionConstants.AesJapan => bios.Key == JapanAesRom,
            RuntimeOptionConstants.UniverseBios => bios.Key.StartsWith(UniverseBiosPrefix, StringComparison.Ordinal),
            RuntimeOptionConstants.MvsEurope or RuntimeOptionConstants.MvsUsa or RuntimeOptionConstants.MvsJapan => bios.Key is not (JapanAesRom or AsiaAesRom) && !bios.Key.StartsWith(UniverseBiosPrefix, StringComparison.Ordinal),
            _ => model != MachineModel.NeoGeoAes || bios.Key is JapanAesRom or AsiaAesRom || bios.Key.StartsWith(UniverseBiosPrefix, StringComparison.Ordinal)
        }));
    }
}