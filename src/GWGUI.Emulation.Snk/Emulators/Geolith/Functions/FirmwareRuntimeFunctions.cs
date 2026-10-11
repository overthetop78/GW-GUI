using System.IO;
using System.IO.Compression;
using GWGUI.Emulation.Snk.Emulators.Geolith.Constants;
using FirmwareCopy = GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions.FirmwareRuntimeFunctions;

namespace GWGUI.Emulation.Snk.Emulators.Geolith.Functions;

internal static class FirmwareRuntimeFunctions
{
    private const string AesArchive = "aes.zip";
    private const string MvsArchive = "neogeo.zip";
    private const string CdArchive = "neocd.zip";
    private const string CdzArchive = "neocdz.zip";
    private const string TrackballArchive = "irrmaze.zip";
    private const string ZoomRom = "000-lo.lo";
    private const string FixedRom = "sfix.sfix";
    private const string SoundRom = "sm1.sm1";
    private const string TrackballRom = "236-bios.sp1";
    private const string UniverseRom = "uni-bios_4_0.rom";
    private const string UniverseCdRom = "uni-bioscd33.rom";
    private const string FrontRom = "front-sp1.bin";
    private const string TopRom = "top-sp1.bin";
    private const string CdzRom = "neocd.bin";
    private const int RomBankBytes = 131072;
    private const int CdBiosBytes = 524288;
    private static readonly IReadOnlyDictionary<string, string> MvsRegionRoms = new Dictionary<string, string>(StringComparer.Ordinal) { [RuntimeOptionConstants.UnitedStates] = "sp-u2.sp1", [RuntimeOptionConstants.Japan] = "japan-j3.bin", [RuntimeOptionConstants.Asia] = "sp-45.sp1", [RuntimeOptionConstants.Europe] = "sp-s2.sp1" };
    private static readonly IReadOnlyDictionary<string, string> AesRegionRoms = new Dictionary<string, string>(StringComparer.Ordinal) { [RuntimeOptionConstants.UnitedStates] = "neo-epo.bin", [RuntimeOptionConstants.Japan] = "neo-po.bin", [RuntimeOptionConstants.Asia] = "neo-epo.bin", [RuntimeOptionConstants.Europe] = "neo-epo.bin" };

    internal static IReadOnlyList<FirmwareCategory> Roles(MachineModel model) => model switch { MachineModel.NeoGeoCd or MachineModel.NeoGeoMvs => [FirmwareCategory.SystemBios, FirmwareCategory.AuxiliaryBios], MachineModel.NeoGeoAes or MachineModel.NeoGeoCdz => [FirmwareCategory.SystemBios], _ => [] };
    internal static bool Required(MachineModel model, FirmwareCategory role) => role == FirmwareCategory.SystemBios && Roles(model).Contains(role) || model == MachineModel.NeoGeoCd && role == FirmwareCategory.AuxiliaryBios;

    internal static ValueTask<EmulationFirmwareCandidate> Inspect(string path, MachineModel model, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var absolute = Path.GetFullPath(path);
        FirmwareCategory? role = null;
        var compatibility = EmulationFirmwareCompatibility.Incompatible;
        try
        {
            using var archive = ZipFile.OpenRead(absolute);
            bool Has(string name, int length) => archive.GetEntry(name)?.Length == length;
            var zoom = Has(ZoomRom, RomBankBytes);
            var mvsData = zoom && Has(FixedRom, RomBankBytes) && Has(SoundRom, RomBankBytes);
            var system = model switch { MachineModel.NeoGeoAes => zoom && AesRegionRoms.Values.Any(name => Has(name, RomBankBytes)) || mvsData && Has(UniverseRom, RomBankBytes), MachineModel.NeoGeoMvs => mvsData && (MvsRegionRoms.Values.Any(name => Has(name, RomBankBytes)) || Has(UniverseRom, RomBankBytes)), MachineModel.NeoGeoCd => Has(FrontRom, CdBiosBytes) || Has(TopRom, CdBiosBytes), MachineModel.NeoGeoCdz => zoom && (Has(CdzRom, CdBiosBytes) || Has(UniverseCdRom, CdBiosBytes)), _ => false };
            if (system) { role = FirmwareCategory.SystemBios; compatibility = EmulationFirmwareCompatibility.PartiallyCompatible; }
            else if (model == MachineModel.NeoGeoCd && zoom || model == MachineModel.NeoGeoMvs && Has(TrackballRom, RomBankBytes)) { role = FirmwareCategory.AuxiliaryBios; compatibility = EmulationFirmwareCompatibility.Compatible; }
        }
        catch (InvalidDataException) { }
        catch (IOException) { }
        return ValueTask.FromResult(new EmulationFirmwareCandidate(absolute, absolute, Path.GetFileName(absolute), null, compatibility, role is FirmwareCategory selected ? FirmwareFunctions.Field(selected) : null));
    }

    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        var options = RuntimeOptionFunctions.Prepare(configuration);
        var cd = ConfigurationFunctions.Family(configuration.Model) == MachineFamily.NeoGeoCd;
        var nativeSystem = cd ? options[RuntimeOptionConstants.CdSystemType] : options[RuntimeOptionConstants.SystemType];
        var bios = nativeSystem switch { RuntimeOptionConstants.Aes => AesRegionRoms[options[RuntimeOptionConstants.Region]], RuntimeOptionConstants.Mvs => MvsRegionRoms[options[RuntimeOptionConstants.Region]], RuntimeOptionConstants.UniverseBios => UniverseRom, RuntimeOptionConstants.CdFront => FrontRom, RuntimeOptionConstants.CdTop => TopRom, RuntimeOptionConstants.Cdz => CdzRom, RuntimeOptionConstants.CdzUniverseBios => UniverseCdRom, _ => throw new ArgumentException(nameof(configuration)) };
        var mainName = cd ? configuration.Model == MachineModel.NeoGeoCd ? CdArchive : CdzArchive : nativeSystem == RuntimeOptionConstants.Aes ? AesArchive : MvsArchive;
        var mainPath = Path.Combine(systemDirectory, mainName);
        FirmwareCopy.CopyVerified(configuration, FirmwareCategory.SystemBios, mainPath);
        using (var archive = ZipFile.OpenRead(mainPath))
        {
            Require(archive, bios, cd ? CdBiosBytes : RomBankBytes);
            if (configuration.Model != MachineModel.NeoGeoCd) Require(archive, ZoomRom, RomBankBytes);
            if (nativeSystem is RuntimeOptionConstants.Mvs or RuntimeOptionConstants.UniverseBios) { Require(archive, FixedRom, RomBankBytes); Require(archive, SoundRom, RomBankBytes); }
        }
        if (configuration.Model == MachineModel.NeoGeoCd)
        {
            var auxiliaryPath = Path.Combine(systemDirectory, CdzArchive);
            FirmwareCopy.CopyVerified(configuration, FirmwareCategory.AuxiliaryBios, auxiliaryPath);
            using var archive = ZipFile.OpenRead(auxiliaryPath);
            Require(archive, ZoomRom, RomBankBytes);
        }
        else if (configuration.Model == MachineModel.NeoGeoMvs && configuration.Firmwares.Any(item => item.Category == FirmwareCategory.AuxiliaryBios)) FirmwareCopy.CopyVerified(configuration, FirmwareCategory.AuxiliaryBios, Path.Combine(systemDirectory, TrackballArchive));
    }

    private static void Require(ZipArchive archive, string name, long length)
    {
        if (archive.GetEntry(name)?.Length != length) throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareFileUnreadable);
    }
}
