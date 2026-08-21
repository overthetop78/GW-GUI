namespace GWGUI.Emulation.Atari;

public sealed record AtariFirmwareDefinition(
    string Id, AtariFirmwareCategory? Category, string? Version, string? ExpectedFileName,
    long? ExpectedSizeBytes, AtariFirmwareProvision Provision, AtariFirmwareDistribution Distribution,
    AtariFirmwareEvidence Evidence, IReadOnlyList<AtariMachineModel> Models,
    IReadOnlyList<AtariStRegion> Regions, IReadOnlyList<AtariFirmwareFingerprint> Fingerprints)
{
    public bool RequiresExternalFile => Provision == AtariFirmwareProvision.RequiredExternal;
    public bool CanBePackaged => Distribution != AtariFirmwareDistribution.UserSuppliedCopyrighted;
}
