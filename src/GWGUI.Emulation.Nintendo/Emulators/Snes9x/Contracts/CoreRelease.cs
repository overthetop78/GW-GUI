using GWGUI.Emulation.Nintendo.Emulators.Snes9x.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Snes9x.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Snes9x.Factories;
using GWGUI.Emulation.Nintendo.Emulators.Snes9x.Functions;
using GWGUI.Emulation.Nintendo.Emulators.Snes9x.Services;

namespace GWGUI.Emulation.Nintendo.Emulators.Snes9x.Contracts;

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



