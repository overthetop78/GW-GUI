using GWGUI.Emulation.Nintendo.Emulators.Mgba.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Mgba.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Mgba.Factories;
using GWGUI.Emulation.Nintendo.Emulators.Mgba.Functions;
using GWGUI.Emulation.Nintendo.Emulators.Mgba.Services;

namespace GWGUI.Emulation.Nintendo.Emulators.Mgba.Contracts;

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



