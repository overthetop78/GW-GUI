using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Apple.Emulators.Common.Exceptions;
using GWGUI.Emulation.Apple.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Apple.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Interop;
using System.IO;

namespace GWGUI.Emulation.Apple.Emulators.Common.Interop.Services;

internal sealed class DiskControl : GWGUI.Emulation.Services.ExternalRichDiskControl<IEmulationLoadedContent>
{
    protected override string UnavailableMessage => DiskControlErrors.Unavailable;
    protected override string IncompleteMessage => DiskControlErrors.Incomplete;
    protected override string EjectFailedMessage => DiskControlErrors.EjectFailed;
    protected override string SelectFailedMessage => DiskControlErrors.SelectFailed;
    protected override string InsertFailedMessage => DiskControlErrors.InsertFailed;
    protected override string CreateSlotFailedMessage => DiskControlErrors.CreateSlotFailed;
    protected override string ReplaceFailedMessage => DiskControlErrors.ReplaceFailed;
    protected override string MediaMissingMessage => DiskControlErrors.MediaMissing;

    protected override IEmulationLoadedContent CreateContent(string path)
        => ContentFunctions.Create(path, true,
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                Path.GetExtension(path).TrimStart(MediaConstants.ExtensionPrefix)
            });
    protected override nint GetGameInfo(IEmulationLoadedContent content) => content.GameInfo;

    internal void InsertAt(int index, string path)
    {
        EnsureAvailable();
        if (index < DiskControlConstants.FirstImageIndex) throw new ArgumentOutOfRangeException(nameof(index));
        if (!File.Exists(path)) throw new FileNotFoundException(DiskControlErrors.MediaMissing, path);
        var wasEjected = _getEjectState!();
        if (!wasEjected && !SetEjectState!(true)) throw new InvalidOperationException(DiskControlErrors.EjectFailed);
        var inserted = false;
        try
        {
            while (ImageCount <= index)
                if (!_addImage!()) throw new InvalidOperationException(DiskControlErrors.CreateSlotFailed);
            if (!SetImageIndex!(checked((uint)index))) throw new InvalidOperationException(DiskControlErrors.SelectFailed);
            using var content = ContentFunctions.Create(path, true,
                new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    Path.GetExtension(path).TrimStart(MediaConstants.ExtensionPrefix)
                });
            if (!ReplaceImage!(checked((uint)index), content.GameInfo))
                throw new InvalidOperationException(DiskControlErrors.ReplaceFailed);
            if (!SetEjectState!(false)) throw new InvalidOperationException(DiskControlErrors.InsertFailed);
            inserted = true;
        }
        finally
        {
            if (!inserted && !wasEjected) SetEjectState!(false);
        }
    }


    internal DiskStatus GetStatus() => ReadStatus(
        static (int index, string? path, string? label) => new DiskImageStatus(index, path, label),
        static (int count, int index, bool ejected, DiskImageStatus[] images) => new DiskStatus(count, index, ejected, images));
}
