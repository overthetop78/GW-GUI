using GWGUI.Emulation.Nintendo.Emulators.MelonDs.Constants;
using GWGUI.Emulation.Nintendo.Emulators.MelonDs.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.MelonDs.Factories;
using GWGUI.Emulation.Nintendo.Emulators.MelonDs.Functions;
using GWGUI.Emulation.Nintendo.Emulators.MelonDs.Services;

namespace GWGUI.Emulation.Nintendo.Emulators.MelonDs.Contracts;

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



