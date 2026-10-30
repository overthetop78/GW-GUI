using GWGUI.Infrastructure.Settings;
using GWGUI.Tests.Application.TestInfrastructure;

namespace GWGUI.Tests.Interface.Themes;

public sealed class ThemeDesignTests
{
    private const string SettingsPath = "preferences.json";

    [Theory]
    [InlineData(AppThemeDesign.Base, AppTheme.System)]
    [InlineData(AppThemeDesign.Base, AppTheme.Light)]
    [InlineData(AppThemeDesign.Base, AppTheme.Dark)]
    public async Task SavedDesignAndAppearanceSurviveNewStoreInstance(AppThemeDesign design, AppTheme appearance)
    {
        var files = new SyntheticData();
        var settings = new AppSettings { ThemeDesign = design, Theme = appearance };
        var writer = new JsonSettingsStore(SettingsPath, files, () => DateTime.UnixEpoch);
        await writer.SaveAsync(settings);

        var reader = new JsonSettingsStore(SettingsPath, files, () => DateTime.UnixEpoch);
        var restored = await reader.LoadAsync();

        Assert.Equal(design, restored.ThemeDesign);
        Assert.Equal(appearance, restored.Theme);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task RemovedDesignFallsBackToBaseWithoutLosingDarkStyle(int removedDesign)
    {
        var files = new SyntheticData();
        files.Seed(SettingsPath, "{\"ThemeDesign\":" + removedDesign + ",\"Theme\":2}");
        var restored = await new JsonSettingsStore(SettingsPath, files, () => DateTime.UnixEpoch).LoadAsync();
        Assert.Equal(AppThemeDesign.Base, GWGUI.App.Dictionaries.Theming.ThemeCatalog.Get(restored.ThemeDesign).Id);
        Assert.Equal(AppTheme.Dark, restored.Theme);
    }

    [Fact]
    public async Task PreferencesWithoutDesignKeepOriginalDarkAppearance()
    {
        var files = new SyntheticData();
        files.Seed(SettingsPath, "{\"Theme\":2}");
        var store = new JsonSettingsStore(SettingsPath, files, () => DateTime.UnixEpoch);

        var restored = await store.LoadAsync();

        Assert.Equal(AppThemeDesign.Base, restored.ThemeDesign);
        Assert.Equal(AppTheme.Dark, restored.Theme);
    }
}
