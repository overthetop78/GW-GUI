using System.Globalization;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Localization.Sources;
using GWGUI.Emulation.Interfaces;
using GWGUI.Tests.Application.TestInfrastructure;

namespace GWGUI.Tests.Interface.Localization;

[Collection("WPF")]
public class ModuleLocalizationTests(StaExecutionScenarios sta)
{
    [Fact]
    public Task ModuleOverridesHostWithoutAffectingAnotherModule() => sta.Run(() =>
    {
        const string key = "Common.Save";
        var host = LocExtension.Get(key);
        var first = new Catalog(new Dictionary<string, string> { [key] = "First module" });
        var second = new Catalog(new Dictionary<string, string> { [key] = "Second module" });
        Assert.Equal("First module", LocExtension.GetLocalized(first, key));
        Assert.Equal("Second module", LocExtension.GetLocalized(second, key));
        Assert.Equal(host, LocExtension.Get(key));
        Assert.Equal(host, LocExtension.GetLocalized(new Catalog([]), key));
    });

    [Fact]
    public Task MissingKeyAndEmptyValueRemainDifferent() => sta.Run(() =>
    {
        const string key = "Module.Localization.Test.Missing";
        Assert.Equal($"[{key}]", LocExtension.GetLocalized(new Catalog([]), key));
        Assert.Equal(string.Empty, LocExtension.GetLocalized(
            new Catalog(new Dictionary<string, string> { [key] = string.Empty }), key));
    });

    [Fact]
    public Task UsesUiCultureForLookupAndFormattingCultureForArguments() => sta.Run(() =>
    {
        var source = LocalizationSource.Instance;
        var previous = (source.Culture, source.UiCulture);
        try
        {
            var catalog = new Catalog(new Dictionary<string, string> { ["Value"] = "{0:N1}" });
            source.SetCultures(CultureInfo.GetCultureInfo("fr-FR"), CultureInfo.GetCultureInfo("de-DE"));
            Assert.Equal(1.5.ToString("N1", source.Culture), LocExtension.GetLocalized(catalog, "Value", 1.5));
            Assert.Equal("de-DE", catalog.LastCulture?.Name);
            source.SetCultures(CultureInfo.GetCultureInfo("en-US"), CultureInfo.GetCultureInfo("ja-JP"));
            Assert.Equal(1.5.ToString("N1", source.Culture), LocExtension.GetLocalized(catalog, "Value", 1.5));
            Assert.Equal("ja-JP", catalog.LastCulture?.Name);
        }
        finally { source.SetCultures(previous.Culture, previous.UiCulture); }
    });

    private sealed class Catalog(Dictionary<string, string> values) : IEmulationModuleLocalization
    {
        internal CultureInfo? LastCulture { get; private set; }

        public bool TryGetString(string key, CultureInfo culture, out string value)
        {
            LastCulture = culture;
            return values.TryGetValue(key, out value!);
        }
    }
}
