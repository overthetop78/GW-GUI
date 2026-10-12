using GWGUI.App.Constants.Localization;
using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Contracts.Storage;
using GWGUI.App.Functions.Views.Emulation.Storage;
using GWGUI.App.Localization.Extensions;
using System.Windows;
using System.Windows.Controls;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.App.Contracts.Emulation.Settings;
using GWGUI.App.Functions.Views.Emulation.Settings;
using System.Globalization;

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
        _options.ToDictionary(item => item.Key, item => item.Value switch
        {
            CheckBox { Tag: EmulationSettingsField definition } toggle => toggle.IsChecked == true ? definition.EnabledValue : definition.DisabledValue,
            ComboBox { SelectedItem: EmulationSettingsChoiceView choice } => choice.Choice.Id,
            Slider slider => slider.Value.ToString(CultureInfo.InvariantCulture),
            _ => string.Empty
        }, StringComparer.Ordinal));

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
        Width = 720;
        MaxHeight = SystemParameters.WorkArea.Height * 0.85;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.NoResize;
        _speed.ItemsSource = (speeds ?? []).Select(speed =>
            new StorageDialogChoice(speed,
                $"{speed}×")).ToArray();
        _speed.SelectedItem = _speed.Items.OfType<StorageDialogChoice>().FirstOrDefault(choice => choice.Value == settings.Speed) ?? _speed.Items.OfType<StorageDialogChoice>().FirstOrDefault();
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
            entries.Add((LocExtension.Get(EmulationResourceKeys.CompactDiscSpeed), _speed));
        if (!string.IsNullOrWhiteSpace(cacheLabel)) entries.Add((cacheLabel, _cache));
        if (!string.IsNullOrWhiteSpace(ignoreErrorsLabel))
            entries.Add((ignoreErrorsLabel, _ignoreErrors));
        var fields = StorageDialogUi.TwoColumnFields(entries.ToArray());
        var body = new StackPanel();
        body.Children.Add(fields);
        foreach (var field in configurationFields ?? [])
        {
            FrameworkElement control;
            if (field.Editor == EmulationSettingsEditor.Toggle)
                control = new CheckBox { IsChecked = field.Value == field.EnabledValue, Tag = field };
            else if (field.NumericRange is { } range)
            {
                var value = double.TryParse(field.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : range.Minimum;
                control = new Slider { Minimum = range.Minimum, Maximum = range.Maximum,
                    TickFrequency = range.Step, SmallChange = range.Step, LargeChange = range.Step,
                    IsSnapToTickEnabled = true, Value = Math.Clamp(value, range.Minimum, range.Maximum) };
            }
            else
            {
                var choices = (field.Choices ?? []).Select(choice => new EmulationSettingsChoiceView(choice, choice.InvariantDisplayValue ?? LocExtension.GetForModule(moduleId ?? string.Empty, choice.DisplayResourceKey))).ToArray();
                control = new ComboBox { ItemsSource = choices,
                    SelectedItem = choices.FirstOrDefault(choice => choice.Choice.Id == field.Value) ?? choices.FirstOrDefault() };
            }
            _options[field.Id] = control;
            FrameworkElement presentation = control;
            if (control is Slider slider)
            {
                var number = new TextBlock { VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(8, 0, 0, 0) };
                void UpdateNumber() => number.Text = slider.Value.ToString(CultureInfo.CurrentCulture)
                    + field.NumericRange!.UnitSuffix;
                UpdateNumber();
                slider.ValueChanged += (_, _) => UpdateNumber();
                var row = new Grid();
                row.ColumnDefinitions.Add(new ColumnDefinition());
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                row.Children.Add(slider);
                Grid.SetColumn(number, 1); row.Children.Add(number);
                presentation = row;
            }
            body.Children.Add(EmulationSettingsLayout.BoundedForm(1, new EmulationSettingsControlField(
                LocExtension.GetForModule(moduleId ?? string.Empty, field.LabelResourceKey), presentation,
                field.ExplanationResourceKey is null ? null : LocExtension.GetForModule(moduleId ?? string.Empty, field.ExplanationResourceKey),
                field.DetailedExplanationResourceKey is null ? null : LocExtension.GetForModule(moduleId ?? string.Empty, field.DetailedExplanationResourceKey))));
        }
        body.Children.Add(StorageDialogUi.Info(LocExtension.Get(EmulationResourceKeys.StorageRuntimeHint)));
        Content = StorageDialogUi.DialogLayout(
            StorageDialogUi.DialogHeader(IconGlyphs.OpticalDisc, Title,
                $"{LocExtension.Get(EmulationResourceKeys.CompactDiscDevice)} · {machineName}"),
            StorageDialogUi.Card(LocExtension.Get(EmulationResourceKeys.CompactDiscDevice),
                new ScrollViewer { Content = body, MaxHeight = SystemParameters.WorkArea.Height * 0.65, VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled }),
            StorageDialogUi.Footer(this, LocExtension.Get(EmulationResourceKeys.CommonSave)));
    }
}
