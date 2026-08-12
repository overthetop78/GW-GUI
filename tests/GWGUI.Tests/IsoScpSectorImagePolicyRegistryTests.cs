using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Reconstruction.Atari;
using GWGUI.MediaEngine.Reconstruction.EpsonQx10;
using GWGUI.MediaEngine.Reconstruction.Iso;

namespace GWGUI.Tests;

/// <summary>Vérifie la résolution des politiques de reconstruction ISO.</summary>
public sealed class IsoScpSectorImagePolicyRegistryTests
{
    /// <summary>Vérifie les familles de formats inscrites dans le registre.</summary>
    [Theory]
    [InlineData(DiskImageFormatIds.Atari90, typeof(Atari8BitIsoScpSectorImagePolicy))]
    [InlineData(DiskImageFormatIds.AtariSt720, typeof(AtariStIsoScpSectorImagePolicy))]
    [InlineData(DiskImageFormatIds.AmstradCpc, typeof(AmstradIsoScpSectorImagePolicy))]
    [InlineData(DiskImageFormatIds.Ibm1440, typeof(IbmPcIsoScpSectorImagePolicy))]
    [InlineData(DiskImageFormatIds.Mac1440, typeof(IbmPcIsoScpSectorImagePolicy))]
    [InlineData(DiskImageFormatIds.AcornDfsSingleSided, typeof(BbcIsoScpSectorImagePolicy))]
    [InlineData(DiskImageFormatIds.EpsonQx10_396, typeof(EpsonQx10IsoScpSectorImagePolicy))]
    [InlineData(DiskImageFormatIds.UcsdIbmMfm, typeof(UcsdIsoScpSectorImagePolicy))]
    public void ResolvesRegisteredFormatFamilies(string formatId, Type expectedType) => Assert.IsType(expectedType, IsoScpSectorImagePolicyRegistry.Resolve(formatId));

    /// <summary>Vérifie le repli automatique utilisé sans identifiant.</summary>
    [Fact]
    public void ResolvesAutomaticPolicyWithoutFormatId() => Assert.IsType<AutomaticIsoScpSectorImagePolicy>(IsoScpSectorImagePolicyRegistry.Resolve(null));

    /// <summary>Vérifie le repli générique utilisé pour un identifiant non inscrit.</summary>
    [Fact]
    public void ResolvesGenericPolicyForUnregisteredFormatId() => Assert.IsType<GenericIsoScpSectorImagePolicy>(IsoScpSectorImagePolicyRegistry.Resolve("custom.iso"));
}
