using System.Windows.Controls;

namespace GWGUI.App.Functions.Views.Common;

internal static class ComboBoxSelection
{
    internal static void SelectByValue<T>(
        ComboBox comboBox,
        string value,
        Func<T, string?> valueSelector,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase,
        bool selectFirstWhenMissing = true)
    {
        comboBox.SelectedItem = comboBox.Items.OfType<T>().FirstOrDefault(item =>
            string.Equals(valueSelector(item), value, comparison));
        if (comboBox.SelectedItem is null && selectFirstWhenMissing && comboBox.Items.Count > 0)
            comboBox.SelectedIndex = 0;
    }

    internal static string SelectedValue<T>(ComboBox comboBox, Func<T, string?> valueSelector) =>
        comboBox.SelectedItem is T item ? valueSelector(item) ?? string.Empty : comboBox.SelectedItem?.ToString() ?? string.Empty;
}
