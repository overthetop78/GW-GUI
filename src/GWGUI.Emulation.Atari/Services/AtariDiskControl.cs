using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Atari.Services;

internal sealed class AtariDiskControl
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
        AtariDiskControlConstants.FirstImageIndex;
    internal int CurrentIndex
    {
        get
        {
            if (!IsAvailable) return AtariDiskControlConstants.NoImageIndex;
            var index = _getImageIndex!();
            return index == AtariDiskControlConstants.NoNativeImageIndex
                ? AtariDiskControlConstants.NoImageIndex
                : checked((int)index);
        }
    }
    internal bool IsEjected => !IsAvailable || _getEjectState!();

    internal void Capture(nint data) => CaptureBasic(Marshal.PtrToStructure<ExternalCoreApi.DiskControl>(data));

    internal void CaptureExtended(nint data)
    {
        var api = Marshal.PtrToStructure<ExternalCoreApi.DiskControlExtended>(data);
        CaptureBasic(api.Basic);
        _getImagePath = AtariDiskControlFunctions.OptionalDelegate<ExternalCoreApi.GetImagePath>(api.GetImagePath);
        _getImageLabel = AtariDiskControlFunctions.OptionalDelegate<ExternalCoreApi.GetImageLabel>(api.GetImageLabel);
    }

    internal void Select(int index)
    {
        EnsureAvailable();
        if (index < AtariDiskControlConstants.FirstImageIndex || index >= ImageCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        var previousIndex = _getImageIndex!();
        var wasEjected = _getEjectState!();
        if (!wasEjected && !_setEjectState!(true)) throw new InvalidOperationException(AtariDiskControlErrors.EjectFailed);
        try
        {
            if (!_setImageIndex!((uint)index)) throw new InvalidOperationException(AtariDiskControlErrors.SelectFailed);
            if (!_setEjectState!(false)) throw new InvalidOperationException(AtariDiskControlErrors.InsertFailed);
        }
        catch
        {
            if (previousIndex != AtariDiskControlConstants.NoNativeImageIndex) _setImageIndex!(previousIndex);
            if (!wasEjected) _setEjectState!(false);
            throw;
        }
    }

    internal void Insert(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException(AtariDiskControlErrors.MediaMissing, path);
        EnsureAvailable();
        var wasEjected = _getEjectState!();
        if (!wasEjected && !_setEjectState!(true)) throw new InvalidOperationException(AtariDiskControlErrors.EjectFailed);
        var count = _getImageCount!();
        var index = count == AtariDiskControlConstants.FirstNativeImageIndex
            ? AtariDiskControlConstants.FirstNativeImageIndex
            : Math.Min(_getImageIndex!(), count - AtariDiskControlConstants.PreviousImageOffset);
        if (count == AtariDiskControlConstants.FirstNativeImageIndex && !_addImage!())
            throw new InvalidOperationException(AtariDiskControlErrors.CreateSlotFailed);

        using var content = AtariContentFunctions.Create(path, true,
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                Path.GetExtension(path).TrimStart(AtariConstants.ExtensionPrefix)
            });
        var inserted = false;
        try
        {
            if (!_replaceImage!(index, content.GameInfo))
                throw new InvalidOperationException(AtariDiskControlErrors.ReplaceFailed);
            if (!_setImageIndex!(index)) throw new InvalidOperationException(AtariDiskControlErrors.SelectFailed);
            if (!_setEjectState!(false)) throw new InvalidOperationException(AtariDiskControlErrors.InsertFailed);
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
        if (!_setEjectState!(true)) throw new InvalidOperationException(AtariDiskControlErrors.EjectFailed);
    }

    internal string? GetPath(int index) => AtariDiskControlFunctions.ReadText(_getImagePath, index);
    internal string? GetLabel(int index) => AtariDiskControlFunctions.ReadText(_getImageLabel, index);

    internal AtariDiskStatus GetStatus()
    {
        var images = Enumerable.Range(AtariDiskControlConstants.FirstImageIndex, ImageCount)
            .Select(index => new AtariDiskImageStatus(index, GetPath(index), GetLabel(index)))
            .ToArray();
        return new AtariDiskStatus(ImageCount, CurrentIndex, IsEjected, images);
    }

    private void CaptureBasic(ExternalCoreApi.DiskControl api)
    {
        _setEjectState = AtariDiskControlFunctions.RequiredDelegate<ExternalCoreApi.SetEjectState>(api.SetEjectState);
        _getEjectState = AtariDiskControlFunctions.RequiredDelegate<ExternalCoreApi.GetEjectState>(api.GetEjectState);
        _getImageIndex = AtariDiskControlFunctions.RequiredDelegate<ExternalCoreApi.GetImageIndex>(api.GetImageIndex);
        _setImageIndex = AtariDiskControlFunctions.RequiredDelegate<ExternalCoreApi.SetImageIndex>(api.SetImageIndex);
        _getImageCount = AtariDiskControlFunctions.RequiredDelegate<ExternalCoreApi.GetImageCount>(api.GetImageCount);
        _replaceImage = AtariDiskControlFunctions.RequiredDelegate<ExternalCoreApi.ReplaceImage>(api.ReplaceImage);
        _addImage = AtariDiskControlFunctions.RequiredDelegate<ExternalCoreApi.AddImage>(api.AddImage);
    }

    private void EnsureAvailable()
    {
        if (!IsAvailable) throw new InvalidOperationException(AtariDiskControlErrors.Unavailable);
    }
}
