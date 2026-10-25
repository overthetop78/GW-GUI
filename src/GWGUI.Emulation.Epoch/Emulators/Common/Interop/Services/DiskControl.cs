using GWGUI.Emulation.Contracts;
using CommonCoreHostFunctions = GWGUI.Emulation.Epoch.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Epoch.Emulators.Common.Exceptions;
using GWGUI.Emulation.Epoch.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Epoch.Emulators.Common.Interop.Functions;
using System.IO;

namespace GWGUI.Emulation.Epoch.Emulators.Common.Interop.Services;

internal sealed class DiskControl : IEmulationDiskControl, IEmulationDiskControlPolicy
{
    private readonly IEmulationDiskControl _control;

    internal DiskControl()
    {
        _control = CommonCoreHostFunctions.NativeResources.CreateDiskControl(this, CreateContent);
    }

    public string UnavailableMessage => DiskControlErrors.Unavailable;
    public string IncompleteMessage => DiskControlErrors.Incomplete;
    public string EjectFailedMessage => DiskControlErrors.EjectFailed;
    public string SelectFailedMessage => DiskControlErrors.SelectFailed;
    public string InsertFailedMessage => DiskControlErrors.InsertFailed;
    public string CreateSlotFailedMessage => DiskControlErrors.CreateSlotFailed;
    public string ReplaceFailedMessage => DiskControlErrors.ReplaceFailed;
    public string MediaMissingMessage => DiskControlErrors.MediaMissing;
    public bool ResetOnEmptyCapture => true;

    private IEmulationLoadedContent CreateContent(string path)
        => ContentFunctions.Create(path, true,
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                Path.GetExtension(path).TrimStart(MediaConstants.ExtensionPrefix)
            });


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
