
namespace GWGUI.Emulation.Commodore.Emulators.Common.Interop.Constants;

internal static class CoreReleaseConstants
{
    internal const string CoreJson = "core.json";
    internal const string Unknown = "unknown";
    internal const string Version = "version";
    internal const string YyyyMMddHHmm = "yyyyMMdd-HHmm";
    internal const string Latest = "latest";
    internal const string LibretroLatest = "Libretro · latest";
    internal const string Download = ".download";
    internal const string Extract = ".extract";
    internal const string X64 = "x64";
    internal const string OfficialReleaseIdPrefix = "official-";
    internal const string PublishedDisplayFormat = "{0:dd/MM/yyyy HH:mm} · Libretro";
    internal const double CompletedDownloadProgress = 1d;
    internal const int DosHeaderSize = 0x40;
    internal const ushort DosHeaderSignature = 0x5A4D;
    internal const int PeHeaderOffsetPosition = 0x3c;
    internal const int PeSignatureAndMachineSize = 6;
    internal const uint PeHeaderSignature = 0x00004550;
    internal const ushort Amd64Machine = 0x8664;
}
