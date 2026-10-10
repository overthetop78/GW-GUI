using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Constants.Localization;
using GWGUI.App.Functions.Views.Emulation.Storage;
using GWGUI.App.Localization.Extensions;
using System.Windows;
using System.Windows.Controls;
using GWGUI.Emulation;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.App.Contracts.Emulation.Settings;
using GWGUI.App.Functions.Views.Emulation.Settings;

namespace GWGUI.App.Views.Dialogs.Emulation.Storage;

public sealed class CartridgeSlotConfigurationDialog : Window
{
    private readonly CheckBox _option = new();
    private readonly Dictionary<string, ComboBox> _options = new(StringComparer.Ordinal);

    public CartridgeSlotSettings Settings => new(_option.IsChecked == true,
        _options.ToDictionary(item => item.Key,
            item => ((EmulationSettingsChoiceView)item.Value.SelectedItem).Choice.Id, StringComparer.Ordinal));

    public CartridgeSlotConfigurationDialog(string identifier, string machineName,
        CartridgeSlotSettings settings, string optionLabel, string? optionHint,
        string? moduleId = null, IReadOnlyList<EmulationSettingsField>? configurationFields = null)
    {
        Title = $"{LocExtension.Get(EmulationResourceKeys.StorageDeviceConfigure)} {identifier}";
        Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
        ShowInTaskbar = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Width = 720;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.NoResize;
        _option.Content = optionLabel;
        _option.IsChecked = settings.OptionEnabled;
        _option.Margin = new Thickness(0, 4, 0, 4);

        var body = new StackPanel();
        if (!string.IsNullOrWhiteSpace(optionLabel)) body.Children.Add(_option);
        if (!string.IsNullOrWhiteSpace(optionHint)) body.Children.Add(StorageDialogUi.Info(optionHint));
        foreach (var field in configurationFields ?? [])
        {
            if (field.Editor != EmulationSettingsEditor.Selection || field.Choices is not { Count: > 0 })
                throw new ArgumentException(nameof(configurationFields));
            var choices = field.Choices.Select(choice => new EmulationSettingsChoiceView(choice,
                choice.InvariantDisplayValue ?? LocExtension.GetForModule(moduleId ?? string.Empty,
                    choice.DisplayResourceKey))).ToArray();
            var value = settings.Options?.TryGetValue(field.Id, out var selectedValue) == true
                ? selectedValue : field.Value;
            var control = new ComboBox { ItemsSource = choices,
                SelectedItem = choices.FirstOrDefault(choice => choice.Choice.Id == value) ?? choices[0] };
            _options[field.Id] = control;
            body.Children.Add(EmulationSettingsLayout.BoundedForm(1, new EmulationSettingsControlField(
                LocExtension.GetForModule(moduleId ?? string.Empty, field.LabelResourceKey), control,
                field.ExplanationResourceKey is null ? null : LocExtension.GetForModule(
                    moduleId ?? string.Empty, field.ExplanationResourceKey))));
        }
        Content = StorageDialogUi.DialogLayout(
            StorageDialogUi.DialogHeader(IconGlyphs.Controller, Title,
                $"{LocExtension.Get(EmulationResourceKeys.CartridgeDevice)} · {machineName}"),
            StorageDialogUi.Card(LocExtension.Get(EmulationResourceKeys.CartridgeDevice), body),
            StorageDialogUi.Footer(this, LocExtension.Get("Common.Save")));
    }
}
