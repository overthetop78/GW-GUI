namespace GWGUI.Emulation.Services;

public static class EmulationErrorService
{
    public static Exception Translate(Exception error,
        EmulationMessageCategory fallbackCategory,
        EmulationMessageCode fallbackCode,
        IEmulationMessageContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(error);
        if (error is OperationCanceledException or EmulationMessageException) return error;
        return new EmulationMessageException(new EmulationMessage(
            fallbackCategory,
            fallbackCode,
            EmulationMessageSeverity.Error,
            EmulationMessageTarget.Dialog,
            context), error);
    }

    public static Exception TranslateLocalized(Exception error,
        EmulationMessageCategory category,
        IEmulationMessageContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(error);
        if (error is OperationCanceledException or EmulationMessageException) return error;
        return new EmulationMessageException(new EmulationMessage(
            category,
            EmulationMessageCode.UntranslatedEmulatorMessage,
            EmulationMessageSeverity.Error,
            EmulationMessageTarget.Dialog,
            context,
            error.Message), error);
    }
}
