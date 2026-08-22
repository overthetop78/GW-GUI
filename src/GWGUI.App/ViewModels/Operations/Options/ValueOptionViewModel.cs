using GWGUI.Domain.Commands.Options;
namespace GWGUI.App.ViewModels.Operations.Options;

public sealed class ValueOptionViewModel(string argument, string initialValue) : OperationOptionViewModelBase(argument)
{
    private string _value = initialValue;
    public string Value { get => _value; set { if (_value == value) return; _value = value; Changed(nameof(Value)); } }
    public override EnabledOption ToEnabledOption() => new(Argument, Value.Trim());
}
