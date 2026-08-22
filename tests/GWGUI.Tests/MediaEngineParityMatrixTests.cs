using GWGUI.App.Services.Conversion;
using GWGUI.App.Services.Parity;
using GWGUI.Domain.Conversion;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Parity;
using GWGUI.Emulation.Common;
using GWGUI.MediaEngine.Exploration.Metadata;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

public sealed class MediaEngineParityMatrixTests
{
    [Fact]
    public void MatrixContainsEveryBuiltInSourceTargetRouteAndEveryRequiredColumn()
    {
        var catalog = new BuiltInImageFormatCatalog();
        var expected = catalog.Formats.Sum(format =>
            (format.CompatibleSourceExtensions?.Count ?? 0) * format.Extensions.Count);

        var matrix = MediaEngineParityCatalog.Create(catalog);

        Assert.Equal(expected, matrix.Rows.Count);
        Assert.All(matrix.Rows, row =>
        {
            Assert.False(string.IsNullOrWhiteSpace(row.FormatId));
            Assert.StartsWith(".", row.SourceContainer);
            Assert.StartsWith(".", row.TargetContainer);
            Assert.False(string.IsNullOrWhiteSpace(row.Geometry));
            Assert.True(row.GwFallbackAvailable);
        });
    }

    [Fact]
    public void OnlyRowsWithReadConversionReopenAndParityEvidenceUseMediaEngine()
    {
        var eligible = new MediaParityRow(
            "format",
            ".source",
            ".target",
            "geometry",
            ParityValidationStatus.Passed,
            ParityValidationStatus.Passed,
            ParityValidationStatus.Passed,
            ParityValidationStatus.Passed,
            ParityValidationStatus.Passed,
            ParityValidationStatus.Passed,
            ParityValidationStatus.NotApplicable,
            ParityValidationStatus.Pending,
            true,
            "evidence");
        var pending = eligible with { Reopen = ParityValidationStatus.Pending };
        var failed = eligible with { MetadataIdentical = ParityValidationStatus.Failed };

        Assert.True(eligible.IsValidatedFor(MediaParityOperation.Conversion));
        Assert.False(eligible.IsValidatedFor(MediaParityOperation.PhysicalWrite));
        Assert.False(pending.IsValidatedFor(MediaParityOperation.Conversion));
        Assert.False(failed.IsValidatedFor(MediaParityOperation.Conversion));
    }

    [Fact]
    public void QualifiedRowsCarryEvidenceButPhysicalWriteRemainsUnqualified()
    {
        var rows = MediaEngineParityCatalog.Matrix.Rows;
        var qualified = rows.Where(row => row.IsValidatedFor(MediaParityOperation.Conversion)).ToArray();

        Assert.NotEmpty(qualified);
        Assert.All(qualified, row => Assert.False(string.IsNullOrWhiteSpace(row.EvidenceId)));
        Assert.DoesNotContain(rows, row => row.IsValidatedFor(MediaParityOperation.PhysicalWrite));
    }

    [Theory]
    [InlineData("capture.scp", "raw.scp", ".scp", true)]
    [InlineData("capture.hfe", "raw.scp", ".scp", true)]
    [InlineData("capture.scp", "amiga.amigados", ".adf", true)]
    [InlineData("capture.unknown", "amiga.amigados", ".adf", false)]
    public void ExecutorUsesTheSameSourceAwareMatrixAsThePlanner(
        string sourcePath,
        string formatId,
        string targetExtension,
        bool expected)
    {
        var output = new ConversionOutput(formatId, targetExtension, "output" + targetExtension, false);

        Assert.Equal(expected, ConversionBatchExecutor.IsInternal(sourcePath, output));
    }

    [Fact]
    public void PreservedFluxRowsRequireFluxParityInsteadOfSectorParity()
    {
        var row = MediaEngineParityCatalog.Matrix.Find(".hfe", "raw.scp", ".scp");

        Assert.NotNull(row);
        Assert.Equal(ParityValidationStatus.NotApplicable, row.BlocksIdentical);
        Assert.Equal(ParityValidationStatus.NotApplicable, row.FilesIdentical);
        Assert.Equal(ParityValidationStatus.NotApplicable, row.MetadataIdentical);
        Assert.Equal(ParityValidationStatus.Passed, row.FluxIdentical);
        Assert.True(row.IsValidatedFor(MediaParityOperation.Conversion));
    }

    [Fact]
    public void ComparisonBuildsEvidenceFromReopenedMediaEngineAndGwDocuments()
    {
        var mediaEngine = Document([1, 2, 3, 4]);
        var greaseweazle = Document([1, 2, 3, 4]);

        var row = MediaParityComparisonService.Compare(
            "format",
            ".source",
            ".target",
            mediaEngine,
            greaseweazle);

        Assert.Equal("1x1x1x4", row.Geometry);
        Assert.Equal(ParityValidationStatus.Passed, row.BlocksIdentical);
        Assert.Equal(ParityValidationStatus.Passed, row.FilesIdentical);
        Assert.Equal(ParityValidationStatus.Passed, row.MetadataIdentical);
        Assert.True(row.IsValidatedFor(MediaParityOperation.Conversion));
    }

    [Fact]
    public void ComparisonRejectsDifferentLogicalBlocks()
    {
        var mediaEngine = Document([1, 2, 3, 4]);
        var greaseweazle = Document([1, 2, 3, 5]);

        var row = MediaParityComparisonService.Compare(
            "format",
            ".source",
            ".target",
            mediaEngine,
            greaseweazle);

        Assert.Equal(ParityValidationStatus.Failed, row.BlocksIdentical);
        Assert.Equal(ParityValidationStatus.Failed, row.Conversion);
        Assert.False(row.IsValidatedFor(MediaParityOperation.Conversion));
        Assert.True(row.GwFallbackAvailable);
    }

    private static ExploredDiskImage Document(byte[] content)
    {
        var image = new SectorImage(
            "format",
            content.Length,
            1,
            1,
            1,
            [new SectorBlock(0, new SectorAddress(0, 0, 1), content)]);
        var entry = new FileSystemEntry(
            "file",
            FileSystemEntryKind.File,
            content.Length,
            null,
            "",
            0,
            1,
            true,
            [],
            content);
        var volume = new FileSystemVolume("volume", "fs", content.Length, 0, null, null, [entry], []);
        return new ExploredDiskImage(
            "source",
            image,
            volume,
            new DiskImageMetadata(["system"], null));
    }
}
