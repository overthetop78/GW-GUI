using GWGUI.App.Services.Input.GameInput;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GWGUI.App.Views.Controls.Options.ControllerPresentation;

internal sealed class ControllerInputRow(GameInputControlValue control) : INotifyPropertyChanged
{
    internal (GameInputControlType Type, int Index) Key { get; } = (control.Type, control.Index);
    public string Name { get; private set; } = GameInputDisplayFormatter.ControlName(control);
    public string Value { get; private set; } = GameInputDisplayFormatter.ControlValue(control);
    public bool Active { get; private set; } = control.IsPressed;

    internal void RefreshLabel(GameInputControlValue control)
    {
        Name = GameInputDisplayFormatter.ControlName(control);
        OnPropertyChanged(nameof(Name));
        Update(control);
    }

    internal void Update(GameInputControlValue control)
    {
        var value = GameInputDisplayFormatter.ControlValue(control);
        if (!string.Equals(Value, value, StringComparison.Ordinal))
        {
            Value = value;
            OnPropertyChanged(nameof(Value));
        }
        var active = control.IsPressed;
        if (Active != active)
        {
            Active = active;
            OnPropertyChanged(nameof(Active));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
