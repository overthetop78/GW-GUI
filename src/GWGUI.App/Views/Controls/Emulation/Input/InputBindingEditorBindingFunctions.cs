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
        view.Filter = item => item is InputBindingRow row && (query.Length == 0 || row.Label.Contains(query, StringComparison.CurrentCultureIgnoreCase) || row.Binding.Contains(query, StringComparison.CurrentCultureIgnoreCase));
    }

    private void ValidateBindings()
    {
        var duplicates = _rows.Where(row => !string.IsNullOrWhiteSpace(row.Binding) && _rows.Any(other => !ReferenceEquals(row, other)
            && !string.IsNullOrWhiteSpace(other.Binding)
            && !(IsIdentityKeyboardBinding(row) && IsIdentityKeyboardBinding(other))
            && InputBindingSyntax.Conflicts(row.Binding, other.Binding))).ToHashSet();
        foreach (var row in _rows)
        {
            if (string.IsNullOrWhiteSpace(row.Binding)) row.SetState(InputBindingState.Unassigned);
            else if (!IsIdentityKeyboardBinding(row) && (!TryValidateBinding(row.Binding, out var reserved) || reserved)) row.SetState(InputBindingState.Reserved);
            else if (duplicates.Contains(row)) row.SetState(InputBindingState.Conflict);
            else row.SetState(InputBindingState.Valid);
        }
        BindingsList.Items.Refresh();
        RefreshAnalogSelections();
    }

    private bool IsIdentityKeyboardBinding(InputBindingRow row) =>
        _allowIdentityKeyboardBindings && _captureSources.HasFlag(InputCaptureSources.Keyboard)
        && Enum.TryParse<GWGUI.Emulation.Enums.EmulationKey>(row.Id, out var key)
        && key != GWGUI.Emulation.Enums.EmulationKey.Unknown
        && InputBindingSyntax.Conflicts(row.Binding, row.Id);

    private bool TryValidateBinding(string value, out bool reserved)
    {
        reserved = false;
        var trimmed = value.Trim();
        reserved = _reservedBindings.Any(binding => InputBindingSyntax.Conflicts(trimmed, binding));
        var parts = trimmed.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var keyboardParts = new List<string>();
        var hasPhysicalSource = false;
        foreach (var part in parts)
        {
            if (part.StartsWith(InputBindingSyntaxConstants.MousePrefix, StringComparison.OrdinalIgnoreCase))
            {
                if (!_captureSources.HasFlag(InputCaptureSources.Mouse) || !EmulationKeyboardBindingFunctions.IsValid(part)) return false;
                hasPhysicalSource = true;
            }
            else if (part.StartsWith(InputBindingSyntaxConstants.ControllerPrefix, StringComparison.OrdinalIgnoreCase))
            {
                if (!_captureSources.HasFlag(InputCaptureSources.Controller) || !EmulationKeyboardBindingFunctions.IsValid(part)) return false;
                hasPhysicalSource = true;
            }
            else keyboardParts.Add(InputBindingSyntax.TryRemovePrefix(part, InputBindingSyntaxConstants.KeyboardPrefix, out var source) ? source : part);
        }
        if (hasPhysicalSource && keyboardParts.Count == 0) return true;
        if (ControllerInputConstants.LegacyButtonNames.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
            return _captureSources.HasFlag(InputCaptureSources.Controller);
        var keyboard = string.Join("+", keyboardParts);
        if (!_captureSources.HasFlag(InputCaptureSources.Keyboard) || !KeyboardChordFunctions.TryParse(keyboard, out var chord)) return false;
        reserved |= KeyboardChordFunctions.IsWindowsReserved(chord);
        return true;
    }

    private static string NormalizedKeyboardBinding(string binding)
    {
        var keyboard = InputBindingSyntax.TryRemovePrefix(binding, InputBindingSyntaxConstants.KeyboardPrefix, out var source) ? source : binding;
        return KeyboardChordFunctions.TryParse(keyboard, out var chord)
            ? KeyboardChordFunctions.Format(chord.Modifiers, chord.Keys.Where(key => !KeyboardChordFunctions.IsModifierKey(key)).OrderBy(key => key))
            : binding.Trim();
    }
}
