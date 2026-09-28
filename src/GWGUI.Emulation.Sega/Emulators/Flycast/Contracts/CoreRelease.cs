using GWGUI.Emulation.Sega.Emulators.Flycast.Constants;
using GWGUI.Emulation.Sega.Emulators.Flycast.Contracts;
using GWGUI.Emulation.Sega.Emulators.Flycast.Factories;
using GWGUI.Emulation.Sega.Emulators.Flycast.Functions;
using GWGUI.Emulation.Sega.Emulators.Flycast.Services;

namespace GWGUI.Emulation.Sega.Emulators.Flycast.Contracts;

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

