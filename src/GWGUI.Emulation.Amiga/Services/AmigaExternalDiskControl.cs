using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Amiga.Services;

internal sealed class AmigaExternalDiskControl
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

    internal int ImageCount => IsAvailable ? checked((int)_getImageCount!()) : 0;
    internal int CurrentIndex
    {
        get
        {
            if (!IsAvailable) return -1;
            var index = _getImageIndex!();
            return index == uint.MaxValue ? -1 : checked((int)index);
        }
    }

    internal void Select(int index)
    {
        EnsureAvailable();
        var count = ImageCount;
        if (index < 0 || index >= count) throw new ArgumentOutOfRangeException(nameof(index));
        var previousIndex = _getImageIndex!();
        var wasEjected = _getEjectState!();
        if (!wasEjected && !_setEjectState!(true))
            throw new InvalidOperationException(AmigaExternalDiskControlConstants.TheAmigaMediaDriveCouldNotBeEjected);
        try
        {
            if (!_setImageIndex!((uint)index))
                throw new InvalidOperationException(AmigaExternalDiskControlConstants.TheAmigaCoreCouldNotSelectTheRequestedDisk);
            if (!_setEjectState!(false))
                throw new InvalidOperationException(AmigaExternalDiskControlConstants.TheAmigaMediaDriveCouldNotInsertTheRequestedImage);
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
        if (getter is null || index < 0) return null;
        var buffer = Marshal.AllocHGlobal(4096);
        try
        {
            var success = getter switch
            {
                ExternalCoreApi.GetImagePath path => path((uint)index, buffer, 4096),
                ExternalCoreApi.GetImageLabel label => label((uint)index, buffer, 4096),
                _ => false
            };
            return success ? Marshal.PtrToStringUTF8(buffer) : null;
        }
        finally { Marshal.FreeHGlobal(buffer); }
    }

    internal void Insert(string path)
    {
        if (!File.Exists(path) && !Directory.Exists(path)) throw new FileNotFoundException(AmigaExternalDiskControlConstants.TheAmigaMediaImageOrDirectoryWasNotFound, path);
        EnsureAvailable();
        var wasEjected = _getEjectState!();
        if (!wasEjected && !_setEjectState!(true)) throw new InvalidOperationException(AmigaExternalDiskControlConstants.TheAmigaMediaDriveCouldNotBeEjected);
        var count = _getImageCount!();
        var index = count == 0 ? 0u : Math.Min(_getImageIndex!(), count - 1);
        if (count == 0 && !_addImage!()) throw new InvalidOperationException(AmigaExternalDiskControlConstants.TheAmigaCoreCouldNotCreateAMediaSlot);

        var nativePath = Marshal.StringToCoTaskMemUTF8(Path.GetFullPath(path));
        var game = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.GameInfo>());
        var inserted = false;
        try
        {
            Marshal.StructureToPtr(new ExternalCoreApi.GameInfo { Path = nativePath }, game, false);
            if (!_replaceImage!(index, game)) throw new InvalidOperationException(AmigaExternalDiskControlConstants.TheAmigaCoreRefusedTheMediaImage);
            if (!_setImageIndex!(index)) throw new InvalidOperationException(AmigaExternalDiskControlConstants.TheAmigaCoreCouldNotSelectTheMediaImage);
            if (!_setEjectState!(false)) throw new InvalidOperationException(AmigaExternalDiskControlConstants.TheAmigaMediaDriveCouldNotInsertTheImage);
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
        if (!_setEjectState!(true)) throw new InvalidOperationException(AmigaExternalDiskControlConstants.TheAmigaMediaDriveCouldNotBeEjected);
    }

    private void EnsureAvailable()
    {
        if (!IsAvailable) throw new InvalidOperationException(AmigaExternalDiskControlConstants.TheAmigaCoreHasNotProvidedDiskControl);
    }

    private static T Delegate<T>(nint pointer) where T : Delegate
    {
        if (pointer == 0) throw new InvalidOperationException(AmigaExternalDiskControlConstants.TheAmigaCoreProvidedIncompleteDiskControl);
        return Marshal.GetDelegateForFunctionPointer<T>(pointer);
    }

    private static T? OptionalDelegate<T>(nint pointer) where T : Delegate =>
        pointer == 0 ? null : Marshal.GetDelegateForFunctionPointer<T>(pointer);
}
