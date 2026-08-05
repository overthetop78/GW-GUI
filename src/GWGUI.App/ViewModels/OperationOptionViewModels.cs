using System.ComponentModel;
using GWGUI.Domain.Read;

namespace GWGUI.App.ViewModels;

public abstract class OperationOptionViewModelBase(string argument) : INotifyPropertyChanged
{
    private bool _enabled;
    public string Argument { get; } = argument;
    public bool Enabled { get => _enabled; set { if (_enabled == value) return; _enabled = value; PropertyChanged?.Invoke(this, new(nameof(Enabled))); } }
    public abstract EnabledOption ToEnabledOption();
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void Changed(string propertyName) => PropertyChanged?.Invoke(this, new(propertyName));
}

public sealed class FlagOptionViewModel(string argument) : OperationOptionViewModelBase(argument)
{
    public override EnabledOption ToEnabledOption() => new(Argument);
}

public sealed class ValueOptionViewModel(string argument, string initialValue) : OperationOptionViewModelBase(argument)
{
    private string _value = initialValue;
    public string Value { get => _value; set { if (_value == value) return; _value = value; Changed(nameof(Value)); } }
    public override EnabledOption ToEnabledOption() => new(Argument, Value.Trim());
}
