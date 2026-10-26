using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using GWGUI.App.Contracts.Emulation.Settings;
using GWGUI.Emulation.Contracts;
using C = GWGUI.App.Constants.Emulation.StorageConfigurationLayoutConstants;

namespace GWGUI.App.Functions.Views.Emulation.Storage;

internal static partial class StorageConfigurationFieldFunctions
{
    /// <summary>Présélections numériques fournies par le module ; aucune valeur métier n'est définie dans App.</summary>
    private static FrameworkElement CreatePresets(EmulationSettingsField field, string? moduleId)
    {
        var range = field.NumericRange!;
        var current = double.TryParse(field.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? Math.Clamp(parsed, range.Minimum, range.Maximum) : range.Minimum;
        var choices = field.Choices!.Where(choice => double.TryParse(choice.Id, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) && value >= range.Minimum && value <= range.Maximum).Select(choice => new EmulationSettingsChoiceView(choice, ChoiceLabel(choice, field, moduleId) + (string.IsNullOrEmpty(range.UnitSuffix) ? string.Empty : " " + range.UnitSuffix))).ToList();
        if (!choices.Any(choice => choice.Choice.Id == current.ToString(CultureInfo.InvariantCulture)))
        {
            var value = current.ToString(CultureInfo.InvariantCulture);
            choices.Add(new(new(value, string.Empty, value), value + " " + range.UnitSuffix));
        }
        var ticks = choices.Select(choice => double.Parse(choice.Choice.Id, CultureInfo.InvariantCulture)).Distinct().Order().ToArray();
        var slider = new Slider { Minimum = range.Minimum, Maximum = range.Maximum, Value = current, Ticks = new DoubleCollection(ticks), IsSnapToTickEnabled = true, TickPlacement = TickPlacement.BottomRight, MinWidth = C.SliderMinimumWidth, MaxWidth = C.SliderMaximumWidth, VerticalAlignment = VerticalAlignment.Center, AutoToolTipPlacement = AutoToolTipPlacement.TopLeft };
        var selection = new ComboBox { ItemsSource = choices, DisplayMemberPath = nameof(EmulationSettingsChoiceView.DisplayName), SelectedItem = choices.First(choice => choice.Choice.Id == current.ToString(CultureInfo.InvariantCulture)), Width = C.PresetWidth, Margin = new Thickness(10, 0, 0, 0) };
        slider.ValueChanged += (_, _) => selection.SelectedItem = choices.FirstOrDefault(choice => double.Parse(choice.Choice.Id, CultureInfo.InvariantCulture) == slider.Value);
        selection.SelectionChanged += (_, _) => { if (selection.SelectedItem is EmulationSettingsChoiceView choice) slider.Value = double.Parse(choice.Choice.Id, CultureInfo.InvariantCulture); };
        var row = new Grid { Tag = (Func<string>)(() => slider.Value.ToString(CultureInfo.InvariantCulture)) };
        row.ColumnDefinitions.Add(new ColumnDefinition()); row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.Children.Add(slider); Grid.SetColumn(selection, 1); row.Children.Add(selection);
        return row;
    }
}
