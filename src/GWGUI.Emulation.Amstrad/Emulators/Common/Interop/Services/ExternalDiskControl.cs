using GWGUI.Emulation.Amstrad.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Amstrad.Emulators.Common.Exceptions;
using GWGUI.Emulation.Amstrad.Emulators.Common.Contracts;
using GWGUI.Emulation.Amstrad.Emulators.Common.Services;

using System.IO;
using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Amstrad.Emulators.Common.Interop.Services;

internal sealed class ExternalDiskControl
{
    private ExternalCoreApi.SetEjectState? _setEjectState;
    private ExternalCoreApi.GetEjectState? _getEjectState;
    private ExternalCoreApi.GetImageIndex? _getImageIndex;
    private ExternalCoreApi.SetImageIndex? _setImageIndex;
    private ExternalCoreApi.GetImageCount? _getImageCount;
    private ExternalCoreApi.ReplaceImage? _replaceImage;
    private ExternalCoreApi.AddImage? _addImage;
    private ExternalCoreApi.GetImagePath? _getImagePath;
    private ExternalCoreApi.GetImageLabel? _getImageLabel;

    internal bool IsAvailable => _setEjectState is not null;

    internal void Capture(nint data)
    {
        var api = Marshal.PtrToStructure<ExternalCoreApi.DiskControl>(data);
        _setEjectState = Delegate<ExternalCoreApi.SetEjectState>(api.SetEjectState);
        _getEjectState = Delegate<ExternalCoreApi.GetEjectState>(api.GetEjectState);
        _getImageIndex = Delegate<ExternalCoreApi.GetImageIndex>(api.GetImageIndex);
        _setImageIndex = Delegate<ExternalCoreApi.SetImageIndex>(api.SetImageIndex);
        _getImageCount = Delegate<ExternalCoreApi.GetImageCount>(api.GetImageCount);
        _replaceImage = Delegate<ExternalCoreApi.ReplaceImage>(api.ReplaceImage);
        _addImage = Delegate<ExternalCoreApi.AddImage>(api.AddImage);
    }

    internal void CaptureExtended(nint data)
    {
        var api = Marshal.PtrToStructure<ExternalCoreApi.DiskControlExtended>(data);
        CaptureBasic(api.Basic);
        _getImagePath = OptionalDelegate<ExternalCoreApi.GetImagePath>(api.GetImagePath);
        _getImageLabel = OptionalDelegate<ExternalCoreApi.GetImageLabel>(api.GetImageLabel);
    }

    private void CaptureBasic(ExternalCoreApi.DiskControl api)
    {
        _setEjectState = Delegate<ExternalCoreApi.SetEjectState>(api.SetEjectState);
        _getEjectState = Delegate<ExternalCoreApi.GetEjectState>(api.GetEjectState);
        _getImageIndex = Delegate<ExternalCoreApi.GetImageIndex>(api.GetImageIndex);
        _setImageIndex = Delegate<ExternalCoreApi.SetImageIndex>(api.SetImageIndex);
        _getImageCount = Delegate<ExternalCoreApi.GetImageCount>(api.GetImageCount);
        _replaceImage = Delegate<ExternalCoreApi.ReplaceImage>(api.ReplaceImage);
        _addImage = Delegate<ExternalCoreApi.AddImage>(api.AddImage);
    }

    internal int ImageCount => IsAvailable ? checked((int)_getImageCount!()) : BufferConstants.EmptyCollectionCount;
    internal int CurrentIndex
    {
        get
        {
            if (!IsAvailable) return ProcessCoreConstants.NoSelectedDiskIndex;
            var index = _getImageIndex!();
            return index == uint.MaxValue ? ProcessCoreConstants.NoSelectedDiskIndex : checked((int)index);
        }
    }

