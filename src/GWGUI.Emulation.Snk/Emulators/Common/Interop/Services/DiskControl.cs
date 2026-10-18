using GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Snk.Emulators.Common.Exceptions;
using System.IO;

namespace GWGUI.Emulation.Snk.Emulators.Common.Interop.Services;

internal sealed class DiskControl : GWGUI.Emulation.Services.ExternalRichDiskControl<GWGUI.Emulation.Snk.Emulators.Common.Interop.Services.LoadedContent>
{
    protected override string UnavailableMessage => DiskControlErrors.Unavailable;
    protected override string IncompleteMessage => DiskControlErrors.Incomplete;
    protected override string EjectFailedMessage => DiskControlErrors.EjectFailed;
    protected override string SelectFailedMessage => DiskControlErrors.SelectFailed;
    protected override string InsertFailedMessage => DiskControlErrors.InsertFailed;
    protected override string CreateSlotFailedMessage => DiskControlErrors.CreateSlotFailed;
    protected override string ReplaceFailedMessage => DiskControlErrors.ReplaceFailed;
    protected override string MediaMissingMessage => DiskControlErrors.MediaMissing;
    protected override bool ResetOnEmptyCapture => true;

    protected override GWGUI.Emulation.Snk.Emulators.Common.Interop.Services.LoadedContent CreateContent(string path)
        => ContentFunctions.Create(path, true, new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                Path.GetExtension(path).TrimStart(MediaConstants.ExtensionPrefix)
            });
    protected override nint GetGameInfo(GWGUI.Emulation.Snk.Emulators.Common.Interop.Services.LoadedContent content) => content.GameInfo;

    internal DiskStatus GetStatus() => ReadStatus(
        static (int index, string? path, string? label) => new DiskImageStatus(index, path, label),
        static (int count, int index, bool ejected, DiskImageStatus[] images) => new DiskStatus(count, index, ejected, images));
}
