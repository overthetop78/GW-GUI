using GWGUI.App.Constants.Controls.Visual;
using C = GWGUI.App.Constants.Emulation.StorageConfigurationLayoutConstants;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Contracts.Emulation.Settings;
using GWGUI.App.Functions.Views.Emulation.Settings;
using GWGUI.App.Localization.Extensions;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.App.Functions.Views.Emulation.Storage;

internal static partial class StorageConfigurationFieldFunctions
{
    internal static string Read(FrameworkElement control) => EmulationSettingsFieldControlFunctions.Read(control) ?? string.Empty;

    internal static FrameworkElement Create(EmulationSettingsField field, string? moduleId, IReadOnlyDictionary<string, string>? values)
    {
        field = field with { Value = values?.GetValueOrDefault(field.Id) ?? field.Value };
        var choices = (field.Choices ?? []).Select(choice => new EmulationSettingsChoiceView(choice, ChoiceLabel(choice, field, moduleId))).ToArray();
        var control = field.Editor == EmulationSettingsEditor.Toggle ? EmulationSettingsFieldControlFunctions.CreateToggle(field)
            : field.NumericRange is not null && field.Choices?.Count > 0 && field.AutomaticValue is null ? CreatePresets(field, moduleId)
            : field.NumericRange is not null ? EmulationSettingsFieldControlFunctions.CreateSlider(field, choice => ChoiceLabel(choice, field, moduleId))
            : EmulationSettingsFieldControlFunctions.CreateSelection(field, choices);
        if (field.NumericRange?.IsVertical == true)
            foreach (var slider in Sliders(control))
            {
                slider.Height = C.VerticalSliderHeight;
                slider.IsDirectionReversed = false;
            }
        control.IsEnabled = field.IsEnabled;
        return control;
    }

    /// <summary>Regroupe les champs déclarés par le module, sans connaître les options d'une machine particulière.</summary>
    internal static StackPanel CreateGroups(IReadOnlyList<EmulationSettingsField> fields, string? moduleId,
        IReadOnlyDictionary<string, string>? values, IDictionary<string, FrameworkElement> controls)
    {
        var groups = new StackPanel();
        foreach (var group in fields.Where(field => field.IsVisible).GroupBy(field => field.BlockId))
        {
            var body = new StackPanel();
            var vertical = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Center };
            foreach (var field in group)
            {
                var control = Create(field, moduleId, values);
                controls[field.Id] = control;
                if (field.NumericRange?.IsVertical == true)
                {
                    var column = new StackPanel { Width = C.VerticalFieldWidth, Margin = new Thickness(3, 6, 3, 0) };
                    column.Children.Add(new TextBlock { Text = LocExtension.GetForModule(moduleId ?? string.Empty, field.LabelResourceKey), TextAlignment = TextAlignment.Center, TextWrapping = TextWrapping.Wrap, Height = C.VerticalLabelHeight, Margin = new Thickness(0, 0, 0, 6) });
                    column.Children.Add(control);
                    vertical.Children.Add(column);
                }
                else if (control is CheckBox toggle)
                {
                    toggle.Content = new TextBlock { Text = LocExtension.GetForModule(moduleId ?? string.Empty, field.LabelResourceKey), TextWrapping = TextWrapping.Wrap };
                    toggle.Margin = new Thickness(0, 4, 0, 8);
                    body.Children.Add(toggle);
                }
                else body.Children.Add(Present(field, control, moduleId));
            }
            UIElement content = body;
            if (vertical.Children.Count == 1 && body.Children.Count > 0)
            {
                var row = new Grid();
                row.ColumnDefinitions.Add(new ColumnDefinition());
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                row.Children.Add(body);
                Grid.SetColumn(vertical, 1);
                row.Children.Add(vertical);
                content = row;
            }
            else if (vertical.Children.Count > 0) body.Children.Add(vertical);
            var title = LocExtension.GetForModule(moduleId ?? string.Empty, group.Key);
            if (title == group.Key) title = LocExtension.Get("Emulation.Input.Behavior");
            groups.Children.Add(StorageDialogUi.IconCard(IconGlyphs.Settings, title, content));
        }
        return groups;
    }

    private static IEnumerable<Slider> Sliders(DependencyObject root)
    {
        if (root is Slider slider) yield return slider;
        foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
            foreach (var nested in Sliders(child)) yield return nested;
    }

    private static string ChoiceLabel(EmulationSettingsChoice choice, EmulationSettingsField field, string? moduleId) =>
        EmulationSettingsValuePresentationFunctions.WithUnit(choice.InvariantDisplayValue ?? LocExtension.GetForModule(moduleId ?? string.Empty, choice.DisplayResourceKey), field.ValueUnitResourceKey);

    internal static FrameworkElement Present(EmulationSettingsField field, FrameworkElement control, string? moduleId) =>
        EmulationSettingsLayout.VideoSettingsChoice(new GWGUI.App.Contracts.Views.Emulation.Settings.EmulationVideoSettingsField(
            LocExtension.GetForModule(moduleId ?? string.Empty, field.LabelResourceKey), control,
            IsTrailingCheckBox: control is CheckBox,
            Explanation: field.ExplanationResourceKey is null ? null : LocExtension.GetForModule(moduleId ?? string.Empty, field.ExplanationResourceKey),
            DetailedExplanation: field.DetailedExplanationResourceKey is null ? null : LocExtension.GetForModule(moduleId ?? string.Empty, field.DetailedExplanationResourceKey)));
}
