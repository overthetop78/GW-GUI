using GWGUI.Domain.Commands.Options;
using System.ComponentModel;

namespace GWGUI.App.ViewModels.Operations.Options;

public abstract class OperationOptionViewModelBase(string argument) : INotifyPropertyChanged
{
    private bool _enabled;
    public string Argument { get; } = argument;
    public bool Enabled { get => _enabled; set { if (_enabled == value) return; _enabled = value; PropertyChanged?.Invoke(this, new(nameof(Enabled))); } }
    public abstract EnabledOption ToEnabledOption();
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void Changed(string propertyName) => PropertyChanged?.Invoke(this, new(propertyName));
}
