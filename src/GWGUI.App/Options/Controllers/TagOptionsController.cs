using GWGUI.Domain.Settings;
using GWGUI.App.Dictionaries.Options;
using GWGUI.App.Functions.Options.Tags;
using GWGUI.App.ViewModels.Options;
using GWGUI.App.Views.Controls.Options;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;



namespace GWGUI.App.Options.Controllers;

internal sealed class TagOptionsController
{
    private readonly OptionsGeneralSection _section;
    private readonly AppSettings _settings;
    private readonly Func<bool> _isInitializing;
    private readonly Func<Task> _persistAsync;
    private readonly Func<string, object[], string> _localize;
    private int _exampleIndex;
    private bool _refreshingPresets;

    public TagOptionsController(
        OptionsGeneralSection section,
        AppSettings settings,
        Func<bool> isInitializing,
        Func<Task> persistAsync,
        Func<string, object[], string> localize)
    {
        _section = section;
        _settings = settings;
        _isInitializing = isInitializing;
        _persistAsync = persistAsync;
        _localize = localize;

        _section.UseTagsChanged += UseTagsChanged;
        _section.TagPatternChanged += PatternChanged;
        _section.TagPresetChanged += PresetChanged;
        _section.TagPatternEditingFinished += PatternEditingFinished;
        _section.RecentTagPatternActivated += RecentPatternActivated;
        _section.NextTagExampleRequested += NextExample;

        _section.UseTags.IsChecked = settings.Conversion.AddTags;
        _section.TagPattern.Text = settings.Conversion.TagPattern;
        RefreshLocalizedContent();
        RefreshRecentPatterns();
    }

    public void RefreshLocalizedContent()
    {
        RefreshPreview();
        RefreshPresets();
        _section.TagVariables.ItemsSource = TagVariableDefinitions.All
            .Select(item => new TagVariableOption(item.Token, Localize(item.Key)))
            .ToArray();
    }

    public void ApplyTo(AppSettings settings)
    {
        settings.Conversion.TagPattern = _section.TagPattern.Text;
        settings.Conversion.AddTags = _section.UseTags.IsChecked == true;
    }

    private async void UseTagsChanged(object sender, RoutedEventArgs e)
    {
        if (_isInitializing()) return;
        _settings.Conversion.AddTags = _section.UseTags.IsChecked == true;
        await _persistAsync();
    }

    private void PatternChanged(object sender, TextChangedEventArgs e)
    {
        if (!_isInitializing() && !_refreshingPresets && !IsPreset(_section.TagPattern.Text))
            _section.TagPresets.SelectedItem = null;
        RefreshPreview();
    }

    private async void PresetChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing() || _refreshingPresets || _section.TagPresets.SelectedItem is not TagPresetOption preset) return;
        _section.TagPattern.Text = preset.Pattern;
        await _persistAsync();
    }

    private async void PatternEditingFinished(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (!IsPreset(_section.TagPattern.Text) &&
            RecentTagPatternFunctions.Remember(_settings.Conversion.RecentCustomTagPatterns, _section.TagPattern.Text))
            RefreshRecentPatterns();
        await _persistAsync();
    }

    private async void RecentPatternActivated(object sender, MouseButtonEventArgs e)
    {
        if (_section.RecentTagPatternsList.SelectedItem is not RecentTagPatternOption { Pattern: not null } item) return;
        _section.TagPattern.Text = item.Pattern;
        await _persistAsync();
    }

    private void NextExample(object sender, RoutedEventArgs e)
    {
        _exampleIndex++;
        RefreshPreview();
    }

    private void RefreshPreview() =>
        _section.TagPreview.Text = Localize("Options.TagPatternPreview", TagPatternFormatter.CreateExample(_section.TagPattern.Text, _exampleIndex));

    private void RefreshPresets()
    {
        var current = _section.TagPattern.Text;
        var presets = TagPresetDefinitions.All
            .Select(item => new TagPresetOption(Localize(item.Key), item.Pattern))
            .ToArray();
        _refreshingPresets = true;
        try
        {
            _section.TagPresets.ItemsSource = presets;
            _section.TagPresets.SelectedItem = presets.FirstOrDefault(item =>
                string.Equals(item.Pattern, current, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            _refreshingPresets = false;
        }
    }

    private void RefreshRecentPatterns() =>
        _section.RecentTagPatternsList.ItemsSource = Enumerable.Range(0, 5)
            .Select(index => new RecentTagPatternOption(
                index + 1,
                index < _settings.Conversion.RecentCustomTagPatterns.Count
                    ? _settings.Conversion.RecentCustomTagPatterns[index]
                    : null))
            .ToArray();

    private static bool IsPreset(string pattern) => TagPresetDefinitions.All.Any(item =>
        string.Equals(item.Pattern, pattern, StringComparison.OrdinalIgnoreCase));

    private string Localize(string key, params object[] arguments) => _localize(key, arguments);
}
