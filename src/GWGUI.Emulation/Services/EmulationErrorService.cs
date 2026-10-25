namespace GWGUI.Emulation.Services;

internal static class EmulationErrorService
{
    /// <summary>Ajoute une fois le bloc ordonné de messages, en conservant le texte déjà fourni.</summary>
    internal static string AppendNativeDiagnostics(string message, IReadOnlyList<string> diagnostics)
    {
        if (diagnostics.Count == 0) return message;
        var block = string.Join(Environment.NewLine, diagnostics);
        return message.Contains(block, StringComparison.Ordinal) ? message : message + Environment.NewLine + block;
    }

    /// <summary>Conserve la cause initiale et ne crée une enveloppe que pour un bloc absent du message.</summary>
    internal static Exception WithNativeDiagnostics(Exception error, IReadOnlyList<string> diagnostics)
    {
        var message = AppendNativeDiagnostics(error.Message, diagnostics);
        return message == error.Message ? error : new InvalidOperationException(message, error);
    }

    internal static EmulationMessageException EmulatorNotInstalled(string emulatorId) =>
        new(new EmulationMessage(EmulationMessageCategory.Emulator, EmulationMessageCode.EmulatorNotInstalled,
            EmulationMessageSeverity.Error, EmulationMessageTarget.Dialog,
            new EmulationEmulatorMessageContext(emulatorId)));

    internal static EmulationMessageException RequiredMediaMissing(string machineId,
        IReadOnlyList<EmulationMediaCategory> requiredMedia, Exception? innerException) =>
        new EmulationMessageException(new EmulationMessage(
            EmulationMessageCategory.Media,
            EmulationMessageCode.RequiredMediaMissing,
            EmulationMessageSeverity.Error,
            EmulationMessageTarget.Dialog,
            new EmulationRequiredMachineMediaMessageContext(machineId, requiredMedia)), innerException);

    internal static EmulationMessageException CreateError(EmulationMessageCategory category,
        EmulationMessageCode code, Exception? innerException) =>
        new(new EmulationMessage(category, code, EmulationMessageSeverity.Error, EmulationMessageTarget.Dialog), innerException);

    public static Exception Translate(Exception error, EmulationMessageCategory fallbackCategory, EmulationMessageCode fallbackCode, IEmulationMessageContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(error);
        if (error is OperationCanceledException or EmulationMessageException) return error;
        return new EmulationMessageException(new EmulationMessage(fallbackCategory, fallbackCode, EmulationMessageSeverity.Error, EmulationMessageTarget.Dialog, context), error);
    }

    public static Exception TranslateLocalized(Exception error, EmulationMessageCategory category, IEmulationMessageContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(error);
        if (error is OperationCanceledException or EmulationMessageException) return error;
        return new EmulationMessageException(new EmulationMessage(category, EmulationMessageCode.UntranslatedEmulatorMessage, EmulationMessageSeverity.Error, EmulationMessageTarget.Dialog, context, error.Message), error);
    }
}
