using System.Security.Cryptography;

namespace GWGUI.Emulation.Atari.Common.Functions;

public static class FirmwareScanFunctions
{
    private static readonly IReadOnlySet<string> RelevantExtensions = new HashSet<string>(
        FirmwareFunctions.Values(FirmwareConstants.RomExtension, FirmwareConstants.ImageExtension,
            FirmwareConstants.TosExtension, FirmwareConstants.BinaryExtension,
            FirmwareConstants.JaguarExtension), StringComparer.OrdinalIgnoreCase);

    public static string FamilyDirectoryName(MachineFamily family) => family switch
    {
        MachineFamily.St => FirmwareConstants.StFamilyDirectoryName,
        MachineFamily.EightBit => FirmwareConstants.EightBitFamilyDirectoryName,
        MachineFamily.Atari5200 => FirmwareConstants.Atari5200FamilyDirectoryName,
        MachineFamily.Atari2600 => FirmwareConstants.Atari2600FamilyDirectoryName,
        MachineFamily.Atari7800 => FirmwareConstants.Atari7800FamilyDirectoryName,
        MachineFamily.Lynx => FirmwareConstants.LynxFamilyDirectoryName,
        MachineFamily.Jaguar => FirmwareConstants.JaguarFamilyDirectoryName,
        _ => throw new ArgumentOutOfRangeException(nameof(family), family, null)
    };

    public static IReadOnlyList<string> EnsureFamilyDirectories(string firmwareRoot)
    {
        var root = Path.GetFullPath(firmwareRoot);
        Directory.CreateDirectory(root);
        return Enum.GetValues<MachineFamily>().Select(family =>
        {
            var directory = Path.Combine(root, FamilyDirectoryName(family));
            Directory.CreateDirectory(directory);
            return directory;
        }).ToArray();
    }

    public static bool IsRelevantFile(string path) => RelevantExtensions.Contains(Path.GetExtension(path)) ||
        FirmwareCatalog.All.Any(definition => definition.ExpectedFileName is not null &&
            string.Equals(Path.GetFileName(path), definition.ExpectedFileName, StringComparison.OrdinalIgnoreCase));

