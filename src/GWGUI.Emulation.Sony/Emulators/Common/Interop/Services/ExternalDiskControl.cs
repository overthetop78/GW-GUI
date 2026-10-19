using GWGUI.Emulation.Sony.Emulators.Common.Interop.Exceptions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;

internal sealed class ExternalDiskControl(string? emulatorId = null) : GWGUI.Emulation.Services.ExternalCoreDiskControl
{
    protected override string UnavailableMessage => CoreExceptions.ForEmulator(emulatorId, CoreExceptionsResourceConstants.DiskControlUnavailable);
    protected override string IncompleteMessage => CoreExceptions.ForEmulator(emulatorId, CoreExceptionsResourceConstants.DiskControlIncomplete);
    protected override string EjectFailedMessage => CoreExceptions.ForEmulator(emulatorId, CoreExceptionsResourceConstants.MediaEjectFailed);
    protected override string SelectFailedMessage => CoreExceptions.ForEmulator(emulatorId, CoreExceptionsResourceConstants.MediaSelectionFailed);
    protected override string InsertFailedMessage => CoreExceptions.ForEmulator(emulatorId, CoreExceptionsResourceConstants.MediaInsertFailed);
    protected override string CreateSlotFailedMessage => CoreExceptions.ForEmulator(emulatorId, CoreExceptionsResourceConstants.MediaSlotCreationFailed);
    protected override string ReplaceFailedMessage => CoreExceptions.ForEmulator(emulatorId, CoreExceptionsResourceConstants.MediaRefused);
    protected override string MediaMissingMessage => CoreExceptions.ForEmulator(emulatorId, CoreExceptionsResourceConstants.MediaNotFound);
    protected override string RequestedSelectFailedMessage => CoreExceptions.ForEmulator(emulatorId, CoreExceptionsResourceConstants.RequestedDiskSelectionFailed);
    protected override string RequestedInsertFailedMessage => CoreExceptions.ForEmulator(emulatorId, CoreExceptionsResourceConstants.RequestedMediaInsertFailed);
}
