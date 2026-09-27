namespace GWGUI.Emulation.Atari.Common.Machines.Common.Contracts;

public sealed record FirmwareConfiguration(
    FirmwareCategory Category,
    string Path,
    bool IsRequired,
    bool IsOriginalFirmware = true);

public sealed record FirmwareDefinition(
    string Id, FirmwareCategory? Category, string? Version, string? ExpectedFileName,
    long? ExpectedSizeBytes, FirmwareProvision Provision, FirmwareDistribution Distribution,
    FirmwareEvidence Evidence, IReadOnlyList<MachineModel> Models,
    IReadOnlyList<StRegion> Regions, IReadOnlyList<FirmwareFingerprint> Fingerprints)
{
    public bool RequiresExternalFile => Provision == FirmwareProvision.RequiredExternal;
    public bool CanBePackaged => Distribution != FirmwareDistribution.UserSuppliedCopyrighted;
}

public sealed record FirmwareFingerprint(
    FirmwareHashAlgorithm Algorithm, string Value, StRegion? Region = null);

public sealed record ScannedFirmware(
    string Path, long? SizeBytes, string? Md5, FirmwareDetectionStatus Detection,
    FirmwareDefinition? Definition, FirmwareCompatibility Compatibility,
    bool IsDuplicate, string? ReadError);
