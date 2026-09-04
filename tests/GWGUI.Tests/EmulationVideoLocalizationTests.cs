using System.Collections;
using System.Globalization;
using System.Resources;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Constants.Localization;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Localization.Sources;
using GWGUI.App.Views.Controls.Emulation.Options;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.Tests;

[Collection(LocalizationTestCollection.Name)]
public sealed class EmulationVideoLocalizationTests
{
    private static readonly string[] Cultures =
    [
        "ar-SA", "cs-CZ", "da-DK", "de-DE", "el-GR", "en-US", "es-ES", "fi-FI", "fr-FR",
        "he-IL", "hu-HU", "id-ID", "it-IT", "ja-JP", "ko-KR", "nb-NO", "nl-NL", "pl-PL",
        "pt-BR", "pt-PT", "ro-RO", "ru-RU", "sv-SE", "th-TH", "tr-TR", "uk-UA", "vi-VN",
        "zh-Hans", "zh-Hant"
    ];

    private static readonly string[] IconKeys =
    [
        "Common.SaveIcon", "Common.TrashIcon", "Icon.Save", "Icon.Reset", "Icon.Copy",
        "Icon.Console", "Options.NextTagExampleIcon"
    ];

    [Fact]
    public void IconsExistOnlyInTheNeutralIconCatalogAndResolveThroughFallback()
    {
        var resources = new ResourceManager("GWGUI.App.Resources.Icons",
            typeof(EmulationResourceKeys).Assembly);

        AssertCultureContains(resources, CultureInfo.InvariantCulture, IconKeys);
        foreach (var cultureName in Cultures)
        {
            var culture = CultureInfo.GetCultureInfo(cultureName);
            Assert.Null(resources.GetResourceSet(culture, createIfNotExists: true, tryParents: false));
            AssertCultureResolves(resources, culture, IconKeys);
        }
    }

    [Fact]
    public void EveryVideoResourceKeyResolvesInEveryCulture()
    {
        var keys = typeof(EmulationResourceKeys).GetFields()
            .Where(field => field.IsLiteral && field.FieldType == typeof(string))
            .Select(field => (string)field.GetRawConstantValue()!)
            .Where(key => key.StartsWith("Emulation.Video.", StringComparison.Ordinal))
            .ToArray();
        var resources = new ResourceManager("GWGUI.App.Resources.Emulation",
            typeof(EmulationResourceKeys).Assembly);

        AssertCultureContains(resources, CultureInfo.InvariantCulture, keys);
        foreach (var culture in Cultures)
            AssertCultureResolves(resources, CultureInfo.GetCultureInfo(culture), keys);
    }

    [Fact]
    public void FrenchKeepsPlasmaAsTheTechnologyName()
    {
        var resources = new ResourceManager("GWGUI.App.Resources.Emulation",
            typeof(EmulationResourceKeys).Assembly);
        var french = CultureInfo.GetCultureInfo("fr-FR");

        Assert.Equal("Plasma", resources.GetString(
            EmulationResourceKeys.VideoTechnologyPlasma, french));
        Assert.Equal("Plasma", resources.GetString(
            EmulationResourceKeys.VideoPresetPlasma, french));
    }

    [Fact]
    public void RefreshLocalizedContentImmediatelyReplacesVisiblePanelText()
    {
        RunSta(() =>
        {
            var source = LocalizationSource.Instance;
            var originalCulture = source.Culture;
            var originalUiCulture = source.UiCulture;
            try
            {
                var englishCulture = CultureInfo.GetCultureInfo("en-US");
                source.SetCultures(englishCulture, englishCulture);
                var panel = new EmulationVideoProcessingSettingsSection();
                panel.SetConfiguration(new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.Crt
                });
                var english = LocExtension.Get(EmulationResourceKeys.VideoGeneralSettings);
                Assert.Contains(english, Texts(panel));

                var frenchCulture = CultureInfo.GetCultureInfo("fr-FR");
                source.SetCultures(frenchCulture, frenchCulture);
                panel.RefreshLocalizedContent();
                var french = LocExtension.Get(EmulationResourceKeys.VideoGeneralSettings);
                var visible = Texts(panel);

                Assert.NotEqual(english, french);
                Assert.Contains(french, visible);
                Assert.DoesNotContain(english, visible);
            }
            finally
            {
                source.SetCultures(originalCulture, originalUiCulture);
            }
        });
    }

    private static void AssertCultureContains(ResourceManager resources, CultureInfo culture,
        IReadOnlyCollection<string> expected)
    {
        var set = resources.GetResourceSet(culture, createIfNotExists: true, tryParents: false);
        Assert.NotNull(set);
        var actual = set.Cast<DictionaryEntry>().Select(entry => (string)entry.Key)
            .ToHashSet(StringComparer.Ordinal);
        var missing = expected.Where(key => !actual.Contains(key)).ToArray();
        Assert.True(missing.Length == 0,
            $"{culture.Name}: missing {string.Join(", ", missing)}");
    }

    private static void AssertCultureResolves(ResourceManager resources, CultureInfo culture,
        IReadOnlyCollection<string> expected)
    {
        var missing = expected.Where(key => resources.GetString(key, culture) is null).ToArray();
        Assert.True(missing.Length == 0,
            $"{culture.Name}: unresolved {string.Join(", ", missing)}");
    }

    private static IReadOnlyList<string> Texts(DependencyObject root) =>
        Descendants(root).OfType<TextBlock>().Select(text => text.Text).ToArray();

    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        yield return root;
        foreach (var logicalChild in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
            foreach (var child in Descendants(logicalChild))
                yield return child;
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception error) { failure = error; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();
    }
}
