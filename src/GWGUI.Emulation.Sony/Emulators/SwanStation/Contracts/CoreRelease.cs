using GWGUI.Emulation.Sony.Emulators.SwanStation.Constants;
using GWGUI.Emulation.Sony.Emulators.SwanStation.Contracts;
using GWGUI.Emulation.Sony.Emulators.SwanStation.Factories;
using GWGUI.Emulation.Sony.Emulators.SwanStation.Functions;
using GWGUI.Emulation.Sony.Emulators.SwanStation.Services;

namespace GWGUI.Emulation.Sony.Emulators.SwanStation.Contracts;

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



