using GWGUI.Emulation.Nec.Emulators.BeetlePce.Constants;
using GWGUI.Emulation.Nec.Emulators.BeetlePce.Contracts;
using GWGUI.Emulation.Nec.Emulators.BeetlePce.Factories;
using GWGUI.Emulation.Nec.Emulators.BeetlePce.Functions;
using GWGUI.Emulation.Nec.Emulators.BeetlePce.Services;

namespace GWGUI.Emulation.Nec.Emulators.BeetlePce.Contracts;

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



