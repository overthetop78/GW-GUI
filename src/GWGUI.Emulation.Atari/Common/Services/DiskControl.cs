using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Atari.Common.Services;

internal sealed class DiskControl
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
    internal int ImageCount => IsAvailable ? checked((int)_getImageCount!()) :
        DiskControlConstants.FirstImageIndex;
    internal int CurrentIndex
    {
        get
        {
            if (!IsAvailable) return DiskControlConstants.NoImageIndex;
            var index = _getImageIndex!();
            return index == DiskControlConstants.NoNativeImageIndex
                ? DiskControlConstants.NoImageIndex
                : checked((int)index);
        }
    }
    internal bool IsEjected => !IsAvailable || _getEjectState!();

    internal void Capture(nint data) => CaptureBasic(Marshal.PtrToStructure<ExternalCoreApi.DiskControl>(data));

    internal void CaptureExtended(nint data)
    {
        var api = Marshal.PtrToStructure<ExternalCoreApi.DiskControlExtended>(data);
        CaptureBasic(api.Basic);
        _getImagePath = DiskControlFunctions.OptionalDelegate<ExternalCoreApi.GetImagePath>(api.GetImagePath);
        _getImageLabel = DiskControlFunctions.OptionalDelegate<ExternalCoreApi.GetImageLabel>(api.GetImageLabel);
    }

    internal void Select(int index)
    {
        EnsureAvailable();
        if (index < DiskControlConstants.FirstImageIndex || index >= ImageCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        var previousIndex = _getImageIndex!();
        var wasEjected = _getEjectState!();
        if (!wasEjected && !_setEjectState!(true)) throw new InvalidOperationException(DiskControlErrors.EjectFailed);
        try
        {
            if (!_setImageIndex!((uint)index)) throw new InvalidOperationException(DiskControlErrors.SelectFailed);
            if (!_setEjectState!(false)) throw new InvalidOperationException(DiskControlErrors.InsertFailed);
        }
        catch
        {
            if (previousIndex != DiskControlConstants.NoNativeImageIndex) _setImageIndex!(previousIndex);
            if (!wasEjected) _setEjectState!(false);
            throw;
        }
    }

    internal void Insert(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException(DiskControlErrors.MediaMissing, path);
        EnsureAvailable();
        var wasEjected = _getEjectState!();
        if (!wasEjected && !_setEjectState!(true)) throw new InvalidOperationException(DiskControlErrors.EjectFailed);
        var count = _getImageCount!();
        var index = count == DiskControlConstants.FirstNativeImageIndex
            ? DiskControlConstants.FirstNativeImageIndex
            : Math.Min(_getImageIndex!(), count - DiskControlConstants.PreviousImageOffset);
        if (count == DiskControlConstants.FirstNativeImageIndex && !_addImage!())
            throw new InvalidOperationException(DiskControlErrors.CreateSlotFailed);

        using var content = ContentFunctions.Create(path, true,
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                Path.GetExtension(path).TrimStart(CommonConstants.ExtensionPrefix)
            });
        var inserted = false;
        try
        {
            if (!_replaceImage!(index, content.GameInfo))
                throw new InvalidOperationException(DiskControlErrors.ReplaceFailed);
            if (!_setImageIndex!(index)) throw new InvalidOperationException(DiskControlErrors.SelectFailed);
            if (!_setEjectState!(false)) throw new InvalidOperationException(DiskControlErrors.InsertFailed);
            inserted = true;
        }
        finally
        {
            if (!inserted && !wasEjected) _setEjectState!(false);
        }
    }

    internal void Eject()
    {
        EnsureAvailable();
        if (!_setEjectState!(true)) throw new InvalidOperationException(DiskControlErrors.EjectFailed);
    }

    internal string? GetPath(int index) => DiskControlFunctions.ReadText(_getImagePath, index);
    internal string? GetLabel(int index) => DiskControlFunctions.ReadText(_getImageLabel, index);

    internal DiskStatus GetStatus()
    {
        var images = Enumerable.Range(DiskControlConstants.FirstImageIndex, ImageCount)
            .Select(index => new DiskImageStatus(index, GetPath(index), GetLabel(index)))
            .ToArray();
        return new DiskStatus(ImageCount, CurrentIndex, IsEjected, images);
    }

    private void CaptureBasic(ExternalCoreApi.DiskControl api)
    {
        _setEjectState = DiskControlFunctions.RequiredDelegate<ExternalCoreApi.SetEjectState>(api.SetEjectState);
        _getEjectState = DiskControlFunctions.RequiredDelegate<ExternalCoreApi.GetEjectState>(api.GetEjectState);
        _getImageIndex = DiskControlFunctions.RequiredDelegate<ExternalCoreApi.GetImageIndex>(api.GetImageIndex);
        _setImageIndex = DiskControlFunctions.RequiredDelegate<ExternalCoreApi.SetImageIndex>(api.SetImageIndex);
        _getImageCount = DiskControlFunctions.RequiredDelegate<ExternalCoreApi.GetImageCount>(api.GetImageCount);
        _replaceImage = DiskControlFunctions.RequiredDelegate<ExternalCoreApi.ReplaceImage>(api.ReplaceImage);
        _addImage = DiskControlFunctions.RequiredDelegate<ExternalCoreApi.AddImage>(api.AddImage);
    }

    private void EnsureAvailable()
    {
        if (!IsAvailable) throw new InvalidOperationException(DiskControlErrors.Unavailable);
    }
}
