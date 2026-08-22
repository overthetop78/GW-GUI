using GWGUI.App.Constants.Input.Bindings;
using GWGUI.App.Constants.Input.Controllers;
using GWGUI.App.Enums.Input;
using GWGUI.App.Functions.Input.Bindings;
using GWGUI.App.ViewModels.Input;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace GWGUI.App.Views.Controls.Emulation.Input;

public partial class InputBindingEditor
{
    private void ClearClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: InputBindingRow row }) return;
        row.Binding = string.Empty;
        ValidateBindings();
        BindingsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void RestoreDefaultsClicked(object sender, RoutedEventArgs e)
    {
        foreach (var row in _rows) row.Binding = row.DefaultBinding;
        ValidateBindings();
        BindingsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ClearConflictsClicked(object sender, RoutedEventArgs e)
    {
        foreach (var row in _rows.Where(row => row.State is InputBindingState.Conflict or InputBindingState.Reserved))
            row.Binding = string.Empty;
        ValidateBindings();
        BindingsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SearchChanged(object sender, TextChangedEventArgs e)
    {
        var view = CollectionViewSource.GetDefaultView(_rows);
        var query = SearchBox.Text.Trim();
        view.Filter = item => item is InputBindingRow row && (query.Length == 0 ||
            row.Label.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
            row.Binding.Contains(query, StringComparison.CurrentCultureIgnoreCase));
    }

    private void ValidateBindings()
    {
        var duplicates = _rows.Where(row => !string.IsNullOrWhiteSpace(row.Binding))
            .GroupBy(row => row.Binding.Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1).Select(group => group.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var row in _rows)
        {
            if (string.IsNullOrWhiteSpace(row.Binding)) row.SetState(InputBindingState.Unassigned);
            else if (!TryValidateBinding(row.Binding, out var reserved) || reserved) row.SetState(InputBindingState.Reserved);
            else if (duplicates.Contains(row.Binding.Trim())) row.SetState(InputBindingState.Conflict);
            else row.SetState(InputBindingState.Valid);
        }
        BindingsList.Items.Refresh();
    }

    private bool TryValidateBinding(string value, out bool reserved)
    {
        reserved = false;
        var trimmed = value.Trim();
        if (InputBindingSyntax.TryRemovePrefix(trimmed, InputBindingSyntaxConstants.MousePrefix, out _))
            return _captureSources.HasFlag(InputCaptureSources.Mouse);
        if (InputBindingSyntax.TryRemovePrefix(trimmed, InputBindingSyntaxConstants.ControllerPrefix, out _))
            return _captureSources.HasFlag(InputCaptureSources.Controller);
        if (ControllerInputConstants.LegacyButtonNames.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
            return _captureSources.HasFlag(InputCaptureSources.Controller);
        var keyboard = InputBindingSyntax.TryRemovePrefix(trimmed, InputBindingSyntaxConstants.KeyboardPrefix, out var keyboardSource)
            ? keyboardSource : trimmed;
        if (!_captureSources.HasFlag(InputCaptureSources.Keyboard) || !KeyboardChordFunctions.TryParse(keyboard, out var chord)) return false;
        reserved = KeyboardChordFunctions.IsWindowsReserved(chord) ||
                   _reservedBindings.Contains(trimmed) || _reservedBindings.Contains(keyboard);
        return true;
    }
}
