namespace GWGUI.Emulation.Atari.Functions;

public static class AtariFirmwareFunctions
{
    public static IReadOnlyList<T> Values<T>(params T[] values) => Array.AsReadOnly(values);
    public static IReadOnlyDictionary<string, AtariFirmwareDefinition> Index(
        IEnumerable<AtariFirmwareDefinition> definitions) =>
        definitions.ToDictionary(definition => definition.Id, StringComparer.Ordinal);
    public static bool IsValidFingerprint(AtariFirmwareFingerprint fingerprint) =>
        fingerprint.Algorithm == AtariFirmwareHashAlgorithm.Md5 &&
        fingerprint.Value.Length == AtariFirmwareConstants.Md5HexLength &&
        fingerprint.Value.All(Uri.IsHexDigit);
    public static string TosId(string version) => AtariFirmwareConstants.TosIdPrefix + version;

    public static AtariFirmwareDefinition Tos(string version, IReadOnlyList<AtariMachineModel> models,
        IReadOnlyList<AtariStRegion> regions, params AtariFirmwareFingerprint[] fingerprints) => new(
        TosId(version), AtariFirmwareCategory.Tos, version, AtariFirmwareConstants.TosFileName, null,
        AtariFirmwareProvision.RequiredExternal, AtariFirmwareDistribution.UserSuppliedCopyrighted,
        AtariFirmwareEvidence.HatariCoreInformation, models, regions, Values(fingerprints));

    public static AtariFirmwareDefinition Replaceable(string id, AtariFirmwareCategory category, string fileName,
        string md5, IReadOnlyList<AtariMachineModel> models, IReadOnlyList<AtariStRegion> noRegions,
        params string[] additionalMd5) => new(
        id, category, null, fileName, null, AtariFirmwareProvision.EmbeddedReplaceable,
        AtariFirmwareDistribution.UserSuppliedCopyrighted, AtariFirmwareEvidence.Atari800CoreInformation,
        models, noRegions, Values(new[] { md5 }.Concat(additionalMd5).Select(value =>
            new AtariFirmwareFingerprint(AtariFirmwareHashAlgorithm.Md5, value)).ToArray()));

    public static AtariFirmwareDefinition ReplaceableRevision(string id, AtariFirmwareCategory category, string version,
        string fileName, string md5, IReadOnlyList<AtariMachineModel> models,
        IReadOnlyList<AtariStRegion> noRegions) => new(
        id, category, version, fileName, null, AtariFirmwareProvision.EmbeddedReplaceable,
        AtariFirmwareDistribution.UserSuppliedCopyrighted, AtariFirmwareEvidence.Atari800CoreInformation,
        models, noRegions, Values(new AtariFirmwareFingerprint(AtariFirmwareHashAlgorithm.Md5, md5)));

    public static AtariFirmwareDefinition External(string id, AtariFirmwareCategory category, string fileName,
        string md5, AtariFirmwareProvision provision, AtariMachineModel model, AtariFirmwareEvidence evidence,
        IReadOnlyList<AtariStRegion> noRegions) => new(
        id, category, null, fileName, null, provision, AtariFirmwareDistribution.UserSuppliedCopyrighted,
        evidence, Values(model), noRegions,
        Values(new AtariFirmwareFingerprint(AtariFirmwareHashAlgorithm.Md5, md5)));

    public static AtariFirmwareDefinition ExternalRevision(string id, AtariFirmwareCategory category, string version,
        string fileName, string md5, AtariFirmwareProvision provision, AtariMachineModel model,
        AtariFirmwareEvidence evidence, IReadOnlyList<AtariStRegion> noRegions) => new(
        id, category, version, fileName, null, provision, AtariFirmwareDistribution.UserSuppliedCopyrighted,
        evidence, Values(model), noRegions,
        Values(new AtariFirmwareFingerprint(AtariFirmwareHashAlgorithm.Md5, md5)));

    public static AtariFirmwareDefinition Embedded(string id, AtariFirmwareCategory category,
        AtariFirmwareProvision provision, IReadOnlyList<AtariMachineModel> models,
        IReadOnlyList<AtariStRegion> noRegions,
        IReadOnlyList<AtariFirmwareFingerprint> noFingerprints) => new(
        id, category, null, null, null, provision, AtariFirmwareDistribution.NoExternalFile,
        AtariFirmwareEvidence.VirtualJaguarCoreInformation, models, noRegions, noFingerprints);

    public static AtariFirmwareDefinition JaguarCd(string id, string fileName,
        IReadOnlyList<AtariStRegion> noRegions, IReadOnlyList<AtariFirmwareFingerprint> noFingerprints) => new(
        id, AtariFirmwareCategory.JaguarCdBios, null, fileName, null,
        AtariFirmwareProvision.EmbeddedReplaceable, AtariFirmwareDistribution.UserSuppliedCopyrighted,
        AtariFirmwareEvidence.VirtualJaguarCoreInformation, Values(AtariMachineModel.JaguarCd), noRegions,
        noFingerprints);

    public static AtariFirmwareDefinition None(string id, AtariMachineModel model,
        AtariFirmwareEvidence evidence, IReadOnlyList<AtariStRegion> noRegions,
        IReadOnlyList<AtariFirmwareFingerprint> noFingerprints) => new(
        id, null, null, null, null, AtariFirmwareProvision.NotUsed,
        AtariFirmwareDistribution.NoExternalFile, evidence, Values(model), noRegions, noFingerprints);
}
