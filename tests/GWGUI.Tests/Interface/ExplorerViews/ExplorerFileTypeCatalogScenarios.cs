using GWGUI.App.Dictionaries.Explorer.FileTypes;
using GWGUI.App.Enums.Explorer;

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
}
