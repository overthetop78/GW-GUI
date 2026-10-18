using GWGUI.Emulation.Nec.Emulators.Common.Interop.Exceptions;

namespace GWGUI.Emulation.Nec.Emulators.Common.Interop.Services;

internal sealed class ExternalDiskControl(string errorPrefix = "Emulation.Error.Core") : GWGUI.Emulation.Services.ExternalCoreDiskControl
{
    private string Error(string name, params object[] arguments) => CoreExceptions.Text(errorPrefix + "." + name, arguments);

    protected override string UnavailableMessage => Error("DiskControlUnavailable");
    protected override string IncompleteMessage => Error("DiskControlIncomplete");
    protected override string EjectFailedMessage => Error("MediaEjectFailed");
    protected override string SelectFailedMessage => Error("MediaSelectionFailed");
    protected override string InsertFailedMessage => Error("MediaInsertFailed");
    protected override string CreateSlotFailedMessage => Error("MediaSlotCreationFailed");
    protected override string ReplaceFailedMessage => Error("MediaRefused");
    protected override string MediaMissingMessage => Error("MediaNotFound");
    protected override string RequestedSelectFailedMessage => Error("RequestedDiskSelectionFailed");
    protected override string RequestedInsertFailedMessage => Error("RequestedMediaInsertFailed");
}
