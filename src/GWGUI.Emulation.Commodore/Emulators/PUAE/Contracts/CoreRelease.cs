using GWGUI.Emulation.Commodore.Emulators.PUAE.Constants;
using GWGUI.Emulation.Commodore.Emulators.PUAE.Contracts;
using GWGUI.Emulation.Commodore.Emulators.PUAE.Factories;
using GWGUI.Emulation.Commodore.Emulators.PUAE.Functions;
using GWGUI.Emulation.Commodore.Emulators.PUAE.Services;

namespace GWGUI.Emulation.Commodore.Emulators.PUAE.Contracts;

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
