using GWGUI.Emulation.Sony.Emulators.Pcsx2.Exceptions;

namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Services;

internal sealed class ExternalDiskControl : GWGUI.Emulation.Services.ExternalCoreDiskControl
{
    protected override string UnavailableMessage => Pcsx2Exceptions.DiskControlUnavailable();
    protected override string IncompleteMessage => Pcsx2Exceptions.DiskControlIncomplete();
    protected override string EjectFailedMessage => Pcsx2Exceptions.MediaEjectFailed();
    protected override string SelectFailedMessage => Pcsx2Exceptions.MediaSelectionFailed();
    protected override string InsertFailedMessage => Pcsx2Exceptions.MediaInsertFailed();
    protected override string CreateSlotFailedMessage => Pcsx2Exceptions.MediaSlotCreationFailed();
    protected override string ReplaceFailedMessage => Pcsx2Exceptions.MediaRefused();
    protected override string MediaMissingMessage => Pcsx2Exceptions.MediaNotFound();
    protected override string RequestedSelectFailedMessage => Pcsx2Exceptions.RequestedDiskSelectionFailed();
    protected override string RequestedInsertFailedMessage => Pcsx2Exceptions.RequestedMediaInsertFailed();
}
