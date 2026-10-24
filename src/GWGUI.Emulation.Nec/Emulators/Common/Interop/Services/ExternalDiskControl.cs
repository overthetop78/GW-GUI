using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Nec.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Nec.Common.Exceptions;

namespace GWGUI.Emulation.Nec.Emulators.Common.Interop.Services;

internal sealed class ExternalDiskControl : IEmulationDiskControl, IEmulationDiskControlPolicy
{
    private readonly IEmulationDiskControl _control;
    private readonly string _errorPrefix;

    internal ExternalDiskControl(string errorPrefix = "Emulation.Error.Core")
    {
        _errorPrefix = errorPrefix;
        _control = CommonCoreHostFunctions.NativeResources.CreateDiskControl(this);
    }

    private string Error(string name, params object[] arguments) => CoreExceptions.Text(_errorPrefix + "." + name, arguments);

    public string UnavailableMessage => Error("DiskControlUnavailable");
    public string IncompleteMessage => Error("DiskControlIncomplete");
    public string EjectFailedMessage => Error("MediaEjectFailed");
    public string SelectFailedMessage => Error("MediaSelectionFailed");
    public string InsertFailedMessage => Error("MediaInsertFailed");
    public string CreateSlotFailedMessage => Error("MediaSlotCreationFailed");
    public string ReplaceFailedMessage => Error("MediaRefused");
    public string MediaMissingMessage => Error("MediaNotFound");
    public string RequestedSelectFailedMessage => Error("RequestedDiskSelectionFailed");
    public string RequestedInsertFailedMessage => Error("RequestedMediaInsertFailed");

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
