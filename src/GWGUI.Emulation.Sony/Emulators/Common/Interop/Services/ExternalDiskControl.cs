using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Sony.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Exceptions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;

internal sealed class ExternalDiskControl : IEmulationDiskControl, IEmulationDiskControlPolicy
{
    private readonly IEmulationDiskControl _control;
    private readonly string? _emulatorId;

    internal ExternalDiskControl(string? emulatorId = null)
    {
        _emulatorId = emulatorId;
        _control = CommonCoreHostFunctions.NativeResources.CreateDiskControl(this);
    }

    public string UnavailableMessage => CoreExceptions.ForEmulator(_emulatorId, CoreExceptionsResourceConstants.DiskControlUnavailable);
    public string IncompleteMessage => CoreExceptions.ForEmulator(_emulatorId, CoreExceptionsResourceConstants.DiskControlIncomplete);
    public string EjectFailedMessage => CoreExceptions.ForEmulator(_emulatorId, CoreExceptionsResourceConstants.MediaEjectFailed);
    public string SelectFailedMessage => CoreExceptions.ForEmulator(_emulatorId, CoreExceptionsResourceConstants.MediaSelectionFailed);
    public string InsertFailedMessage => CoreExceptions.ForEmulator(_emulatorId, CoreExceptionsResourceConstants.MediaInsertFailed);
    public string CreateSlotFailedMessage => CoreExceptions.ForEmulator(_emulatorId, CoreExceptionsResourceConstants.MediaSlotCreationFailed);
    public string ReplaceFailedMessage => CoreExceptions.ForEmulator(_emulatorId, CoreExceptionsResourceConstants.MediaRefused);
    public string MediaMissingMessage => CoreExceptions.ForEmulator(_emulatorId, CoreExceptionsResourceConstants.MediaNotFound);
    public string RequestedSelectFailedMessage => CoreExceptions.ForEmulator(_emulatorId, CoreExceptionsResourceConstants.RequestedDiskSelectionFailed);
    public string RequestedInsertFailedMessage => CoreExceptions.ForEmulator(_emulatorId, CoreExceptionsResourceConstants.RequestedMediaInsertFailed);

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
