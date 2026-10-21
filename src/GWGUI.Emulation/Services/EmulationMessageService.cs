namespace GWGUI.Emulation.Services;

/// <summary>Relie le contrat des messages aux règles de contextualisation existantes.</summary>
internal sealed class EmulationMessageService : IEmulationMessages
{
    /// <inheritdoc/>
    public Exception Translate(Exception error, EmulationMessageCategory fallbackCategory, EmulationMessageCode fallbackCode, IEmulationMessageContext? context = null) => EmulationErrorService.Translate(error, fallbackCategory, fallbackCode, context);
    /// <inheritdoc/>
    public Exception TranslateLocalized(Exception error, EmulationMessageCategory category, IEmulationMessageContext? context = null) => EmulationErrorService.TranslateLocalized(error, category, context);
    /// <inheritdoc/>
    public Exception RequiredMediaMissing(string machineId, IReadOnlyList<EmulationMediaCategory> requiredMedia, Exception? innerException) => EmulationErrorService.RequiredMediaMissing(machineId, requiredMedia, innerException);
}
