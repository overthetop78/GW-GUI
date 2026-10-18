using GWGUI.Emulation.Sony.Emulators.Common.Interop.Exceptions;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;

internal sealed class ExternalDiskControl : GWGUI.Emulation.Services.ExternalCoreDiskControl
{
    protected override string UnavailableMessage => CoreExceptions.DiskControlUnavailable();
    protected override string IncompleteMessage => CoreExceptions.DiskControlIncomplete();
    protected override string EjectFailedMessage => CoreExceptions.MediaEjectFailed();
    protected override string SelectFailedMessage => CoreExceptions.MediaSelectionFailed();
    protected override string InsertFailedMessage => CoreExceptions.MediaInsertFailed();
    protected override string CreateSlotFailedMessage => CoreExceptions.MediaSlotCreationFailed();
    protected override string ReplaceFailedMessage => CoreExceptions.MediaRefused();
    protected override string MediaMissingMessage => CoreExceptions.MediaNotFound();
    protected override string RequestedSelectFailedMessage => CoreExceptions.RequestedDiskSelectionFailed();
    protected override string RequestedInsertFailedMessage => CoreExceptions.RequestedMediaInsertFailed();
}
