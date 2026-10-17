using GWGUI.App.Constants.Localization;
using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Contracts.Storage;
using GWGUI.App.Functions.Views.Emulation.Storage;
using GWGUI.App.Localization.Extensions;
using System.Windows;
using System.Windows.Controls;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Dialogs.Emulation.Storage;

public sealed class CompactDiscDriveConfigurationDialog : Window
{
    private readonly ComboBox _speed = new();
    private readonly CheckBox _cache = new();
    private readonly CheckBox _ignoreErrors = new();
    private readonly Dictionary<string, FrameworkElement> _options = new(StringComparer.Ordinal);

    public EmulationCompactDiscDriveSettings Settings => new(
        (_speed.SelectedItem as StorageDialogChoice)?.Value
            ?? new EmulationCompactDiscDriveSettings().Speed,
        _cache.IsChecked == true, _ignoreErrors.IsChecked == true,
        _options.ToDictionary(item => item.Key, item => StorageConfigurationFieldFunctions.Read(item.Value), StringComparer.Ordinal));

    public CompactDiscDriveConfigurationDialog(string identifier, string machineName,
        string modelName,
        EmulationCompactDiscDriveSettings settings, IReadOnlyList<string>? speeds,
        string? cacheLabel, string? ignoreErrorsLabel, string? moduleId = null,
        IReadOnlyList<EmulationSettingsField>? configurationFields = null, string? deviceName = null)
    {
        Title = $"{LocExtension.Get(EmulationResourceKeys.StorageDeviceConfigure)} {identifier}";
        deviceName ??= LocExtension.Get(EmulationResourceKeys.CompactDiscDevice);
        Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
        ShowInTaskbar = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Width = 900;
        MinWidth = 720;
        MaxHeight = SystemParameters.WorkArea.Height * 0.85;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.CanResize;
        _speed.ItemsSource = (speeds ?? []).Select(speed => new StorageDialogChoice(speed, $"{speed}×")).ToArray();
        _speed.SelectedItem = _speed.Items.OfType<StorageDialogChoice>().FirstOrDefault(choice => choice.Value == settings.Speed) ?? _speed.Items.OfType<StorageDialogChoice>().FirstOrDefault();
        _cache.IsChecked = settings.CacheImage;
        _ignoreErrors.IsChecked = settings.IgnoreErrors;
        var entries = new List<(string Label, FrameworkElement Control)>();
        if (!string.Equals(modelName, identifier, StringComparison.OrdinalIgnoreCase))
            entries.Add((LocExtension.Get(EmulationResourceKeys.Model), new TextBlock { Text = modelName, TextWrapping = TextWrapping.Wrap }));
        if (speeds is { Count: > 0 })
            entries.Add((LocExtension.Get(EmulationResourceKeys.CompactDiscSpeed), _speed));
        if (!string.IsNullOrWhiteSpace(cacheLabel)) entries.Add((cacheLabel, _cache));
        if (!string.IsNullOrWhiteSpace(ignoreErrorsLabel))
            entries.Add((ignoreErrorsLabel, _ignoreErrors));
        var fields = StorageDialogUi.TwoColumnFields(entries.ToArray());
        var body = new StackPanel();
        body.Children.Add(fields);
        var optionsPanel = new Grid();
        optionsPanel.ColumnDefinitions.Add(new ColumnDefinition());
        optionsPanel.ColumnDefinitions.Add(new ColumnDefinition());
        var optionFields = (configurationFields ?? []).OrderBy(field => field.Editor == EmulationSettingsEditor.Toggle ? 0 : 1).ToArray();
        var cell = 0;
        var previousWasToggle = true;
        foreach (var field in optionFields)
        {
            var isToggle = field.Editor == EmulationSettingsEditor.Toggle;
            if (previousWasToggle && !isToggle && cell % 2 != 0) cell++;
            previousWasToggle = isToggle;
            while (optionsPanel.RowDefinitions.Count <= cell / 2)
                optionsPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var control = StorageConfigurationFieldFunctions.Create(field, moduleId, settings.Options);
            _options[field.Id] = control;
            var presentation = StorageConfigurationFieldFunctions.Present(field, control, moduleId);
            presentation.Margin = new Thickness(cell % 2 == 0 ? 0 : 14, isToggle ? 4 : 14, cell % 2 == 0 ? 14 : 0, 8);
            Grid.SetRow(presentation, cell / 2);
            Grid.SetColumn(presentation, cell % 2);
            optionsPanel.Children.Add(presentation);
            cell++;
        }
        body.Children.Add(optionsPanel);
        body.Children.Add(StorageDialogUi.Info(LocExtension.Get(EmulationResourceKeys.StorageRuntimeHint)));
        Content = StorageDialogUi.DialogLayout(
            StorageDialogUi.DialogHeader(IconGlyphs.OpticalDisc, Title, $"{deviceName} · {machineName}"),
            StorageDialogUi.Card(deviceName,
                new ScrollViewer { Content = body, MaxHeight = SystemParameters.WorkArea.Height * 0.65, VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled }),
            StorageDialogUi.Footer(this, LocExtension.Get(EmulationResourceKeys.CommonSave)));
    }
}