    public static IReadOnlyList<string> EnumerateCandidates(string firmwareRoot)
    {
        var directories = EnsureFamilyDirectories(firmwareRoot);
        return directories.SelectMany(directory => Directory.EnumerateFiles(directory,
                FirmwareConstants.AllFilesPattern, SearchOption.TopDirectoryOnly))
            .Where(IsRelevantFile)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static async Task<string> ComputeMd5Async(string path, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            FirmwareConstants.FileBufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
        var hash = await MD5.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
        return Convert.ToHexStringLower(hash);
    }

    public static FirmwareDefinition? Identify(string md5) => FirmwareCatalog.All.FirstOrDefault(
        definition => definition.Fingerprints.Any(fingerprint =>
            string.Equals(fingerprint.Value, md5, StringComparison.OrdinalIgnoreCase)));

    public static FirmwareDefinition? Identify(string md5, MachineModel model) =>
        FirmwareCatalog.ForModel(model).FirstOrDefault(definition => definition.Fingerprints.Any(fingerprint =>
            string.Equals(fingerprint.Value, md5, StringComparison.OrdinalIgnoreCase)));

    public static async Task<(FirmwareDefinition Definition, StRegion? Region)?> IdentifyTosAsync(
        string path, CancellationToken cancellationToken)
    {
        var header = await TosHeaderReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (header is null) return null;
        if (header.Variant != TosVariant.Atari)
            return (CreateAlternativeTosDefinition(header), header.Region);
        var definition = FirmwareCatalog.All.FirstOrDefault(candidate =>
            candidate.Category == FirmwareCategory.Tos &&
            string.Equals(candidate.Version, header.Version, StringComparison.Ordinal));
        if (definition is null) return null;
        return (definition, header.Region);
    }

    private static FirmwareDefinition CreateAlternativeTosDefinition(TosHeader header)
    {
        var product = header.Variant == TosVariant.EmuTos ? FirmwareScanFunctionsConstants.EmuTOS : FirmwareScanFunctionsConstants.KAOSTOS;
        var models = header.Variant switch
        {
            TosVariant.KaosTos => FirmwareFunctions.Values(MachineModel.St,
                MachineModel.Stf, MachineModel.Stfm, MachineModel.MegaSt),
            TosVariant.EmuTos when header.ImageSize <= 196_608 => FirmwareFunctions.Values(
                MachineModel.St, MachineModel.Stf, MachineModel.Stfm, MachineModel.MegaSt),
            TosVariant.EmuTos when header.ImageSize <= 262_144 => FirmwareFunctions.Values(
                MachineModel.St, MachineModel.Stf, MachineModel.Stfm, MachineModel.MegaSt,
                MachineModel.Ste, MachineModel.MegaSte),
            _ => FirmwareFunctions.Values(MachineModel.St, MachineModel.Stf,
                MachineModel.Stfm, MachineModel.MegaSt, MachineModel.Ste,
                MachineModel.MegaSte, MachineModel.Tt, MachineModel.Falcon)
        };
        return new FirmwareDefinition(
            $"{product.ToLowerInvariant().Replace(' ', '-')}-{header.Version}", FirmwareCategory.Tos,
            $"{product} {header.Version}", FirmwareConstants.TosFileName, header.ImageSize,
            FirmwareProvision.RequiredExternal,
            header.Variant == TosVariant.EmuTos
                ? FirmwareDistribution.BuiltInOpenReplacement
                : FirmwareDistribution.UserSuppliedCopyrighted,
            FirmwareEvidence.HatariCoreInformation, models,
            FirmwareFunctions.Values(header.Region), FirmwareFunctions.Values<FirmwareFingerprint>());
    }

    public static FirmwareDefinition? IdentifyByExpectedName(string path, MachineModel model) =>
        FirmwareCatalog.ForModel(model).FirstOrDefault(definition => definition.ExpectedFileName is not null &&
            string.Equals(Path.GetFileName(path), definition.ExpectedFileName, StringComparison.OrdinalIgnoreCase));

    public static FirmwareCompatibility Classify(FirmwareDefinition? identified,
        FirmwareDefinition? named, MachineModel model, StRegion? region)
    {
        var candidate = identified ?? named;
        if (candidate is null || !candidate.Models.Contains(model) ||
            candidate.Provision is FirmwareProvision.Embedded or FirmwareProvision.NotUsed)
            return FirmwareCompatibility.Incompatible;
        if (identified is null) return FirmwareCompatibility.PartiallyCompatible;
        var fingerprintRegion = identified.Fingerprints.FirstOrDefault(fingerprint =>
            region is null || fingerprint.Region == region)?.Region;
        return region is not null && identified.Fingerprints.Any(fingerprint => fingerprint.Region is not null)
            && fingerprintRegion != region
            ? FirmwareCompatibility.Incompatible
            : FirmwareCompatibility.Compatible;
    }

    public static async Task<ScannedFirmware> ScanFileAsync(string path, MachineModel model,
        StRegion? region, CancellationToken cancellationToken)
    {
        try
        {
            var file = new FileInfo(path);
            var md5 = await ComputeMd5Async(path, cancellationToken).ConfigureAwait(false);
            var identified = Identify(md5, model);
            var tos = identified is null ? await IdentifyTosAsync(path, cancellationToken).ConfigureAwait(false) : null;
            identified ??= tos?.Definition;
            var named = IdentifyByExpectedName(path, model);
            var detectedRegion = tos?.Region;
            var compatibility = Classify(identified, named, model, region);
            if (compatibility == FirmwareCompatibility.Compatible && region is not null &&
                detectedRegion is not null && detectedRegion != StRegion.Multilingual && detectedRegion != region)
                compatibility = FirmwareCompatibility.Incompatible;
            return new(file.FullName, file.Length, md5,
                identified is null ? FirmwareDetectionStatus.Unknown : FirmwareDetectionStatus.Known,
                identified ?? named, compatibility, false, null);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            return new(Path.GetFullPath(path), null, null, FirmwareDetectionStatus.Unreadable,
                IdentifyByExpectedName(path, model), FirmwareCompatibility.Incompatible, false, error.Message);
        }
    }

    public static FirmwareConfiguration CreateSelection(ScannedFirmware firmware)
    {
        if (firmware.Detection == FirmwareDetectionStatus.Unreadable ||
            firmware.Compatibility == FirmwareCompatibility.Incompatible || firmware.Definition?.Category is null ||
            firmware.Definition.Provision is FirmwareProvision.Embedded or FirmwareProvision.NotUsed)
            throw new InvalidOperationException(ErrorMessages.FirmwareCannotBeSelected);
        return new(firmware.Definition.Category.Value, firmware.Path, firmware.Definition.RequiresExternalFile);
    }
}
