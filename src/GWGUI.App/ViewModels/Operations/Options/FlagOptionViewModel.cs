using GWGUI.Domain.Commands.Options;
namespace GWGUI.App.ViewModels.Operations.Options;

public sealed class FlagOptionViewModel(string argument) : OperationOptionViewModelBase(argument)
{
    public override EnabledOption ToEnabledOption() => new(Argument);
}
