using GWGUI.Emulation.Sega.Emulators.GenesisPlusGX.Constants;
using GWGUI.Emulation.Sega.Emulators.GenesisPlusGX.Contracts;
using GWGUI.Emulation.Sega.Emulators.GenesisPlusGX.Factories;
using GWGUI.Emulation.Sega.Emulators.GenesisPlusGX.Functions;
using GWGUI.Emulation.Sega.Emulators.GenesisPlusGX.Services;

namespace GWGUI.Emulation.Sega.Emulators.GenesisPlusGX.Contracts;

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

