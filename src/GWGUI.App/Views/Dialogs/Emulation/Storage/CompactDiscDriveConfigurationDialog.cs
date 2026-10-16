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
        IReadOnlyList<EmulationSettingsField>? configurationFields = null)
    {
        Title = $"{LocExtension.Get(EmulationResourceKeys.StorageDeviceConfigure)} {identifier}";
        Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
        ShowInTaskbar = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Width = 920;
        MaxHeight = SystemParameters.WorkArea.Height * 0.85;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.NoResize;
        _speed.ItemsSource = (speeds ?? []).Select(speed => new StorageDialogChoice(speed, $"{speed}×")).ToArray();
        _speed.SelectedItem = _speed.Items.OfType<StorageDialogChoice>().FirstOrDefault(choice => choice.Value == settings.Speed) ?? _speed.Items.OfType<StorageDialogChoice>().FirstOrDefault();
        _cache.IsChecked = settings.CacheImage;
        _ignoreErrors.IsChecked = settings.IgnoreErrors;
        var entries = new List<(string Label, FrameworkElement Control)>
        {
            (LocExtension.Get(EmulationResourceKeys.DeviceIdentifier),
                new TextBlock { Text = identifier, VerticalAlignment = VerticalAlignment.Center }),
            (LocExtension.Get(EmulationResourceKeys.Model),
                new TextBlock { Text = modelName, VerticalAlignment = VerticalAlignment.Center })
        };
        if (speeds is { Count: > 0 })
            entries.Add((LocExtension.Get(EmulationResourceKeys.CompactDiscSpeed), _speed));
        if (!string.IsNullOrWhiteSpace(cacheLabel)) entries.Add((cacheLabel, _cache));
        if (!string.IsNullOrWhiteSpace(ignoreErrorsLabel))
            entries.Add((ignoreErrorsLabel, _ignoreErrors));
        var fields = StorageDialogUi.TwoColumnFields(entries.ToArray());
        var body = new StackPanel();
        body.Children.Add(fields);
        var optionsPanel = new WrapPanel();
        foreach (var field in configurationFields ?? [])
        {
            var control = StorageConfigurationFieldFunctions.Create(field, moduleId, settings.Options);
            _options[field.Id] = control;
            var presentation = StorageConfigurationFieldFunctions.Present(field, control, moduleId);
            presentation.Width = 400;
            presentation.Margin = new Thickness(0, 4, 14, 4);
            optionsPanel.Children.Add(presentation);
        }
        body.Children.Add(optionsPanel);
        body.Children.Add(StorageDialogUi.Info(LocExtension.Get(EmulationResourceKeys.StorageRuntimeHint)));
        Content = StorageDialogUi.DialogLayout(
            StorageDialogUi.DialogHeader(IconGlyphs.OpticalDisc, Title, $"{LocExtension.Get(EmulationResourceKeys.CompactDiscDevice)} · {machineName}"),
            StorageDialogUi.Card(LocExtension.Get(EmulationResourceKeys.CompactDiscDevice),
                new ScrollViewer { Content = body, MaxHeight = SystemParameters.WorkArea.Height * 0.65, VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled }),
            StorageDialogUi.Footer(this, LocExtension.Get(EmulationResourceKeys.CommonSave)));
    }
}
