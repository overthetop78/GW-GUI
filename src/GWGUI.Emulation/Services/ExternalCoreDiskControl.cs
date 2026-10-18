using System.IO;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Interop;

namespace GWGUI.Emulation.Services;

internal abstract class ExternalCoreDiskControl
{
    protected ExternalCoreApi.SetEjectState? SetEjectState;
    protected ExternalCoreApi.GetEjectState? _getEjectState;
    private ExternalCoreApi.GetImageIndex? _getImageIndex;
    protected ExternalCoreApi.SetImageIndex? SetImageIndex;
    private ExternalCoreApi.GetImageCount? _getImageCount;
    protected ExternalCoreApi.ReplaceImage? ReplaceImage;
    protected ExternalCoreApi.AddImage? _addImage;
    private ExternalCoreApi.GetImagePath? _getImagePath;
    private ExternalCoreApi.GetImageLabel? _getImageLabel;

    protected abstract string UnavailableMessage { get; }
    protected abstract string IncompleteMessage { get; }
    protected abstract string EjectFailedMessage { get; }
    protected abstract string SelectFailedMessage { get; }
    protected abstract string InsertFailedMessage { get; }
    protected abstract string CreateSlotFailedMessage { get; }
    protected abstract string ReplaceFailedMessage { get; }
    protected abstract string MediaMissingMessage { get; }
    protected virtual string RequestedSelectFailedMessage => SelectFailedMessage;
    protected virtual string RequestedInsertFailedMessage => InsertFailedMessage;
    protected virtual bool ResetOnEmptyCapture => false;
    protected virtual bool CountBeforeIndexValidation => true;
    protected virtual bool MediaExists(string path) => File.Exists(path) || Directory.Exists(path);

    internal bool IsAvailable => SetEjectState is not null;
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
    protected bool Ejected => !IsAvailable || _getEjectState!();

    internal void Capture(nint data) => CaptureBasic(Marshal.PtrToStructure<ExternalCoreApi.DiskControl>(data));

    internal void CaptureExtended(nint data)
    {
        var api = Marshal.PtrToStructure<ExternalCoreApi.DiskControlExtended>(data);
        CaptureBasic(api.Basic);
        _getImagePath = ExternalCoreEnvironmentFunctions.OptionalDelegate<ExternalCoreApi.GetImagePath>(api.GetImagePath);
        _getImageLabel = ExternalCoreEnvironmentFunctions.OptionalDelegate<ExternalCoreApi.GetImageLabel>(api.GetImageLabel);
    }

    private void CaptureBasic(ExternalCoreApi.DiskControl api)
    {
        if (ResetOnEmptyCapture && api.SetEjectState == 0 && api.GetEjectState == 0 && api.GetImageIndex == 0
            && api.SetImageIndex == 0 && api.GetImageCount == 0 && api.ReplaceImage == 0 && api.AddImage == 0)
        {
            Clear();
            return;
        }
        SetEjectState = Required<ExternalCoreApi.SetEjectState>(api.SetEjectState);
        _getEjectState = Required<ExternalCoreApi.GetEjectState>(api.GetEjectState);
        _getImageIndex = Required<ExternalCoreApi.GetImageIndex>(api.GetImageIndex);
        SetImageIndex = Required<ExternalCoreApi.SetImageIndex>(api.SetImageIndex);
        _getImageCount = Required<ExternalCoreApi.GetImageCount>(api.GetImageCount);
        ReplaceImage = Required<ExternalCoreApi.ReplaceImage>(api.ReplaceImage);
        _addImage = Required<ExternalCoreApi.AddImage>(api.AddImage);
    }

    internal void Clear()
    {
        SetEjectState = null;
        _getEjectState = null;
        _getImageIndex = null;
        SetImageIndex = null;
        _getImageCount = null;
        ReplaceImage = null;
        _addImage = null;
        _getImagePath = null;
        _getImageLabel = null;
    }

    internal void Select(int index)
    {
        EnsureAvailable();
        if (CountBeforeIndexValidation)
        {
            var count = ImageCount;
            if (index < 0 || index >= count) throw new ArgumentOutOfRangeException(nameof(index));
        }
        else if (index < 0 || index >= ImageCount) throw new ArgumentOutOfRangeException(nameof(index));
        ExternalCoreEnvironmentFunctions.SelectDisk(index,
            () => _getImageIndex!(), () => _getEjectState!(),
            value => SetEjectState!(value), value => SetImageIndex!(value),
            () => new InvalidOperationException(EjectFailedMessage),
            () => new InvalidOperationException(RequestedSelectFailedMessage),
            () => new InvalidOperationException(RequestedInsertFailedMessage));
    }

    internal string? GetPath(int index) => ExternalCoreEnvironmentFunctions.ReadDiskText(_getImagePath, index, 4096);
    internal string? GetLabel(int index) => ExternalCoreEnvironmentFunctions.ReadDiskText(_getImageLabel, index, 4096);

    protected TStatus ReadStatus<TImage, TStatus>(Func<int, string?, string?, TImage> image, Func<int, int, bool, TImage[], TStatus> status)
    {
        var images = Enumerable.Range(0, ImageCount).Select(index => image(index, GetPath(index), GetLabel(index))).ToArray();
        return status(ImageCount, CurrentIndex, Ejected, images);
    }

    protected uint PrepareInsertion(string path, out bool wasEjected)
    {
        if (!MediaExists(path)) throw new FileNotFoundException(MediaMissingMessage, path);
        EnsureAvailable();
        wasEjected = _getEjectState!();
        if (!wasEjected && !SetEjectState!(true)) throw new InvalidOperationException(EjectFailedMessage);
        var count = _getImageCount!();
        var index = count == 0 ? 0 : Math.Min(_getImageIndex!(), count - 1);
        if (count == 0 && !_addImage!()) throw new InvalidOperationException(CreateSlotFailedMessage);
        return index;
    }

    internal virtual void Insert(string path)
    {
        var index = PrepareInsertion(path, out var wasEjected);
        nint nativePath = 0;
        nint game = 0;
        var entered = false;
        var inserted = false;
        try
        {
            nativePath = Marshal.StringToCoTaskMemUTF8(Path.GetFullPath(path));
            game = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.GameInfo>());
            entered = true;
            Marshal.StructureToPtr(new ExternalCoreApi.GameInfo { Path = nativePath }, game, false);
            ReplaceAndInsert(index, game);
            inserted = true;
        }
        finally
        {
            if (game != 0) Marshal.FreeHGlobal(game);
            if (nativePath != 0) Marshal.FreeCoTaskMem(nativePath);
            if (entered && !inserted && !wasEjected) SetEjectState!(false);
        }
    }

    protected void ReplaceAndInsert(uint index, nint game)
    {
        if (!ReplaceImage!(index, game)) throw new InvalidOperationException(ReplaceFailedMessage);
        if (!SetImageIndex!(index)) throw new InvalidOperationException(SelectFailedMessage);
        if (!SetEjectState!(false)) throw new InvalidOperationException(InsertFailedMessage);
    }

    internal void Eject()
    {
        EnsureAvailable();
        if (!SetEjectState!(true)) throw new InvalidOperationException(EjectFailedMessage);
    }

    protected void EnsureAvailable()
    {
        if (!IsAvailable) throw new InvalidOperationException(UnavailableMessage);
    }

    private T Required<T>(nint pointer) where T : Delegate
        => ExternalCoreEnvironmentFunctions.RequiredDelegate<T>(pointer, () => new InvalidOperationException(IncompleteMessage));
}
