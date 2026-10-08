using GWGUI.Emulation.Sega.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Services;

namespace GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts;

public sealed record CoreRelease(
    string Id,
    string DisplayName,
    Uri DownloadUri,
    DateTimeOffset? PublishedUtc,
    bool IsRequired,
    bool IsZip)
{
    public override string ToString() => DisplayName;
}



