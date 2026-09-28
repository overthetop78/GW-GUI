using GWGUI.Emulation.Nintendo.Emulators.Mupen64PlusNext.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Mupen64PlusNext.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Mupen64PlusNext.Factories;
using GWGUI.Emulation.Nintendo.Emulators.Mupen64PlusNext.Functions;
using GWGUI.Emulation.Nintendo.Emulators.Mupen64PlusNext.Services;

namespace GWGUI.Emulation.Nintendo.Emulators.Mupen64PlusNext.Contracts;

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



