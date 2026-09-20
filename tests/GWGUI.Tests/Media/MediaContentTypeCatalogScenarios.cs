using GWGUI.MediaAnalysis.Dictionaries.FileTypes;
using GWGUI.MediaAnalysis.Enums;
using GWGUI.MediaAnalysis.Functions;

namespace GWGUI.Tests.Media;

public sealed class MediaContentTypeCatalogScenarios
{
    [Fact]
    public void FamilyRuleTakesPriorityOverCommonRule()
    {
        var atari = MediaContentTypeCatalog.Find(MediaFileSystemFamily.Atari8Bit, ".TXT");
        var common = MediaContentTypeCatalog.Find(MediaFileSystemFamily.Unknown, ".TXT");

        Assert.NotNull(atari);
        Assert.NotNull(common);
        Assert.Equal(MediaTextEncoding.Atascii, atari.TextEncoding);
        Assert.Equal(MediaTextEncoding.Unknown, common.TextEncoding);
    }

    [Theory]
    [InlineData(MediaFileSystemFamily.IbmPc, ".BAT", MediaContentCategory.Command, MediaExecutionKind.CommandScript)]
    [InlineData(MediaFileSystemFamily.AtariSt, ".PRG", MediaContentCategory.Executable, MediaExecutionKind.NativeExecutable)]
    [InlineData(MediaFileSystemFamily.Atari8Bit, ".XEX", MediaContentCategory.Executable, MediaExecutionKind.NativeExecutable)]
    [InlineData(MediaFileSystemFamily.Msx, ".COM", MediaContentCategory.Executable, MediaExecutionKind.NativeExecutable)]
    [InlineData(MediaFileSystemFamily.Cpm, ".SUB", MediaContentCategory.Command, MediaExecutionKind.CommandScript)]
    public void MachineRulesAreCaseInsensitive(
        MediaFileSystemFamily family,
        string extension,
        MediaContentCategory category,
        MediaExecutionKind execution)
    {
        var definition = MediaContentTypeCatalog.Find(family, extension);

        Assert.NotNull(definition);
        Assert.Equal(category, definition.Category);
        Assert.Equal(execution, definition.ExecutionKind);
    }

    [Fact]
    public void RowsAreUniqueAndNormalized()
    {
        Assert.All(MediaContentTypeCatalog.Rows, row =>
        {
            Assert.StartsWith(".", row.Extension);
            Assert.Equal(row.Extension.ToLowerInvariant(), row.Extension);
        });
        Assert.Equal(
            MediaContentTypeCatalog.Rows.Count,
            MediaContentTypeCatalog.Rows.Select(row => (row.Family, row.Extension)).Distinct().Count());
    }

    [Fact]
    public void GenericExecutableSignaturesUseExtractedFileBytes()
    {
        var classifier = new MediaContentClassifier();
        var metadata = new Dictionary<string, string>();
        var amiga = classifier.Classify(".bin", MediaEntryKind.File, null, string.Empty, true,
            [0x00, 0x00, 0x03, 0xF3], metadata, MediaFileSystemFamily.Amiga);
        var dos = classifier.Classify(".bin", MediaEntryKind.File, null, string.Empty, true,
            [(byte)'M', (byte)'Z'], metadata, MediaFileSystemFamily.IbmPc);

        Assert.Equal(MediaContentCategory.Executable, amiga?.Category);
        Assert.Equal(MediaContentCategory.Executable, dos?.Category);
    }
}
