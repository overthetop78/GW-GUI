using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using GWGUI.App.Contracts.Theming;
using GWGUI.App.Dictionaries.Theming;
using GWGUI.App.Localization.Extensions;
using GWGUI.Infrastructure.Settings;

namespace GWGUI.App.Views.Controls.Options;

public partial class OptionsThemesSection : UserControl
{
    private const string ChooseResource = "Common.Choose";
    private static readonly char[] ChoicePunctuation = ['…', '.'];
    private const string ActiveResource = "Options.Themes.Active";
    private const string SystemResource = "Options.Theme.System";
    private const string DarkResource = "Options.Theme.Dark";
    private const string LightResource = "Options.Theme.Light";
    private const string AppearanceSeparator = " / ";
    private const string VariantGroup = "ThemeAppearance";
    private static readonly Thickness VariantMargin = new(0, 0, 16, 0);
    private Func<AppThemeDesign, AppTheme, Task<bool>>? _save;
    private AppThemeDesign _current;
    private AppTheme _currentVariant;
    private AppTheme _variant;
    private bool _saving;

    public OptionsThemesSection()
    {
        InitializeComponent();
        RefreshLocalizedContent();
    }

    internal ListBox Themes => ThemeList;
    internal Button SaveAction => SaveTheme;
    internal AppThemeDesign SelectedTheme => (ThemeList.SelectedItem as Choice)?.Definition.Id ?? _current;

    internal void Configure(AppThemeDesign current, AppTheme variant, Func<AppThemeDesign, AppTheme, Task<bool>> save)
    {
        _current = ThemeCatalog.Get(current).Id;
        _currentVariant = ThemeCatalog.Get(_current).Normalize(variant);
        _variant = _currentVariant;
        _save = save;
        RefreshLocalizedContent();
        ThemeList.SelectedItem = ThemeList.Items.Cast<Choice>().First(choice => choice.Definition.Id == _current);
        RefreshSelection();
    }

    internal void RefreshLocalizedContent()
    {
        SaveTheme.Content = LocExtension.Get(ChooseResource).TrimEnd(ChoicePunctuation);
        var selected = SelectedTheme;
        ThemeList.ItemsSource = ThemeCatalog.All.Select(definition => new Choice(definition,
            LocExtension.Get(definition.NameResourceKey), string.Join(AppearanceSeparator, definition.Variants.Select(VariantName)))).ToArray();
        ThemeList.SelectedItem = ThemeList.Items.Cast<Choice>().FirstOrDefault(choice => choice.Definition.Id == selected)
            ?? ThemeList.Items.Cast<Choice>().FirstOrDefault();
        RefreshSelection();
    }

    private void SelectionChanged(object sender, SelectionChangedEventArgs args) => RefreshSelection();

    private void RefreshSelection()
    {
        if (ThemeList.SelectedItem is not Choice choice) return;
        ThemeName.Text = choice.Name;
        Description.Text = LocExtension.Get(choice.Definition.DescriptionResourceKey);
        ActiveTheme.Text = LocExtension.Get(ActiveResource, string.Join(AppearanceSeparator, LocExtension.Get(ThemeCatalog.Get(_current).NameResourceKey), VariantName(_currentVariant)));
        _variant = choice.Definition.Normalize(_variant);
        Variants.Children.Clear();
        AppearanceHeading.Visibility = Variants.Visibility = choice.Definition.Variants.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        foreach (var variant in choice.Definition.Variants)
        {
            var option = new RadioButton { Content = VariantName(variant), Tag = variant, GroupName = VariantGroup, IsChecked = variant == _variant, Margin = VariantMargin };
            option.Checked += (_, _) => { _variant = variant; RefreshPreview(choice.Definition); };
            Variants.Children.Add(option);
        }
        RefreshPreview(choice.Definition);
    }

    private static string VariantName(AppTheme variant) => LocExtension.Get(variant == AppTheme.System ? SystemResource : variant == AppTheme.Dark ? DarkResource : LightResource);

    private void RefreshPreview(ThemeDefinition definition)
    {
        SaveTheme.IsEnabled = !_saving && _save is not null && (definition.Id != _current || _variant != _currentVariant);
        Preview.Source = null;
        try
        {
            var resource = Application.GetResourceStream(new Uri(definition.PreviewUri(_variant == AppTheme.System && GWGUI.App.Services.Theming.ThemeManager.SystemUsesDarkTheme() ? AppTheme.Dark : _variant), UriKind.Relative));
            if (resource is not null)
            {
                using var stream = resource.Stream;
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze();
                Preview.Source = bitmap;
            }
        }
        catch (IOException) { }
        PreviewUnavailable.Visibility = Preview.Source is null ? Visibility.Visible : Visibility.Collapsed;
    }

    internal async Task SaveSelectedAsync()
    {
        if (_saving || _save is null || SelectedTheme == _current && _variant == _currentVariant) return;
        var selected = SelectedTheme;
        var variant = _variant;
        _saving = true;
        ThemeList.IsEnabled = false;
        Variants.IsEnabled = false;
        SaveTheme.IsEnabled = false;
        try { if (await _save(selected, variant)) { _current = selected; _currentVariant = variant; } }
        finally
        {
            _saving = false;
            ThemeList.IsEnabled = true;
            Variants.IsEnabled = true;
            RefreshSelection();
        }
    }

    private async void SaveClicked(object sender, RoutedEventArgs args) => await SaveSelectedAsync();
    private sealed record Choice(ThemeDefinition Definition, string Name, string Appearance);
}
