using GWGUI.Emulation.Nintendo.Emulators.GameWatch.Constants;
using GWGUI.Emulation.Nintendo.Emulators.GameWatch.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.GameWatch.Factories;
using GWGUI.Emulation.Nintendo.Emulators.GameWatch.Functions;
using GWGUI.Emulation.Nintendo.Emulators.GameWatch.Services;

namespace GWGUI.Emulation.Nintendo.Emulators.GameWatch.Contracts;

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




