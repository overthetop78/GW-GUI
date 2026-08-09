using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GWGUI.App.Controls;

public partial class OptionsGeneralSection : UserControl
{
    public OptionsGeneralSection() => InitializeComponent();

    public ScrollViewer Scroller => GeneralScrollViewer;
    public TextBox ImagesFolder => ImagesFolderText;
    public ComboBox Languages => LanguageCombo;
    public ComboBox Themes => ThemeCombo;
    public CheckBox UseTags => UseTagsCheck;
    public ComboBox TagPresets => TagPresetCombo;
    public TextBox TagPattern => TagPatternText;
    public ListBox RecentTagPatternsList => RecentTagPatterns;
    public ItemsControl TagVariables => TagVariablesList;
    public TextBlock TagPreview => TagPatternPreview;

    public event SelectionChangedEventHandler? LanguageChanged;
    public event SelectionChangedEventHandler? ThemeChanged;
    public event RoutedEventHandler? UseTagsChanged;
    public event RoutedEventHandler? BrowseImagesFolderRequested;
    public event TextChangedEventHandler? TagPatternChanged;
    public event SelectionChangedEventHandler? TagPresetChanged;
    public event KeyboardFocusChangedEventHandler? TagPatternEditingFinished;
    public event MouseButtonEventHandler? RecentTagPatternActivated;
    public event RoutedEventHandler? NextTagExampleRequested;
    public event KeyboardFocusChangedEventHandler? AutoSaveTextEditingFinished;

    private void Language_SelectionChanged(object sender, SelectionChangedEventArgs e) => LanguageChanged?.Invoke(sender, e);
    private void Theme_SelectionChanged(object sender, SelectionChangedEventArgs e) => ThemeChanged?.Invoke(sender, e);
    private void UseTags_Changed(object sender, RoutedEventArgs e) => UseTagsChanged?.Invoke(sender, e);
    private void BrowseImagesFolder_Click(object sender, RoutedEventArgs e) => BrowseImagesFolderRequested?.Invoke(sender, e);
    private void TagPattern_Changed(object sender, TextChangedEventArgs e) => TagPatternChanged?.Invoke(sender, e);
    private void TagPreset_SelectionChanged(object sender, SelectionChangedEventArgs e) => TagPresetChanged?.Invoke(sender, e);
    private void TagPattern_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => TagPatternEditingFinished?.Invoke(sender, e);
    private void RecentTagPattern_DoubleClick(object sender, MouseButtonEventArgs e) => RecentTagPatternActivated?.Invoke(sender, e);
    private void NextTagExample_Click(object sender, RoutedEventArgs e) => NextTagExampleRequested?.Invoke(sender, e);
    private void AutoSaveText_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => AutoSaveTextEditingFinished?.Invoke(sender, e);
}
