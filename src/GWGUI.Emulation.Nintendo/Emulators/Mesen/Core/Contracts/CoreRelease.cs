using GWGUI.Emulation.Nintendo.Emulators.Mesen.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Mesen.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Mesen.Factories;
using GWGUI.Emulation.Nintendo.Emulators.Mesen.Functions;
using GWGUI.Emulation.Nintendo.Emulators.Mesen.Services;

namespace GWGUI.Emulation.Nintendo.Emulators.Mesen.Contracts;

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



