using GWGUI.App.Views.Controls.Options.ControllerVisualization;
using System.Collections;
using System.Reflection;

namespace GWGUI.Tests.Interface.EmulationViews;

public sealed class ControllerArtworkCatalogTests
{
    private const string DefinitionsFieldName = "ProfileDefinitions";
    private const string LazyValuePropertyName = "Value";
    private const string DefinitionPropertyName = "Value";
    private const string ZonesPropertyName = "Zones";

    [Fact]
    public void EveryControllerProfileHasInitializedZones()
    {
        var field = typeof(ControllerArtworkCatalog).GetField(
            DefinitionsFieldName, BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(field);

        var lazy = field.GetValue(null);
        Assert.NotNull(lazy);
        var definitions = lazy.GetType().GetProperty(LazyValuePropertyName)?.GetValue(lazy) as IEnumerable;
        Assert.NotNull(definitions);
        Assert.NotEmpty(definitions);

        foreach (var entry in definitions)
        {
            Assert.NotNull(entry);
            var definition = entry.GetType().GetProperty(DefinitionPropertyName)?.GetValue(entry);
            Assert.NotNull(definition);
            var zones = definition.GetType().GetProperty(ZonesPropertyName)?.GetValue(definition) as IEnumerable;
            Assert.NotNull(zones);
            Assert.NotEmpty(zones);
        }
    }
}
