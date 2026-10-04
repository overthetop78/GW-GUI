using GWGUI.App.Constants.Localization;
using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Contracts.Storage;
using GWGUI.App.Functions.Views.Emulation.Storage;
using GWGUI.App.Localization.Extensions;
using System.Windows;
using System.Windows.Controls;
using GWGUI.Emulation.Contracts;

namespace GWGUI.App.Views.Dialogs.Emulation.Storage;

public sealed class CompactDiscDriveConfigurationDialog : Window
{
    private readonly ComboBox _speed = new();
    private readonly CheckBox _cache = new();
    private readonly CheckBox _ignoreErrors = new();

    public EmulationCompactDiscDriveSettings Settings => new(
        (_speed.SelectedItem as StorageDialogChoice)?.Value
            ?? new EmulationCompactDiscDriveSettings().Speed,
        _cache.IsChecked == true, _ignoreErrors.IsChecked == true);

    public CompactDiscDriveConfigurationDialog(string identifier, string machineName,
        string modelName,
        EmulationCompactDiscDriveSettings settings, IReadOnlyList<string>? speeds,
        string? cacheLabel, string? ignoreErrorsLabel)
    {
        Title = $"{LocExtension.Get(EmulationResourceKeys.StorageDeviceConfigure)} {identifier}";
        Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
        ShowInTaskbar = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Width = 720;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.NoResize;
        _speed.ItemsSource = (speeds ?? []).Select(speed =>
            new StorageDialogChoice(speed,
                $"{speed}×")).ToArray();
        _speed.SelectedItem = _speed.Items.OfType<StorageDialogChoice>()
            .FirstOrDefault(choice => choice.Value == settings.Speed)
            ?? _speed.Items.OfType<StorageDialogChoice>().FirstOrDefault();
        _cache.IsChecked = settings.CacheImage;
        _ignoreErrors.IsChecked = settings.IgnoreErrors;
        var entries = new List<(string Label, FrameworkElement Control)>
        {
            (LocExtension.Get(EmulationResourceKeys.DeviceIdentifier),
                new TextBox { Text = identifier, IsReadOnly = true }),
            (LocExtension.Get(EmulationResourceKeys.Model),
                new TextBox { Text = modelName, IsReadOnly = true })
        };
        if (speeds is { Count: > 0 })
            entries.Add((LocExtension.Get(EmulationResourceKeys.CompactDiscSpeed),
                _speed));
        if (!string.IsNullOrWhiteSpace(cacheLabel)) entries.Add((cacheLabel, _cache));
        if (!string.IsNullOrWhiteSpace(ignoreErrorsLabel))
            entries.Add((ignoreErrorsLabel, _ignoreErrors));
        var fields = StorageDialogUi.TwoColumnFields(entries.ToArray());
        var body = new StackPanel();
        body.Children.Add(fields);
        body.Children.Add(StorageDialogUi.Info(LocExtension.Get(EmulationResourceKeys.StorageRuntimeHint)));
        Content = StorageDialogUi.DialogLayout(
            StorageDialogUi.DialogHeader(IconGlyphs.OpticalDisc, Title,
                $"{LocExtension.Get(EmulationResourceKeys.CompactDiscDevice)} · {machineName}"),
            StorageDialogUi.Card(LocExtension.Get(EmulationResourceKeys.CompactDiscDevice), body),
            StorageDialogUi.Footer(this,
                LocExtension.Get(EmulationResourceKeys.CommonSave)));
    }
}
