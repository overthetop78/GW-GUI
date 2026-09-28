using GWGUI.Emulation.Nintendo.Emulators.BeetleVb.Constants;
using GWGUI.Emulation.Nintendo.Emulators.BeetleVb.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.BeetleVb.Factories;
using GWGUI.Emulation.Nintendo.Emulators.BeetleVb.Functions;
using GWGUI.Emulation.Nintendo.Emulators.BeetleVb.Services;

namespace GWGUI.Emulation.Nintendo.Emulators.BeetleVb.Contracts;

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



