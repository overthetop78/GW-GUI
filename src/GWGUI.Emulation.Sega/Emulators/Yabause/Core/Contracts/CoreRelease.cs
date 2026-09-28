using GWGUI.Emulation.Sega.Emulators.Yabause.Constants;
using GWGUI.Emulation.Sega.Emulators.Yabause.Contracts;
using GWGUI.Emulation.Sega.Emulators.Yabause.Factories;
using GWGUI.Emulation.Sega.Emulators.Yabause.Functions;
using GWGUI.Emulation.Sega.Emulators.Yabause.Services;

namespace GWGUI.Emulation.Sega.Emulators.Yabause.Contracts;

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

