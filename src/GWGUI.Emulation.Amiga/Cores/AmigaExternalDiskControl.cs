using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Amiga.Cores;

internal sealed class AmigaExternalDiskControl
{
    private AmigaExternalApi.SetEjectState? _setEjectState;
    private AmigaExternalApi.GetEjectState? _getEjectState;
    private AmigaExternalApi.GetImageIndex? _getImageIndex;
    private AmigaExternalApi.SetImageIndex? _setImageIndex;
    private AmigaExternalApi.GetImageCount? _getImageCount;
    private AmigaExternalApi.ReplaceImage? _replaceImage;
    private AmigaExternalApi.AddImage? _addImage;

    internal bool IsAvailable => _setEjectState is not null;

    internal void Capture(nint data)
    {
        var api = Marshal.PtrToStructure<AmigaExternalApi.DiskControl>(data);
        _setEjectState = Delegate<AmigaExternalApi.SetEjectState>(api.SetEjectState);
        _getEjectState = Delegate<AmigaExternalApi.GetEjectState>(api.GetEjectState);
        _getImageIndex = Delegate<AmigaExternalApi.GetImageIndex>(api.GetImageIndex);
        _setImageIndex = Delegate<AmigaExternalApi.SetImageIndex>(api.SetImageIndex);
        _getImageCount = Delegate<AmigaExternalApi.GetImageCount>(api.GetImageCount);
        _replaceImage = Delegate<AmigaExternalApi.ReplaceImage>(api.ReplaceImage);
        _addImage = Delegate<AmigaExternalApi.AddImage>(api.AddImage);
    }

    internal void Insert(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("The Amiga floppy image was not found.", path);
        EnsureAvailable();
        var wasEjected = _getEjectState!();
        if (!wasEjected && !_setEjectState!(true)) throw new InvalidOperationException("The Amiga floppy drive could not be ejected.");
        var count = _getImageCount!();
        var index = count == 0 ? 0u : Math.Min(_getImageIndex!(), count - 1);
        if (count == 0 && !_addImage!()) throw new InvalidOperationException("The Amiga core could not create a floppy slot.");

        var nativePath = Marshal.StringToCoTaskMemUTF8(Path.GetFullPath(path));
        var game = Marshal.AllocHGlobal(Marshal.SizeOf<AmigaExternalApi.GameInfo>());
        try
        {
            Marshal.StructureToPtr(new AmigaExternalApi.GameInfo { Path = nativePath }, game, false);
            if (!_replaceImage!(index, game)) throw new InvalidOperationException("The Amiga core refused the floppy image.");
            if (!_setImageIndex!(index)) throw new InvalidOperationException("The Amiga core could not select the floppy image.");
            if (!_setEjectState!(false)) throw new InvalidOperationException("The Amiga floppy drive could not insert the image.");
        }
        finally
        {
            Marshal.FreeHGlobal(game);
            Marshal.FreeCoTaskMem(nativePath);
        }
    }

    internal void Eject()
    {
        EnsureAvailable();
        if (!_setEjectState!(true)) throw new InvalidOperationException("The Amiga floppy drive could not be ejected.");
    }

    private void EnsureAvailable()
    {
        if (!IsAvailable) throw new InvalidOperationException("The Amiga core has not provided disk control.");
    }

    private static T Delegate<T>(nint pointer) where T : Delegate
    {
        if (pointer == 0) throw new InvalidOperationException("The Amiga core provided incomplete disk control.");
        return Marshal.GetDelegateForFunctionPointer<T>(pointer);
    }
}
