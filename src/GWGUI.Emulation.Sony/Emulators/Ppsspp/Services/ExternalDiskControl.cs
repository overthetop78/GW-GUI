using GWGUI.Emulation.Sony.Emulators.Ppsspp.Exceptions;

namespace GWGUI.Emulation.Sony.Emulators.Ppsspp.Services;

internal sealed class ExternalDiskControl : GWGUI.Emulation.Services.ExternalCoreDiskControl
{
    protected override string UnavailableMessage => PpssppExceptions.DiskControlUnavailable();
    protected override string IncompleteMessage => PpssppExceptions.DiskControlIncomplete();
    protected override string EjectFailedMessage => PpssppExceptions.MediaEjectFailed();
    protected override string SelectFailedMessage => PpssppExceptions.MediaSelectionFailed();
    protected override string InsertFailedMessage => PpssppExceptions.MediaInsertFailed();
    protected override string CreateSlotFailedMessage => PpssppExceptions.MediaSlotCreationFailed();
    protected override string ReplaceFailedMessage => PpssppExceptions.MediaRefused();
    protected override string MediaMissingMessage => PpssppExceptions.MediaNotFound();
    protected override string RequestedSelectFailedMessage => PpssppExceptions.RequestedDiskSelectionFailed();
    protected override string RequestedInsertFailedMessage => PpssppExceptions.RequestedMediaInsertFailed();
}
