using System.IO;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Contracts;

namespace GWGUI.Emulation.Services;

internal class ExternalCoreDiskControl : IEmulationDiskControl
{
    private readonly IEmulationDiskControlPolicy _policy;
    protected readonly Func<string, bool>? _mediaExists;

    internal ExternalCoreDiskControl(IEmulationDiskControlPolicy policy, Func<string, bool>? mediaExists = null)
    {
        ArgumentNullException.ThrowIfNull(policy);
        _policy = policy;
        _mediaExists = mediaExists;
    }

    protected ExternalCoreApi.SetEjectState? SetEjectState;
    protected ExternalCoreApi.GetEjectState? _getEjectState;
    private ExternalCoreApi.GetImageIndex? _getImageIndex;
    protected ExternalCoreApi.SetImageIndex? SetImageIndex;
    private ExternalCoreApi.GetImageCount? _getImageCount;
    protected ExternalCoreApi.ReplaceImage? ReplaceImage;
    protected ExternalCoreApi.AddImage? _addImage;
    private ExternalCoreApi.GetImagePath? _getImagePath;
    private ExternalCoreApi.GetImageLabel? _getImageLabel;

    protected string UnavailableMessage => _policy.UnavailableMessage;
    protected string IncompleteMessage => _policy.IncompleteMessage;
    protected string EjectFailedMessage => _policy.EjectFailedMessage;
    protected string SelectFailedMessage => _policy.SelectFailedMessage;
    protected string InsertFailedMessage => _policy.InsertFailedMessage;
    protected string CreateSlotFailedMessage => _policy.CreateSlotFailedMessage;
    protected string ReplaceFailedMessage => _policy.ReplaceFailedMessage;
    protected string MediaMissingMessage => _policy.MediaMissingMessage;
    protected string RequestedSelectFailedMessage => _policy.RequestedSelectFailedMessage;
    protected string RequestedInsertFailedMessage => _policy.RequestedInsertFailedMessage;
    protected bool ResetOnEmptyCapture => _policy.ResetOnEmptyCapture;
    protected virtual bool CountBeforeIndexValidation => true;
    protected virtual bool MediaExists(string path) => _mediaExists?.Invoke(path) ?? (File.Exists(path) || Directory.Exists(path));

    public bool IsAvailable => SetEjectState is not null;
    public int ImageCount => IsAvailable ? checked((int)_getImageCount!()) : 0;
    public int CurrentIndex
    {
        get
        {
            if (!IsAvailable) return -1;
            var index = _getImageIndex!();
            return index == uint.MaxValue ? -1 : checked((int)index);
        }
    }
    protected bool Ejected => !IsAvailable || _getEjectState!();
    public bool IsEjected => Ejected;

    public void Capture(nint data) => CaptureBasic(Marshal.PtrToStructure<ExternalCoreApi.DiskControl>(data));

    public void CaptureExtended(nint data)
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

    public void Clear()
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

    public void Select(int index)
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

    public string? GetPath(int index) => ExternalCoreEnvironmentFunctions.ReadDiskText(_getImagePath, index, 4096);
    public string? GetLabel(int index) => ExternalCoreEnvironmentFunctions.ReadDiskText(_getImageLabel, index, 4096);

    public DiskStatus GetStatus()
    {
        var images = Enumerable.Range(0, ImageCount).Select(index => new DiskImageStatus(index, GetPath(index), GetLabel(index))).ToArray();
        return new DiskStatus(ImageCount, CurrentIndex, Ejected, images);
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

    public virtual void Insert(string path)
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

    public void InsertAt(int index, string path, int firstImageIndex, Func<string, IEmulationLoadedContent> createContent)
    {
        EnsureAvailable();
        if (index < firstImageIndex) throw new ArgumentOutOfRangeException(nameof(index));
        if (!MediaExists(path)) throw new FileNotFoundException(MediaMissingMessage, path);
        var wasEjected = _getEjectState!();
        if (!wasEjected && !SetEjectState!(true)) throw new InvalidOperationException(EjectFailedMessage);
        var inserted = false;
        try
        {
            while (ImageCount <= index)
                if (!_addImage!()) throw new InvalidOperationException(CreateSlotFailedMessage);
            if (!SetImageIndex!(checked((uint)index))) throw new InvalidOperationException(SelectFailedMessage);
            using var content = createContent(path);
            if (!ReplaceImage!(checked((uint)index), content.GameInfo)) throw new InvalidOperationException(ReplaceFailedMessage);
            if (!SetEjectState!(false)) throw new InvalidOperationException(InsertFailedMessage);
            inserted = true;
        }
        finally
        {
            if (!inserted && !wasEjected) SetEjectState!(false);
        }
    }

    public void Eject()
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
