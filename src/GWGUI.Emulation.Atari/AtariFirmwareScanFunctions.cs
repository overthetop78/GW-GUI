using System.Security.Cryptography;

namespace GWGUI.Emulation.Atari;

public static class AtariFirmwareScanFunctions
{
    private static readonly IReadOnlySet<string> RelevantExtensions = new HashSet<string>(
        AtariFirmwareFunctions.Values(AtariFirmwareConstants.RomExtension, AtariFirmwareConstants.ImageExtension,
            AtariFirmwareConstants.TosExtension, AtariFirmwareConstants.BinaryExtension,
            AtariFirmwareConstants.JaguarExtension), StringComparer.OrdinalIgnoreCase);

    public static string FamilyDirectoryName(AtariMachineFamily family) => family switch
    {
        AtariMachineFamily.St => AtariFirmwareConstants.StFamilyDirectoryName,
        AtariMachineFamily.EightBit => AtariFirmwareConstants.EightBitFamilyDirectoryName,
        AtariMachineFamily.Atari5200 => AtariFirmwareConstants.Atari5200FamilyDirectoryName,
        AtariMachineFamily.Atari2600 => AtariFirmwareConstants.Atari2600FamilyDirectoryName,
        AtariMachineFamily.Atari7800 => AtariFirmwareConstants.Atari7800FamilyDirectoryName,
        AtariMachineFamily.Lynx => AtariFirmwareConstants.LynxFamilyDirectoryName,
        AtariMachineFamily.Jaguar => AtariFirmwareConstants.JaguarFamilyDirectoryName,
        _ => throw new ArgumentOutOfRangeException(nameof(family), family, null)
    };

    public static IReadOnlyList<string> EnsureFamilyDirectories(string firmwareRoot)
    {
        var root = Path.GetFullPath(firmwareRoot);
        Directory.CreateDirectory(root);
        return Enum.GetValues<AtariMachineFamily>().Select(family =>
        {
            var directory = Path.Combine(root, FamilyDirectoryName(family));
            Directory.CreateDirectory(directory);
            return directory;
        }).ToArray();
    }

    public static bool IsRelevantFile(string path) => RelevantExtensions.Contains(Path.GetExtension(path)) ||
        AtariFirmwareCatalog.All.Any(definition => definition.ExpectedFileName is not null &&
            string.Equals(Path.GetFileName(path), definition.ExpectedFileName, StringComparison.OrdinalIgnoreCase));

