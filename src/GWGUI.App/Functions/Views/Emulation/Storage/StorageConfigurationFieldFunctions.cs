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
        IReadOnlyDictionary<string, string>? values, IDictionary<string, FrameworkElement> controls, bool compact = false)
    {
        var groups = new StackPanel();
        foreach (var group in fields.Where(field => field.IsVisible).GroupBy(field => field.BlockId))
        {
            if (compact)
            {
                var label = LocExtension.GetForModule(moduleId ?? string.Empty, group.Key);
                if (label == group.Key) label = LocExtension.Get("Emulation.Input.Behavior");
                groups.Children.Add(StorageDialogUi.IconCard(IconGlyphs.Settings, label,
                    CompactGroup(group.ToArray(), moduleId, values, controls), compact: true));
                continue;
            }
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

    /// <summary>Aligne les champs ordinaires et réserve une colonne indépendante aux réglages verticaux.</summary>
    internal static Grid CompactGroup(IReadOnlyList<EmulationSettingsField> fields, string? moduleId,
        IReadOnlyDictionary<string, string>? values, IDictionary<string, FrameworkElement> controls, int columns = 1)
    {
        var content = new Grid();
        content.ColumnDefinitions.Add(new ColumnDefinition());
        content.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var form = new Grid { VerticalAlignment = VerticalAlignment.Center };
        for (var column = 0; column < columns; column++) form.ColumnDefinitions.Add(new ColumnDefinition());
        var vertical = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        var index = 0;
        foreach (var field in fields.Where(field => field.IsVisible))
        {
            var control = Create(field, moduleId, values);
            controls[field.Id] = control;
            var label = LocExtension.GetForModule(moduleId ?? string.Empty, field.LabelResourceKey);
            var row = new StackPanel { Margin = new Thickness(0, 3, 10, 7) };
            if (field.ExplanationResourceKey is { } help) control.ToolTip = LocExtension.GetForModule(moduleId ?? string.Empty, help);
            if (control is CheckBox check) check.Content = new TextBlock { Text = label, TextWrapping = TextWrapping.Wrap };
            else row.Children.Add(new TextBlock { Text = label, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 4) });
            row.Children.Add(control);
            if (field.NumericRange?.IsVertical == true)
            {
                if (control is Grid { Tag: Slider } sliderGrid)
                {
                    sliderGrid.ColumnDefinitions.Clear();
                    sliderGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    sliderGrid.RowDefinitions.Clear();
                    sliderGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    sliderGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    foreach (var child in sliderGrid.Children.OfType<FrameworkElement>())
                    {
                        Grid.SetColumn(child, 0);
                        Grid.SetRow(child, child is Slider ? 0 : 1);
                        if (child is TextBlock value)
                        {
                            value.MinWidth = 0;
                            value.Margin = new Thickness(0, 4, 0, 0);
                            value.TextAlignment = TextAlignment.Center;
                        }
                    }
                }
                row.Width = C.VerticalFieldWidth;
                row.Margin = new Thickness(8, 0, 0, 0);
                foreach (var text in row.Children.OfType<TextBlock>()) text.TextAlignment = TextAlignment.Center;
                control.HorizontalAlignment = HorizontalAlignment.Center;
                vertical.Children.Add(row);
            }
            else
            {
                var position = index / columns;
                if (position == form.RowDefinitions.Count) form.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                Grid.SetRow(row, position); Grid.SetColumn(row, index % columns); form.Children.Add(row); index++;
            }
        }
        content.Children.Add(form);
        if (vertical.Children.Count > 0) { Grid.SetColumn(vertical, 1); content.Children.Add(vertical); }
        return content;
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
