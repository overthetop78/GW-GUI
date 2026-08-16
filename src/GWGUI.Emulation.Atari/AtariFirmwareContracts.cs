namespace GWGUI.Emulation.Atari;

public enum AtariFirmwareProvision { RequiredExternal, OptionalExternal, Embedded, EmbeddedReplaceable, NotUsed }
public enum AtariFirmwareDistribution { UserSuppliedCopyrighted, BuiltInOpenReplacement, NoExternalFile }
public enum AtariFirmwareEvidence
{
    HatariCoreInformation, Atari800CoreInformation, StellaCoreInformation, ProSystemCoreInformation,
    BeetleLynxCoreInformation, VirtualJaguarCoreInformation
}
public enum AtariFirmwareHashAlgorithm { Md5 }
public enum AtariFirmwareDetectionStatus { Known, Unknown, Unreadable }
public enum AtariFirmwareCompatibility { Compatible, PartiallyCompatible, Incompatible }

public sealed record AtariFirmwareFingerprint(
    AtariFirmwareHashAlgorithm Algorithm, string Value, AtariStRegion? Region = null);

public sealed record AtariFirmwareDefinition(
    string Id, AtariFirmwareKind? Kind, string? Version, string? ExpectedFileName,
    long? ExpectedSizeBytes, AtariFirmwareProvision Provision, AtariFirmwareDistribution Distribution,
    AtariFirmwareEvidence Evidence, IReadOnlyList<AtariMachineModel> Models,
    IReadOnlyList<AtariStRegion> Regions, IReadOnlyList<AtariFirmwareFingerprint> Fingerprints)
{
    public bool RequiresExternalFile => Provision == AtariFirmwareProvision.RequiredExternal;
    public bool CanBePackaged => Distribution != AtariFirmwareDistribution.UserSuppliedCopyrighted;
}

public sealed record AtariScannedFirmware(
    string Path, long? SizeBytes, string? Md5, AtariFirmwareDetectionStatus Detection,
    AtariFirmwareDefinition? Definition, AtariFirmwareCompatibility Compatibility,
    bool IsDuplicate, string? ReadError);
