using GWGUI.Emulation.Nintendo.Emulators.Dolphin.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Dolphin.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Dolphin.Factories;
using GWGUI.Emulation.Nintendo.Emulators.Dolphin.Functions;
using GWGUI.Emulation.Nintendo.Emulators.Dolphin.Services;

namespace GWGUI.Emulation.Nintendo.Emulators.Dolphin.Contracts;

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



