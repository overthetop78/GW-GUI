using System.Globalization;

namespace GWGUI.Emulation.Atari.Common.Machines.Common.Functions;

public static class FirmwareFunctions
{
    public static IReadOnlyList<T> Values<T>(params T[] values) => Array.AsReadOnly(values);
    public static IReadOnlyDictionary<string, FirmwareDefinition> Index(
        IEnumerable<FirmwareDefinition> definitions) =>
        definitions.ToDictionary(definition => definition.Id, StringComparer.Ordinal);
    public static bool IsValidFingerprint(FirmwareFingerprint fingerprint) =>
        fingerprint.Algorithm == FirmwareHashAlgorithm.Md5 &&
        fingerprint.Value.Length == FirmwareConstants.Md5HexLength &&
        fingerprint.Value.All(Uri.IsHexDigit);
    public static string TosId(string version) => FirmwareConstants.TosIdPrefix + version;

    public static FirmwareDefinition Tos(string version, IReadOnlyList<MachineModel> models,
        IReadOnlyList<StRegion> regions, params FirmwareFingerprint[] fingerprints) => new(
        TosId(version), FirmwareCategory.Tos, version, FirmwareConstants.TosFileName, null,
        FirmwareProvision.RequiredExternal, FirmwareDistribution.UserSuppliedCopyrighted,
        FirmwareEvidence.HatariCoreInformation, models, regions, Values(fingerprints));

    public static FirmwareDefinition Replaceable(string id, FirmwareCategory category, string fileName,
        string md5, IReadOnlyList<MachineModel> models, IReadOnlyList<StRegion> noRegions,
        params string[] additionalMd5) => new(
        id, category, null, fileName, null, FirmwareProvision.EmbeddedReplaceable,
        FirmwareDistribution.UserSuppliedCopyrighted, FirmwareEvidence.Atari800CoreInformation,
        models, noRegions, Values(new[] { md5 }.Concat(additionalMd5).Select(value =>
            new FirmwareFingerprint(FirmwareHashAlgorithm.Md5, value)).ToArray()));

    public static FirmwareDefinition ReplaceableRevision(string id, FirmwareCategory category, string version,
        string fileName, string md5, IReadOnlyList<MachineModel> models,
        IReadOnlyList<StRegion> noRegions) => new(
        id, category, version, fileName, null, FirmwareProvision.EmbeddedReplaceable,
        FirmwareDistribution.UserSuppliedCopyrighted, FirmwareEvidence.Atari800CoreInformation,
        models, noRegions, Values(new FirmwareFingerprint(FirmwareHashAlgorithm.Md5, md5)));

    public static FirmwareDefinition External(string id, FirmwareCategory category, string fileName,
        string md5, FirmwareProvision provision, MachineModel model, FirmwareEvidence evidence,
        IReadOnlyList<StRegion> noRegions) => new(
        id, category, null, fileName, null, provision, FirmwareDistribution.UserSuppliedCopyrighted,
        evidence, Values(model), noRegions,
        Values(new FirmwareFingerprint(FirmwareHashAlgorithm.Md5, md5)));

    public static FirmwareDefinition ExternalRevision(string id, FirmwareCategory category, string version,
        string fileName, string md5, FirmwareProvision provision, MachineModel model,
        FirmwareEvidence evidence, IReadOnlyList<StRegion> noRegions) => new(
        id, category, version, fileName, null, provision, FirmwareDistribution.UserSuppliedCopyrighted,
        evidence, Values(model), noRegions,
        Values(new FirmwareFingerprint(FirmwareHashAlgorithm.Md5, md5)));

    public static FirmwareDefinition Embedded(string id, FirmwareCategory category,
        FirmwareProvision provision, IReadOnlyList<MachineModel> models,
        IReadOnlyList<StRegion> noRegions,
        IReadOnlyList<FirmwareFingerprint> noFingerprints) => new(
        id, category, null, null, null, provision, FirmwareDistribution.NoExternalFile,
        FirmwareEvidence.VirtualJaguarCoreInformation, models, noRegions, noFingerprints);

    public static FirmwareDefinition JaguarCd(string id, string fileName,
        IReadOnlyList<StRegion> noRegions, IReadOnlyList<FirmwareFingerprint> noFingerprints) => new(
        id, FirmwareCategory.JaguarCdBios, null, fileName, null,
        FirmwareProvision.EmbeddedReplaceable, FirmwareDistribution.UserSuppliedCopyrighted,
        FirmwareEvidence.VirtualJaguarCoreInformation, Values(MachineModel.JaguarCd), noRegions,
        noFingerprints);

