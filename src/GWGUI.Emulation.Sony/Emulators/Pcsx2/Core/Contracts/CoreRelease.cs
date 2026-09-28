using GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Contracts;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Factories;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Functions;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Services;

namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Contracts;

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



