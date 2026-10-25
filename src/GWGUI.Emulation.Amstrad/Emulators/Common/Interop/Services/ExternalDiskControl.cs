using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Amstrad.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Amstrad.Common.Exceptions;

namespace GWGUI.Emulation.Amstrad.Emulators.Common.Interop.Services;

internal sealed class ExternalDiskControl : IEmulationDiskControl, IEmulationDiskControlPolicy
{
    private readonly IEmulationDiskControl _control;

    internal ExternalDiskControl()
    {
        _control = CommonCoreHostFunctions.NativeResources.CreateDiskControl(this);
    }

    public string UnavailableMessage => ErrorMessages.DiskControlUnavailable();
    public string IncompleteMessage => ErrorMessages.DiskControlIncomplete();
    public string EjectFailedMessage => ErrorMessages.MediaEjectFailed();
    public string SelectFailedMessage => ErrorMessages.MediaSelectionFailed();
    public string InsertFailedMessage => ErrorMessages.MediaInsertFailed();
    public string CreateSlotFailedMessage => ErrorMessages.MediaSlotCreationFailed();
    public string ReplaceFailedMessage => ErrorMessages.MediaRefused();
    public string MediaMissingMessage => ErrorMessages.MediaNotFound();
    public string RequestedSelectFailedMessage => ErrorMessages.RequestedDiskSelectionFailed();
    public string RequestedInsertFailedMessage => ErrorMessages.RequestedMediaInsertFailed();

    /// <summary>État et opérations transmis au propriétaire SDK par son interface.</summary>
    public bool IsAvailable => _control.IsAvailable;
    public int ImageCount => _control.ImageCount;
    public int CurrentIndex => _control.CurrentIndex;
    public bool IsEjected => _control.IsEjected;
    public void Capture(nint data) => _control.Capture(data);
    public void CaptureExtended(nint data) => _control.CaptureExtended(data);
    public void Clear() => _control.Clear();
    public void Select(int index) => _control.Select(index);
    public string? GetPath(int index) => _control.GetPath(index);
    public string? GetLabel(int index) => _control.GetLabel(index);
    public DiskStatus GetStatus() => _control.GetStatus();
    public void Insert(string path) => _control.Insert(path);
    public void InsertAt(int index, string path, int firstImageIndex, Func<string, IEmulationLoadedContent> createContent) => _control.InsertAt(index, path, firstImageIndex, createContent);
    public void Eject() => _control.Eject();
}
