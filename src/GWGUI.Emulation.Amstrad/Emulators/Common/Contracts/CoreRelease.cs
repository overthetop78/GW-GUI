using GWGUI.Emulation.Amstrad.Emulators.Common.Contracts;
using GWGUI.Emulation.Amstrad.Emulators.Common.Services;

namespace GWGUI.Emulation.Amstrad.Emulators.Common.Contracts;

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
