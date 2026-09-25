using GWGUI.Emulation.Amiga.Emulators.PUAE.Constants;
using GWGUI.Emulation.Amiga.Emulators.PUAE.Contracts;
using GWGUI.Emulation.Amiga.Emulators.PUAE.Factories;
using GWGUI.Emulation.Amiga.Emulators.PUAE.Functions;
using GWGUI.Emulation.Amiga.Emulators.PUAE.Services;

namespace GWGUI.Emulation.Amiga.Emulators.PUAE.Contracts;

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
