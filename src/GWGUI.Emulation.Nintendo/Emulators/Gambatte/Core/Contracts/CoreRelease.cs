using GWGUI.Emulation.Nintendo.Emulators.Gambatte.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Gambatte.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Gambatte.Factories;
using GWGUI.Emulation.Nintendo.Emulators.Gambatte.Functions;
using GWGUI.Emulation.Nintendo.Emulators.Gambatte.Services;

namespace GWGUI.Emulation.Nintendo.Emulators.Gambatte.Contracts;

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



