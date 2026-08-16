using GWGUI.Emulation.Atari;

namespace GWGUI.Tests;

public sealed class AtariFirmwareCatalogTests
{
    [Fact]
    public void DefinitionsHaveUniqueIdentifiersAndValidPublicFingerprints()
    {
        Assert.Equal(AtariFirmwareCatalog.All.Count,
            AtariFirmwareCatalog.All.Select(definition => definition.Id).Distinct(StringComparer.Ordinal).Count());
        Assert.All(AtariFirmwareCatalog.All.SelectMany(definition => definition.Fingerprints),
            fingerprint => Assert.True(AtariFirmwareFunctions.IsValidFingerprint(fingerprint)));
    }

    [Fact]
    public void TosVersionsExactlyFollowEachStModelCatalogue()
    {
        foreach (var model in AtariStModelCatalog.All)
        {
            var versions = AtariFirmwareCatalog.ForModel(model.Model)
                .Where(definition => definition.Kind == AtariFirmwareKind.Tos)
                .Select(definition => definition.Version)
                .ToArray();

            Assert.Equal(model.TosVersions.Order(), versions.Order());
            Assert.All(AtariFirmwareCatalog.ForModel(model.Model)
                    .Where(definition => definition.Kind == AtariFirmwareKind.Tos),
                definition =>
                {
                    Assert.Equal(AtariFirmwareProvision.RequiredExternal, definition.Provision);
                    Assert.Equal(Enum.GetValues<AtariStRegion>().Order(), definition.Regions.Order());
                    Assert.Equal(AtariFirmwareConstants.TosFileName, definition.ExpectedFileName);
                });
        }
    }

    [Fact]
    public void OnlyVerifiedUnitedStatesTos102FingerprintIsPublished()
    {
        var tosFingerprints = AtariFirmwareCatalog.All
            .Where(definition => definition.Kind == AtariFirmwareKind.Tos)
            .SelectMany(definition => definition.Fingerprints)
            .ToArray();

        var fingerprint = Assert.Single(tosFingerprints);
        Assert.Equal(AtariFirmwareConstants.Tos102UnitedStatesMd5, fingerprint.Value);
        Assert.Equal(AtariStRegion.UnitedStates, fingerprint.Region);
    }

    [Fact]
    public void Atari800RealRomsAreOptionalReplacementsForEmbeddedOpenFirmware()
    {
        var definitions = AtariFirmwareCatalog.All
            .Where(definition => definition.Evidence == AtariFirmwareEvidence.Atari800CoreInformation)
            .ToArray();

        Assert.Equal(AtariFirmwareConstants.Atari800ExternalFirmwareCount, definitions.Length);
        Assert.All(definitions, definition =>
        {
            Assert.Equal(AtariFirmwareProvision.EmbeddedReplaceable, definition.Provision);
            Assert.Equal(AtariFirmwareDistribution.UserSuppliedCopyrighted, definition.Distribution);
            Assert.NotNull(definition.ExpectedFileName);
            Assert.Single(definition.Fingerprints);
        });
    }

    [Fact]
    public void ConsoleFirmwarePoliciesMatchOfficialCoreInformation()
    {
        Assert.Equal(AtariFirmwareProvision.NotUsed,
            AtariFirmwareCatalog.Get(AtariFirmwareConstants.Atari2600NoBiosId).Provision);
        Assert.Equal(AtariFirmwareProvision.OptionalExternal,
            AtariFirmwareCatalog.Get(AtariFirmwareConstants.Atari7800Id).Provision);
        Assert.Equal(AtariFirmwareProvision.RequiredExternal,
            AtariFirmwareCatalog.Get(AtariFirmwareConstants.LynxBootId).Provision);
        Assert.Equal(AtariFirmwareProvision.Embedded,
            AtariFirmwareCatalog.Get(AtariFirmwareConstants.JaguarBootId).Provision);
        Assert.Equal(AtariFirmwareProvision.EmbeddedReplaceable,
            AtariFirmwareCatalog.Get(AtariFirmwareConstants.JaguarCdRetailId).Provision);
        Assert.Equal(AtariFirmwareProvision.EmbeddedReplaceable,
            AtariFirmwareCatalog.Get(AtariFirmwareConstants.JaguarCdDeveloperId).Provision);
        Assert.Equal(AtariFirmwareProvision.NotUsed,
            AtariFirmwareCatalog.Get(AtariFirmwareConstants.JaguarCdDriveFirmwareId).Provision);
    }

    [Fact]
    public void ProtectedExternalFirmwareCanNeverBePackaged()
    {
        var protectedDefinitions = AtariFirmwareCatalog.All
            .Where(definition => definition.Distribution == AtariFirmwareDistribution.UserSuppliedCopyrighted)
            .ToArray();

        Assert.NotEmpty(protectedDefinitions);
        Assert.All(protectedDefinitions, definition => Assert.False(definition.CanBePackaged));
        Assert.All(AtariFirmwareCatalog.All.Where(definition => definition.CanBePackaged), definition =>
            Assert.Null(definition.ExpectedFileName));
    }

    [Fact]
    public void SizesRemainUnknownWhenOfficialEvidenceDoesNotProvideThem()
    {
        Assert.All(AtariFirmwareCatalog.All, definition => Assert.Null(definition.ExpectedSizeBytes));
    }
}
