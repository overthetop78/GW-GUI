using GWGUI.Emulation.Amstrad.Emulators.Common.Exceptions;

namespace GWGUI.Emulation.Amstrad.Emulators.Common.Interop.Services;

internal sealed class ExternalDiskControl : GWGUI.Emulation.Services.ExternalCoreDiskControl
{
    protected override string UnavailableMessage => CommonExceptions.DiskControlUnavailable();
    protected override string IncompleteMessage => CommonExceptions.DiskControlIncomplete();
    protected override string EjectFailedMessage => CommonExceptions.MediaEjectFailed();
    protected override string SelectFailedMessage => CommonExceptions.MediaSelectionFailed();
    protected override string InsertFailedMessage => CommonExceptions.MediaInsertFailed();
    protected override string CreateSlotFailedMessage => CommonExceptions.MediaSlotCreationFailed();
    protected override string ReplaceFailedMessage => CommonExceptions.MediaRefused();
    protected override string MediaMissingMessage => CommonExceptions.MediaNotFound();
    protected override string RequestedSelectFailedMessage => CommonExceptions.RequestedDiskSelectionFailed();
    protected override string RequestedInsertFailedMessage => CommonExceptions.RequestedMediaInsertFailed();
}
