using GWGUI.Emulation.Nintendo.Emulators.Citra.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Citra.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Citra.Factories;
using GWGUI.Emulation.Nintendo.Emulators.Citra.Functions;
using GWGUI.Emulation.Nintendo.Emulators.Citra.Services;

namespace GWGUI.Emulation.Nintendo.Emulators.Citra.Contracts;

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



