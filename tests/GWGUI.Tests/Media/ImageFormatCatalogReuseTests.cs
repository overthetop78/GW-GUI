using System.Globalization;
using GWGUI.MediaEngine.Functions;
using GWGUI.MediaEngine.Images.Formats;

namespace GWGUI.Tests.Media;

public sealed class ImageFormatCatalogReuseTests
{
    [Theory]
    [InlineData("raw.scp", ".scp")]
    [InlineData("raw.hfe", ".hfe")]
    public void RawContainersRemainReadableWithKnownCapabilities(string id, string extension)
    {
        var capabilities = new GwFormatCapabilities(new HashSet<string>([id], StringComparer.OrdinalIgnoreCase),
            new HashSet<string>([extension], StringComparer.OrdinalIgnoreCase));
        var catalog = new CapabilityAwareImageFormatCatalog(new BuiltInImageFormatCatalog(), capabilities);
        Assert.True(catalog.Formats.Single(format => format.Id == id).SupportsPhysicalRead);
    }

    [Theory]
    [InlineData(".DSK", ".dsk")]
    [InlineData("DSK", ".dsk")]
    [InlineData("", ".")]
    [InlineData(" ", ". ")]
    [InlineData(" .DSK ", ". .dsk ")]
    [InlineData(".", ".")]
    public void RawNormalizationKeepsTheOriginalEmptyAndWhitespacePolicy(string value, string expected)
        => Assert.Equal(expected, MediaFileExtensionFunctions.NormalizeCore(value));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t")]
    public void PublicNormalizationKeepsItsValidation(string? value)
        => Assert.ThrowsAny<ArgumentException>(() => MediaFileExtensionFunctions.Normalize(value!));

    [Fact]
    public void RawNormalizationKeepsItsNullFailure()
        => Assert.Throws<NullReferenceException>(() => MediaFileExtensionFunctions.NormalizeCore(null!));

    [Fact]
    public void NormalizationUsesInvariantCase()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            Assert.Equal(".img", MediaFileExtensionFunctions.NormalizeCore("IMG"));
            Assert.Equal(".img", MediaFileExtensionFunctions.Normalize("IMG"));
        }
        finally { CultureInfo.CurrentCulture = original; }
    }

    [Fact]
    public void UnknownCapabilitiesKeepOrderAndTheSourceSetComparers()
    {
        var curated = Curated();
        var catalog = new CapabilityAwareImageFormatCatalog(curated, GwFormatCapabilities.Unknown);
        Assert.Same(curated.Formats, catalog.Formats);
        Assert.Equal(new[] { "first", "last" }, catalog.GetCompatibleOutputs("BIN").Select(format => format.Id));
    }

    [Fact]
    public void KnownCapabilitiesUseTheirEnrichedFormats()
    {
        var catalog = new CapabilityAwareImageFormatCatalog(Curated(),
            new GwFormatCapabilities(new HashSet<string> { "custom.disk" },
                new HashSet<string>(new[] { ".IMG", ".BIN" }, StringComparer.OrdinalIgnoreCase)));
        Assert.Equal(new[] { "first", "last", "custom.disk" }, catalog.GetCompatibleOutputs(".BIN").Select(format => format.Id));
        Assert.Contains(catalog.GetCompatibleOutputs("img"), format => format.Id == "custom.disk");
    }

    [Fact]
    public void RuntimeSelectionUsesReplacedAndAddedFormats()
    {
        var catalog = new RuntimeImageFormatCatalog(Curated(),
            [Format("first", new HashSet<string> { ".rom" }), Format("added", new HashSet<string> { ".rom" })]);
        Assert.Equal(new[] { "first", "added" }, catalog.GetCompatibleOutputs("ROM").Select(format => format.Id));
        Assert.Equal(new[] { "last" }, catalog.GetCompatibleOutputs("bin").Select(format => format.Id));
    }

    [Fact]
    public void BuiltInCatalogKeepsItsPublicSelection()
    {
        var catalog = new BuiltInImageFormatCatalog(key => key);
        Assert.Contains(catalog.GetCompatibleOutputs("SCP"), format => format.Id == "amiga.amigados");
        Assert.Empty(catalog.GetCompatibleOutputs(""));
    }

    [Fact]
    public void ResultsRemainIndependentOfOtherCallsAndTheCatalog()
    {
        var curated = Curated();
        var catalog = new CapabilityAwareImageFormatCatalog(curated, GwFormatCapabilities.Unknown);
        var first = Assert.IsType<DiskFormat[]>(catalog.GetCompatibleOutputs("bin"));
        first[0] = Format("changed", null);
        Assert.Equal(new[] { "first", "last" }, catalog.GetCompatibleOutputs("bin").Select(format => format.Id));
        Assert.Equal("first", curated.Formats[0].Id);
    }

    [Theory]
    [InlineData("builtin")]
    [InlineData("capability")]
    [InlineData("runtime")]
    public void CatalogsRetainTheRawNullPolicy(string kind)
    {
        IImageFormatCatalog catalog = kind switch
        {
            "builtin" => new BuiltInImageFormatCatalog(key => key),
            "capability" => new CapabilityAwareImageFormatCatalog(Curated(), GwFormatCapabilities.Unknown),
            _ => new RuntimeImageFormatCatalog(Curated(), [])
        };
        Assert.Throws<NullReferenceException>(() => catalog.GetCompatibleOutputs(null!));
    }

    private static Catalog Curated() => new(
        [Format("first", new HashSet<string>(new[] { ".bin" }, StringComparer.Ordinal)),
         Format("case-only", new HashSet<string>(new[] { ".BIN" }, StringComparer.Ordinal)),
         Format("last", new HashSet<string>(new[] { ".BIN" }, StringComparer.OrdinalIgnoreCase)),
         Format("without-sources", null)]);

    private static DiskFormat Format(string id, IReadOnlySet<string>? sources)
        => new(id, "test", id, [], CompatibleSourceExtensions: sources, SupportsPhysicalRead: false, SupportsPhysicalWrite: false);

    private sealed class Catalog(IReadOnlyList<DiskFormat> formats) : IImageFormatCatalog
    {
        public IReadOnlyList<DiskFormat> Formats { get; } = formats;
        public IReadOnlyList<DiskFormat> GetCompatibleOutputs(string sourceExtension)
            => throw new InvalidOperationException("The receiving catalog must select its own formats.");
    }
}
