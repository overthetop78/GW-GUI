using GWGUI.Emulation.Contracts;

namespace GWGUI.Tests.Emulation;

public sealed class CoreCatalogDefinitionTests
{
    [Theory]
    [InlineData(null, "Emulation.Emulator.sample.Description")]
    [InlineData("", "")]
    [InlineData("custom.description", "custom.description")]
    public void DefinitionKeepsIdentityAndOriginalDescriptionFallback(string? description, string expected)
    {
        var entry = Entry(new HashSet<Model> { Model.Alpha }, description);
        var definition = entry.ToDefinition();
        Assert.Equal(entry.Id, definition.Id);
        Assert.Equal(entry.LibraryName, definition.DisplayName);
        Assert.Equal(expected, definition.DescriptionResourceKey);
        Assert.Equal(new[] { "Alpha" }, definition.MachineIds);
    }

    [Fact]
    public void EachDefinitionHasIndependentOrdinalModelSnapshot()
    {
        var models = new HashSet<Model> { Model.Alpha, Model.alpha };
        var entry = Entry(models);
        var first = entry.ToDefinition();
        models.Add(Model.Beta);
        var second = entry.ToDefinition();
        Assert.NotSame(first, second);
        Assert.NotSame(first.MachineIds, second.MachineIds);
        Assert.Equal(2, first.MachineIds.Count);
        Assert.True(first.MachineIds.SetEquals(new[] { "Alpha", "alpha" }));
        Assert.False(first.MachineIds.Contains("Beta"));
        Assert.True(second.MachineIds.Contains("Beta"));
        var firstSet = Assert.IsType<HashSet<string>>(first.MachineIds);
        var secondSet = Assert.IsType<HashSet<string>>(second.MachineIds);
        Assert.Same(StringComparer.Ordinal, firstSet.Comparer);
        Assert.Same(StringComparer.Ordinal, secondSet.Comparer);
        firstSet.Clear();
        Assert.Equal(3, secondSet.Count);
    }

    [Fact]
    public void EmptyModelsRemainAnEmptyOrdinalSet()
    {
        var definition = Entry(new HashSet<Model>()).ToDefinition();
        Assert.Empty(definition.MachineIds);
        Assert.Same(StringComparer.Ordinal, Assert.IsType<HashSet<string>>(definition.MachineIds).Comparer);
    }

    [Fact]
    public void UndefinedEnumValuesKeepTheirNumericIdentifier()
    {
        var definition = Entry(new HashSet<Model> { (Model)42 }).ToDefinition();
        Assert.Equal(new[] { "42" }, definition.MachineIds);
    }

    [Fact]
    public void NullModelSetKeepsTheOriginalLinqFailure()
    {
        var error = Assert.Throws<ArgumentNullException>(() => Entry(null!).ToDefinition());
        Assert.Equal("source", error.ParamName);
    }

    private static EmulationCoreCatalogEntry<Core, Model> Entry(IReadOnlySet<Model> models, string? description = null) =>
        new(Core.One, "sample", "Sample library", "core.dll", "archive.zip",
            new Uri("https://example.invalid/archive.zip"), new Uri("https://example.invalid/source"),
            "revision", models, description);

    private enum Core { One }
    private enum Model { Alpha, alpha, Beta }
}
