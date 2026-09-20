using FileSystemEntryKind = GWGUI.MediaEngine.Enums.FileSystemEntryKind;
using GWGUI.App.Dictionaries.Explorer.FileTypes;
using GWGUI.App.Enums.Explorer;
using GWGUI.App.Functions.Explorer;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts.Explorer;

namespace GWGUI.Tests.Interface.ExplorerViews;

public sealed class ExplorerFileTypeCatalogScenarios
{
    [Fact]
    public void FamilyRuleTakesPriorityOverCommonRule()
    {
        var atari = ExplorerFileTypeCatalog.Find(ExplorerFileSystemFamily.Atari8Bit, ".TXT");
        var common = ExplorerFileTypeCatalog.Find(ExplorerFileSystemFamily.Unknown, ".TXT");

        Assert.NotNull(atari);
        Assert.NotNull(common);
        Assert.Equal(ExplorerTextEncoding.Atascii, atari.TextEncoding);
        Assert.Equal(ExplorerTextEncoding.Unknown, common.TextEncoding);
    }

    [Theory]
    [InlineData(ExplorerFileSystemFamily.IbmPc, "RUN.BAT", ExplorerFileCategory.Command, ExplorerExecutionKind.CommandScript)]
    [InlineData(ExplorerFileSystemFamily.AtariSt, "DEMO.PRG", ExplorerFileCategory.Executable, ExplorerExecutionKind.NativeExecutable)]
    [InlineData(ExplorerFileSystemFamily.Atari8Bit, "GAME.XEX", ExplorerFileCategory.Executable, ExplorerExecutionKind.NativeExecutable)]
    [InlineData(ExplorerFileSystemFamily.Msx, "TOOL.COM", ExplorerFileCategory.Executable, ExplorerExecutionKind.NativeExecutable)]
    [InlineData(ExplorerFileSystemFamily.Cpm, "START.SUB", ExplorerFileCategory.Command, ExplorerExecutionKind.CommandScript)]
    public void MachineRulesAreCaseInsensitive(
        ExplorerFileSystemFamily family,
        string fileName,
        ExplorerFileCategory category,
        ExplorerExecutionKind execution)
    {
        var definition = ExplorerFileTypeCatalog.Find(family, Path.GetExtension(fileName));

        Assert.NotNull(definition);
        Assert.Equal(category, definition.Category);
        Assert.Equal(execution, definition.ExecutionKind);
    }

    [Fact]
    public void RowsAreUniqueAndNormalized()
    {
        Assert.All(ExplorerFileTypeCatalog.Rows, row =>
        {
            Assert.StartsWith(".", row.Extension);
            Assert.Equal(row.Extension.ToLowerInvariant(), row.Extension);
        });
        Assert.Equal(
            ExplorerFileTypeCatalog.Rows.Count,
            ExplorerFileTypeCatalog.Rows.Select(row => (row.Family, row.Extension)).Distinct().Count());
    }

    [Fact]
    public void CatalogContainsEverySupportedFileSystemFamily()
    {
        var expected = Enum.GetValues<ExplorerFileSystemFamily>()
            .Where(family => family != ExplorerFileSystemFamily.Unknown);

        Assert.All(expected, family => Assert.Contains(ExplorerFileTypeCatalog.Rows, row => row.Family == family));
    }

    [Fact]
    public void AtariCassetteContentIsClassifiedFromItsBytes()
    {
        Assert.Equal(
            ExplorerFileSystemFamily.Atari8Bit,
            ExplorerFileIconClassifier.FamilyFor(TapeImageFormatIds.AtariCas, "sequential-content"));

        var basic = new byte[]
        {
            0x00, 0x00, 0x00, 0x01, 0x01, 0x01, 0x01, 0x01,
            0x04, 0x01, 0x08, 0x01, 0x08, 0x01, 0, 0, 0, 0, 0, 0, 0, 0
        };
        var xex = new byte[] { 0xff, 0xff, 0x00, 0x20, 0x02, 0x20, 1, 2, 3 };
        var boot = new byte[128];
        boot[1] = 1;
        boot[2] = 0x00;
        boot[3] = 0x07;
        boot[4] = 0x00;
        boot[5] = 0x07;

        Assert.Equal(ExplorerFileCategory.BasicProgram, DefinitionFor(basic).Category);
        Assert.Equal(ExplorerFileCategory.Executable, DefinitionFor(xex).Category);
        Assert.Equal(ExplorerFileCategory.BootProgram, DefinitionFor(boot).Category);
    }

    private static GWGUI.App.Contracts.Explorer.ExplorerFileTypeDefinition DefinitionFor(byte[] content) =>
        ExplorerFileIconClassifier.DefinitionFor(
            new FileSystemEntry("File 0001.bin", FileSystemEntryKind.File, content.Length, null, string.Empty, 0, 0, true, [], content),
            ExplorerFileSystemFamily.Atari8Bit);
}
