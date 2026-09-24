using EnabledOption = global::GWGUI.MediaEngine.Contracts.Options.EnabledOption;
namespace GWGUI.App.ViewModels.Operations.Options;

public sealed class FlagOptionViewModel(string argument) : OperationOptionViewModelBase(argument)
{
    public override EnabledOption ToEnabledOption() => new(Argument);
}
