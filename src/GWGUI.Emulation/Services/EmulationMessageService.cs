using System.Reflection;

namespace GWGUI.Emulation.Services;

/// <summary>Relie le contrat des messages aux règles de contextualisation existantes.</summary>
internal sealed class EmulationMessageService : IEmulationMessages
{
    /// <inheritdoc/>
    public IEmulationCoreDiagnosticMonitor CreateCoreDiagnosticMonitor(IReadOnlyList<string>? startupFailureDiagnostics, Func<string, Exception> createFailure)
        => new EmulationCoreDiagnosticMonitor(startupFailureDiagnostics, createFailure);

    /// <inheritdoc/>
    public Exception WithNativeDiagnostics(Exception error, IReadOnlyList<string> diagnostics)
        => EmulationErrorService.WithNativeDiagnostics(error, diagnostics);

    /// <inheritdoc/>
    public IEmulationModuleLocalization CreateLocalization(Assembly assembly, string resourcePrefix) => new EmulationModuleLocalization(assembly, resourcePrefix);

    /// <inheritdoc/>
    public EmulationMessageException EmulatorNotInstalled(string emulatorId) => EmulationErrorService.EmulatorNotInstalled(emulatorId);
    /// <inheritdoc/>
    public EmulationMessageException CreateError(EmulationMessageCategory category, EmulationMessageCode code, Exception? innerException) => EmulationErrorService.CreateError(category, code, innerException);

    /// <inheritdoc/>
    public Exception Translate(Exception error, EmulationMessageCategory fallbackCategory, EmulationMessageCode fallbackCode, IEmulationMessageContext? context = null) => EmulationErrorService.Translate(error, fallbackCategory, fallbackCode, context);
    /// <inheritdoc/>
    public Exception TranslateLocalized(Exception error, EmulationMessageCategory category, IEmulationMessageContext? context = null) => EmulationErrorService.TranslateLocalized(error, category, context);
    /// <inheritdoc/>
    public Exception RequiredMediaMissing(string machineId, IReadOnlyList<EmulationMediaCategory> requiredMedia, Exception? innerException) => EmulationErrorService.RequiredMediaMissing(machineId, requiredMedia, innerException);
}