    public static IReadOnlyList<string> EnumerateCandidates(string firmwareRoot)
    {
        var directories = EnsureFamilyDirectories(firmwareRoot);
        return directories.SelectMany(directory => Directory.EnumerateFiles(directory,
                AtariFirmwareConstants.AllFilesPattern, SearchOption.TopDirectoryOnly))
            .Where(IsRelevantFile)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static async Task<string> ComputeMd5Async(string path, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            AtariFirmwareConstants.FileBufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
        var hash = await MD5.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
        return Convert.ToHexStringLower(hash);
    }

    public static AtariFirmwareDefinition? Identify(string md5) => AtariFirmwareCatalog.All.FirstOrDefault(
        definition => definition.Fingerprints.Any(fingerprint =>
            string.Equals(fingerprint.Value, md5, StringComparison.OrdinalIgnoreCase)));

    public static AtariFirmwareDefinition? Identify(string md5, AtariMachineModel model) =>
        AtariFirmwareCatalog.ForModel(model).FirstOrDefault(definition => definition.Fingerprints.Any(fingerprint =>
            string.Equals(fingerprint.Value, md5, StringComparison.OrdinalIgnoreCase)));

    public static async Task<(AtariFirmwareDefinition Definition, AtariStRegion? Region)?> IdentifyTosAsync(
        string path, CancellationToken cancellationToken)
    {
        var header = await AtariTosHeaderReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (header is null) return null;
        if (header.Variant != AtariTosVariant.Atari)
            return (CreateAlternativeTosDefinition(header), header.Region);
        var definition = AtariFirmwareCatalog.All.FirstOrDefault(candidate =>
            candidate.Category == AtariFirmwareCategory.Tos &&
            string.Equals(candidate.Version, header.Version, StringComparison.Ordinal));
        if (definition is null) return null;
        return (definition, header.Region);
    }

    private static AtariFirmwareDefinition CreateAlternativeTosDefinition(AtariTosHeader header)
    {
        var product = header.Variant == AtariTosVariant.EmuTos ? "EmuTOS" : "KAOS TOS";
        var models = header.Variant switch
        {
            AtariTosVariant.KaosTos => AtariFirmwareFunctions.Values(AtariMachineModel.St,
                AtariMachineModel.Stf, AtariMachineModel.Stfm, AtariMachineModel.MegaSt),
            AtariTosVariant.EmuTos when header.ImageSize <= 196_608 => AtariFirmwareFunctions.Values(
                AtariMachineModel.St, AtariMachineModel.Stf, AtariMachineModel.Stfm, AtariMachineModel.MegaSt),
            AtariTosVariant.EmuTos when header.ImageSize <= 262_144 => AtariFirmwareFunctions.Values(
                AtariMachineModel.St, AtariMachineModel.Stf, AtariMachineModel.Stfm, AtariMachineModel.MegaSt,
                AtariMachineModel.Ste, AtariMachineModel.MegaSte),
            _ => AtariFirmwareFunctions.Values(AtariMachineModel.St, AtariMachineModel.Stf,
                AtariMachineModel.Stfm, AtariMachineModel.MegaSt, AtariMachineModel.Ste,
                AtariMachineModel.MegaSte, AtariMachineModel.Tt, AtariMachineModel.Falcon)
        };
        return new AtariFirmwareDefinition(
            $"{product.ToLowerInvariant().Replace(' ', '-')}-{header.Version}", AtariFirmwareCategory.Tos,
            $"{product} {header.Version}", AtariFirmwareConstants.TosFileName, header.ImageSize,
            AtariFirmwareProvision.RequiredExternal,
            header.Variant == AtariTosVariant.EmuTos
                ? AtariFirmwareDistribution.BuiltInOpenReplacement
                : AtariFirmwareDistribution.UserSuppliedCopyrighted,
            AtariFirmwareEvidence.HatariCoreInformation, models,
            AtariFirmwareFunctions.Values(header.Region), AtariFirmwareFunctions.Values<AtariFirmwareFingerprint>());
    }

    public static AtariFirmwareDefinition? IdentifyByExpectedName(string path, AtariMachineModel model) =>
        AtariFirmwareCatalog.ForModel(model).FirstOrDefault(definition => definition.ExpectedFileName is not null &&
            string.Equals(Path.GetFileName(path), definition.ExpectedFileName, StringComparison.OrdinalIgnoreCase));

    public static AtariFirmwareCompatibility Classify(AtariFirmwareDefinition? identified,
        AtariFirmwareDefinition? named, AtariMachineModel model, AtariStRegion? region)
    {
        var candidate = identified ?? named;
        if (candidate is null || !candidate.Models.Contains(model) ||
            candidate.Provision is AtariFirmwareProvision.Embedded or AtariFirmwareProvision.NotUsed)
            return AtariFirmwareCompatibility.Incompatible;
        if (identified is null) return AtariFirmwareCompatibility.PartiallyCompatible;
        var fingerprintRegion = identified.Fingerprints.FirstOrDefault(fingerprint =>
            region is null || fingerprint.Region == region)?.Region;
        return region is not null && identified.Fingerprints.Any(fingerprint => fingerprint.Region is not null)
            && fingerprintRegion != region
            ? AtariFirmwareCompatibility.Incompatible
            : AtariFirmwareCompatibility.Compatible;
    }

    public static async Task<AtariScannedFirmware> ScanFileAsync(string path, AtariMachineModel model,
        AtariStRegion? region, CancellationToken cancellationToken)
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
            if (compatibility == AtariFirmwareCompatibility.Compatible && region is not null &&
                detectedRegion is not null && detectedRegion != AtariStRegion.Multilingual && detectedRegion != region)
                compatibility = AtariFirmwareCompatibility.Incompatible;
            return new(file.FullName, file.Length, md5,
                identified is null ? AtariFirmwareDetectionStatus.Unknown : AtariFirmwareDetectionStatus.Known,
                identified ?? named, compatibility, false, null);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            return new(Path.GetFullPath(path), null, null, AtariFirmwareDetectionStatus.Unreadable,
                IdentifyByExpectedName(path, model), AtariFirmwareCompatibility.Incompatible, false, error.Message);
        }
    }

    public static AtariFirmwareConfiguration CreateSelection(AtariScannedFirmware firmware)
    {
        if (firmware.Detection == AtariFirmwareDetectionStatus.Unreadable ||
            firmware.Compatibility == AtariFirmwareCompatibility.Incompatible || firmware.Definition?.Category is null ||
            firmware.Definition.Provision is AtariFirmwareProvision.Embedded or AtariFirmwareProvision.NotUsed)
            throw new InvalidOperationException(AtariErrorMessages.FirmwareCannotBeSelected);
        return new(firmware.Definition.Category.Value, firmware.Path, firmware.Definition.RequiresExternalFile);
    }
}