    internal void Select(int index)
    {
        EnsureAvailable();
        var count = ImageCount;
        if (index < BufferConstants.FirstCollectionIndex || index >= count) throw new ArgumentOutOfRangeException(nameof(index));
        var previousIndex = _getImageIndex!();
        var wasEjected = _getEjectState!();
        if (!wasEjected && !_setEjectState!(true))
            throw new InvalidOperationException(CommonExceptions.MediaEjectFailed());
        try
        {
            if (!_setImageIndex!((uint)index))
                throw new InvalidOperationException(CommonExceptions.RequestedDiskSelectionFailed());
            if (!_setEjectState!(false))
                throw new InvalidOperationException(CommonExceptions.RequestedMediaInsertFailed());
        }
        catch
        {
            if (previousIndex != uint.MaxValue) _setImageIndex!(previousIndex);
            if (!wasEjected) _setEjectState!(false);
            throw;
        }
    }

    internal string? GetPath(int index) => ReadText(_getImagePath, index);
    internal string? GetLabel(int index) => ReadText(_getImageLabel, index);

    private static string? ReadText<T>(T? getter, int index) where T : Delegate
    {
        if (getter is null || index < BufferConstants.FirstCollectionIndex) return null;
        var buffer = Marshal.AllocHGlobal(ExternalCoreInteropConstants.DiskMetadataBufferSize);
        try
        {
            var success = getter switch
            {
                ExternalCoreApi.GetImagePath path => path((uint)index, buffer, ExternalCoreInteropConstants.DiskMetadataBufferSize),
                ExternalCoreApi.GetImageLabel label => label((uint)index, buffer, ExternalCoreInteropConstants.DiskMetadataBufferSize),
                _ => false
            };
            return success ? Marshal.PtrToStringUTF8(buffer) : null;
        }
        finally { Marshal.FreeHGlobal(buffer); }
    }

    internal void Insert(string path)
    {
        if (!File.Exists(path) && !Directory.Exists(path)) throw new FileNotFoundException(CommonExceptions.MediaNotFound(), path);
        EnsureAvailable();
        var wasEjected = _getEjectState!();
        if (!wasEjected && !_setEjectState!(true)) throw new InvalidOperationException(CommonExceptions.MediaEjectFailed());
        var count = _getImageCount!();
        var index = count == ExternalCoreInteropConstants.EmptyNativeCollectionCount ? ExternalCoreInteropConstants.EmptyNativeCollectionCount : Math.Min(_getImageIndex!(), count - BufferConstants.IndexIncrement);
        if (count == ExternalCoreInteropConstants.EmptyNativeCollectionCount && !_addImage!()) throw new InvalidOperationException(CommonExceptions.MediaSlotCreationFailed());

        var nativePath = Marshal.StringToCoTaskMemUTF8(Path.GetFullPath(path));
        var game = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.GameInfo>());
        var inserted = false;
        try
        {
            Marshal.StructureToPtr(new ExternalCoreApi.GameInfo { Path = nativePath }, game, false);
            if (!_replaceImage!(index, game)) throw new InvalidOperationException(CommonExceptions.MediaRefused());
            if (!_setImageIndex!(index)) throw new InvalidOperationException(CommonExceptions.MediaSelectionFailed());
            if (!_setEjectState!(false)) throw new InvalidOperationException(CommonExceptions.MediaInsertFailed());
            inserted = true;
        }
        finally
        {
            Marshal.FreeHGlobal(game);
            Marshal.FreeCoTaskMem(nativePath);
            if (!inserted && !wasEjected) _setEjectState!(false);
        }
    }

    internal void Eject()
    {
        EnsureAvailable();
        if (!_setEjectState!(true)) throw new InvalidOperationException(CommonExceptions.MediaEjectFailed());
    }

    private void EnsureAvailable()
    {
        if (!IsAvailable) throw new InvalidOperationException(CommonExceptions.DiskControlUnavailable());
    }

    private static T Delegate<T>(nint pointer) where T : Delegate
    {
        if (pointer == nint.Zero) throw new InvalidOperationException(CommonExceptions.DiskControlIncomplete());
        return Marshal.GetDelegateForFunctionPointer<T>(pointer);
    }

    private static T? OptionalDelegate<T>(nint pointer) where T : Delegate =>
        pointer == nint.Zero ? null : Marshal.GetDelegateForFunctionPointer<T>(pointer);
}
