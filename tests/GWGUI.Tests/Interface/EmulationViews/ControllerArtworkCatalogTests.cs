using GWGUI.App.Views.Controls.Options.ControllerVisualization;
using System.Collections;
using System.Reflection;
using GWGUI.Emulation.Constants;
using GWGUI.App.Contracts.Input;
using GWGUI.Emulation.Enums;

namespace GWGUI.Tests.Interface.EmulationViews;

public sealed class ControllerArtworkCatalogTests
{
    private const string DefinitionsFieldName = "ProfileDefinitions";
    private const string LazyValuePropertyName = "Value";
    private const string DefinitionPropertyName = "Value";
    private const string ZonesPropertyName = "Zones";

    [Theory]
    [InlineData(EmulationControllerVisualIds.SonyPsp2000, 16)]
    [InlineData(EmulationControllerVisualIds.SonyPsp3000, 16)]
    [InlineData(EmulationControllerVisualIds.SonyPspGo, 14)]
    public void PspVariantsHaveDistinctNativeButtonZones(string visualId, int expected)
    {
        var lazy = typeof(ControllerArtworkCatalog).GetField(DefinitionsFieldName, BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
        var definitions = Assert.IsAssignableFrom<IEnumerable>(lazy.GetType().GetProperty(LazyValuePropertyName)!.GetValue(lazy));
        var entry = Assert.Single(definitions.Cast<object>(), item => (string)item.GetType().GetProperty("Key")!.GetValue(item)! == visualId);
        var definition = entry.GetType().GetProperty(DefinitionPropertyName)!.GetValue(entry)!;
        var zones = Assert.IsAssignableFrom<IEnumerable<ControllerVisualZone>>(definition.GetType().GetProperty(ZonesPropertyName)!.GetValue(definition)).ToArray();
        Assert.Equal(expected, zones.Length);
        Assert.Equal(expected, zones.Select(zone => zone.Control).Distinct().Count());
        Assert.Contains(zones, zone => zone.Control == EmulationControllerVisualControl.Start);
        Assert.Contains(zones, zone => zone.Control == EmulationControllerVisualControl.Option);
        Assert.Contains(zones, zone => zone.Control == EmulationControllerVisualControl.StickUp);
        Assert.All(zones, zone =>
        {
            Assert.InRange(zone.XPercent, 0, 100 - zone.WidthPercent);
            Assert.InRange(zone.YPercent, 0, 100 - zone.HeightPercent);
        });
        if (visualId == EmulationControllerVisualIds.SonyPspGo)
            Assert.DoesNotContain(zones, zone => zone.Control is EmulationControllerVisualControl.LeftShoulder or EmulationControllerVisualControl.RightShoulder);
    }

    [Fact]
    public void EveryControllerProfileHasInitializedZones()
    {
        var field = typeof(ControllerArtworkCatalog).GetField(DefinitionsFieldName, BindingFlags.NonPublic | BindingFlags.Static);
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
            var key = entry.GetType().GetProperty("Key")?.GetValue(entry) as string;
            if (key is EmulationControllerVisualIds.SonyEyeToy or EmulationControllerVisualIds.SonySingStar or EmulationControllerVisualIds.SonyUsbKeyboard
                or EmulationControllerVisualIds.SonySingStarWireless or EmulationControllerVisualIds.SonySocomHeadset
                or EmulationControllerVisualIds.SonyLogitechVantage or EmulationControllerVisualIds.SonyRockBandMicrophone
                or EmulationControllerVisualIds.SonyGuitarHeroMicrophone)
                Assert.Empty(zones);
            else
                Assert.NotEmpty(zones);
        }
    }
}
