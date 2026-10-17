using GWGUI.Emulation.Snk.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Snk.Emulators.Common.Dictionaries;
using System.IO;
using System.IO.Compression;

namespace GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions;
internal static class FirmwareRuntimeFunctions
{
    private const int CartridgeBiosBytes = 131072;
    private const int PocketBiosBytes = 65536;
    private const string ZoomRom = "000-lo.lo";
    private const string FixedRom = "sfix.sfix";
    private const string SoundRom = "sm1.sm1";
    private const string JapanAesRom = "neo-po.bin";
    private const string AsiaAesRom = "neo-epo.bin";
    private const string UniverseBiosPrefix = "uni-bios_";

    internal static string? SelectModelBios(ZipArchive archive, MachineModel model,
        IReadOnlyList<(string Id, string FileName, int Length)> mvsBios,
        IReadOnlyList<(string Id, string FileName, int Length)> aesBios,
        IReadOnlyList<(string Id, string FileName, int Length)> cdBios,
        IReadOnlyList<(string Id, string FileName, int Length)> cdzBios)
    {
        if (model is MachineModel.NeoGeoPocket or MachineModel.NeoGeoPocketColor) return archive.GetEntry(model == MachineModel.NeoGeoPocket ? "ngp_bios.ngp" : "ngpcbios.rom")?.Length == PocketBiosBytes ? string.Empty : null;
        if (archive.GetEntry(ZoomRom)?.Length != CartridgeBiosBytes) return null;
        if (model == MachineModel.NeoGeoMvs && (archive.GetEntry(FixedRom)?.Length != CartridgeBiosBytes || archive.GetEntry(SoundRom)?.Length != CartridgeBiosBytes)) return null;
        IEnumerable<(string Id, string FileName, int Length)> alternatives = model switch { MachineModel.NeoGeoAes => aesBios.Concat(mvsBios.Where(bios => bios.Id.StartsWith("unibios", StringComparison.Ordinal))), MachineModel.NeoGeoMvs => mvsBios, MachineModel.NeoGeoCd => cdBios, MachineModel.NeoGeoCdz => cdzBios, _ => [] };
        return alternatives.FirstOrDefault(bios => archive.GetEntry(bios.FileName)?.Length == bios.Length).Id;
    }

    internal static bool ValidCartridgeArchive(ZipArchive archive, MachineModel model, string? mode,
        IReadOnlyDictionary<string, int> biosAlternatives, string aesMode, string mvsMode, string universeMode)
    {
        bool Has(string name, int length) => archive.GetEntry(name)?.Length == length;
        if (!Has(ZoomRom, CartridgeBiosBytes)) return false;
        if (model is not (MachineModel.NeoGeoAes or MachineModel.NeoGeoMvs) || !Has(FixedRom, CartridgeBiosBytes) || !Has(SoundRom, CartridgeBiosBytes)) return false;
        return biosAlternatives.Any(bios => Has(bios.Key, bios.Value) && (mode switch { var selected when selected == aesMode => bios.Key is JapanAesRom or AsiaAesRom, var selected when selected == mvsMode => bios.Key is not (JapanAesRom or AsiaAesRom) && !bios.Key.StartsWith(UniverseBiosPrefix, StringComparison.Ordinal), var selected when selected == universeMode => bios.Key.StartsWith(UniverseBiosPrefix, StringComparison.Ordinal), _ => model != MachineModel.NeoGeoAes || bios.Key is JapanAesRom or AsiaAesRom || bios.Key.StartsWith(UniverseBiosPrefix, StringComparison.Ordinal) }));
    }

    internal static ValueTask<EmulationFirmwareCandidate> InspectArchive(string path, MachineModel model,
        CancellationToken cancellationToken, Func<ZipArchive, MachineModel, bool> accepts)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var absolute = Path.GetFullPath(path);
        var compatibility = EmulationFirmwareCompatibility.Incompatible;
        try
        {
            using var archive = ZipFile.OpenRead(absolute);
            if (accepts(archive, model)) compatibility = EmulationFirmwareCompatibility.PartiallyCompatible;
        }
        catch (InvalidDataException) { }
        catch (IOException) { }
        return ValueTask.FromResult(new EmulationFirmwareCandidate(absolute, absolute, Path.GetFileName(absolute),
            null, compatibility, compatibility == EmulationFirmwareCompatibility.Incompatible
                ? null : FirmwareFunctions.Field(FirmwareCategory.SystemBios)));
    }

    internal static void PrepareArchive(MachineConfiguration configuration, string destination, Func<ZipArchive, bool> accepts)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        CopyVerified(configuration, FirmwareCategory.SystemBios, destination);
        using var archive = ZipFile.OpenRead(destination);
        if (!accepts(archive))
            throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareFileUnreadable);
    }

    internal static string SelectedArchiveBios(MachineConfiguration configuration, Func<ZipArchive, string?> select)
    {
        var firmware = configuration.Firmwares.SingleOrDefault(item => item.Category == FirmwareCategory.SystemBios)
            ?? throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareFileUnreadable);
        using var archive = ZipFile.OpenRead(firmware.Path);
        return select(archive) ?? throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareFileUnreadable);
    }

    internal static void Prepare(MachineConfiguration configuration, string systemDirectory) => ((LibretroMachineFactory)CoreCatalog.CreateAdapter(configuration.Core)).PrepareFirmware(configuration, systemDirectory);
    internal static void CopyVerified(MachineConfiguration configuration, FirmwareCategory role, string target)
    {
        var firmware = configuration.Firmwares.SingleOrDefault(item => item.Category == role);
        if (firmware is null)
        {
            if (FirmwareFunctions.IsRequired(configuration.Model, configuration.Core, role))
                throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareMissing, ErrorMessages.RequiredFirmwareMissing);
            return;
        }

        if (!File.Exists(firmware.Path))
            throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareMissing, ErrorMessages.FirmwareFileMissing);
        var candidate = FirmwareFunctions.ScanFileAsync(firmware.Path, configuration.Model, configuration.Core, CancellationToken.None).AsTask().GetAwaiter().GetResult();
        if (candidate.Compatibility is not (EmulationFirmwareCompatibility.Official or EmulationFirmwareCompatibility.Compatible or EmulationFirmwareCompatibility.PartiallyCompatible) || candidate.DestinationFieldId != FirmwareFunctions.Field(role))
            throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareFileUnreadable);
        var source = Path.GetFullPath(firmware.Path);
        var destination = Path.GetFullPath(target);
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        if (!string.Equals(source, destination, StringComparison.OrdinalIgnoreCase))
            File.Copy(source, destination, overwrite: true);
    }
}