    public static FirmwareDefinition None(string id, MachineModel model,
        FirmwareEvidence evidence, IReadOnlyList<StRegion> noRegions,
        IReadOnlyList<FirmwareFingerprint> noFingerprints) => new(
        id, null, null, null, null, FirmwareProvision.NotUsed,
        FirmwareDistribution.NoExternalFile, evidence, Values(model), noRegions, noFingerprints);
}

public static class FirmwareRuntimeFunctions
{
    public static void PrepareSystemDirectory(MachineConfiguration configuration, string systemDirectory)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var absoluteSystemDirectory = Path.GetFullPath(systemDirectory);
        Directory.CreateDirectory(absoluteSystemDirectory);
        ValidateRequiredFirmware(configuration);
        ClearManagedFirmwareFiles(absoluteSystemDirectory);
        foreach (var firmware in configuration.Firmwares)
        {
            var sourcePath = Path.GetFullPath(firmware.Path);
            ValidateReadableFile(firmware.Category, sourcePath);
            var definition = ResolveDefinition(configuration.Model, firmware.Category, sourcePath);
            var targetPath = Path.Combine(absoluteSystemDirectory, definition.ExpectedFileName!);
            File.Copy(sourcePath, targetPath, true);
        }
    }

    public static void ClearManagedFirmwareFiles(string systemDirectory)
    {
        foreach (var fileName in FirmwareCatalog.All.Select(definition => definition.ExpectedFileName)
                     .Where(fileName => fileName is not null).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var path = Path.Combine(systemDirectory, fileName!);
            if (File.Exists(path)) File.Delete(path);
        }
    }

    public static void ValidateRequiredFirmware(MachineConfiguration configuration)
    {
        var configuredCategories = configuration.Firmwares.Select(firmware => firmware.Category).ToHashSet();
        var requiredCategories = FirmwareCatalog.ForModel(configuration.Model)
            .Where(definition => definition.RequiresExternalFile && definition.Category is not null)
            .Select(definition => definition.Category!.Value)
            .Distinct();
        foreach (var category in requiredCategories.Where(category => !configuredCategories.Contains(category)))
            throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture,
                ErrorMessages.RequiredFirmwareMissing, category, configuration.Model));
    }

    public static FirmwareDefinition ResolveDefinition(MachineModel model, FirmwareCategory category,
        string sourcePath)
    {
        var definitions = FirmwareCatalog.ForModel(model)
            .Where(definition => definition.Category == category && definition.ExpectedFileName is not null)
            .ToArray();
        var expectedFileNames = definitions.Select(definition => definition.ExpectedFileName!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (expectedFileNames.Length == FirmwareRuntimeConstants.SingleDefinitionCount)
            return definitions[FirmwareRuntimeConstants.FirstDefinitionIndex];

        var md5 = ComputeMd5(sourcePath);
        var identified = definitions.FirstOrDefault(definition => definition.Fingerprints.Any(fingerprint =>
            string.Equals(fingerprint.Value, md5, StringComparison.OrdinalIgnoreCase)));
        if (identified is not null) return identified;
        var named = definitions.FirstOrDefault(definition => string.Equals(definition.ExpectedFileName,
            Path.GetFileName(sourcePath), StringComparison.OrdinalIgnoreCase));
        return named ?? throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture,
            ErrorMessages.FirmwareIdentityAmbiguous, category, sourcePath));
    }

    public static void ValidateReadableFile(FirmwareCategory category, string sourcePath)
    {
        if (!File.Exists(sourcePath)) throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture,
            ErrorMessages.FirmwareFileMissing, category, sourcePath), sourcePath);
        try
        {
            using var stream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            _ = stream.ReadByte();
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            throw new IOException(string.Format(CultureInfo.InvariantCulture,
                ErrorMessages.FirmwareFileUnreadable, category, sourcePath), error);
        }
    }

    private static string ComputeMd5(string sourcePath)
    {
        using var stream = File.OpenRead(sourcePath);
        return Convert.ToHexStringLower(System.Security.Cryptography.MD5.HashData(stream));
    }
}
