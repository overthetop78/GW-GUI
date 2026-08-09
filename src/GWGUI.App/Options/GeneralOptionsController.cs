using System.Windows;
using GWGUI.App.Controls;
using GWGUI.App.Localization;
using GWGUI.Domain.Settings;
using Microsoft.Win32;

namespace GWGUI.App.Options;

internal sealed class GeneralOptionsController
{
    private readonly Window _owner;
    private readonly OptionsGeneralSection _section;
    private readonly AppSettings _settings;
    private readonly Func<bool> _isInitializing;
    private readonly Func<Task> _persistSettings;
    private readonly Action _refreshLocalizedContent;
    private readonly Func<string, object[], string> _localize;

    public GeneralOptionsController(
        Window owner,
        OptionsGeneralSection section,
        AppSettings settings,
        Func<bool> isInitializing,
        Func<Task> persistSettings,
        Action refreshLocalizedContent,
        Func<string, object[], string> localize)
    {
        _owner = owner;
        _section = section;
        _settings = settings;
        _isInitializing = isInitializing;
        _persistSettings = persistSettings;
        _refreshLocalizedContent = refreshLocalizedContent;
        _localize = localize;

        section.ImagesFolder.Text = settings.DefaultImagesFolder;
        section.Languages.ItemsSource = UiLanguageCatalog.Available;
        section.Languages.SelectedItem = UiLanguageCatalog.Available.FirstOrDefault(language =>
            string.Equals(language.Code, settings.Language, StringComparison.OrdinalIgnoreCase))
            ?? UiLanguageCatalog.Fallback;
        section.Themes.SelectedIndex = (int)settings.Theme;

        section.LanguageChanged += LanguageChanged;
        section.ThemeChanged += ThemeChanged;
        section.BrowseImagesFolderRequested += BrowseImagesFolder;
        section.AutoSaveTextEditingFinished += async (_, _) => await _persistSettings();
    }

    public void ApplyTo(AppSettings settings)
    {
        settings.DefaultImagesFolder = _section.ImagesFolder.Text.Trim();
        if (_section.Languages.SelectedItem is UiLanguage language) settings.Language = language.Code;
        settings.Theme = (AppTheme)Math.Max(0, _section.Themes.SelectedIndex);
    }

    private async void LanguageChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (_isInitializing() || _section.Languages.SelectedItem is not UiLanguage language ||
            string.Equals(_settings.Language, language.Code, StringComparison.OrdinalIgnoreCase)) return;

        _settings.Language = language.Code;
        if (Application.Current is App app) app.SetLanguage(language.Code);
        else LocalizationSource.Instance.Refresh();
        _refreshLocalizedContent();
        await _persistSettings();
    }

    private async void ThemeChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (_isInitializing() || _section.Themes.SelectedIndex < 0) return;
        _settings.Theme = (AppTheme)_section.Themes.SelectedIndex;
        if (Application.Current is App app) app.SetTheme(_settings.Theme);
        await _persistSettings();
    }

    private async void BrowseImagesFolder(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Multiselect = false, Title = _localize("Options.ImagesFolder", []) };
        if (dialog.ShowDialog(_owner) != true) return;
        _section.ImagesFolder.Text = dialog.FolderName;
        await _persistSettings();
    }
}
