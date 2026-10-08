using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;

internal static class CoreReleaseConstants
{
    internal const string CoreJson = "core.json";
    internal const string Unknown = "unknown";
    internal const string Version = "version";
    internal const string YyyyMMddHHmm = "yyyyMMdd-HHmm";
    internal const string Latest = "latest";
    internal const string Download = ".download";
    internal const string Extract = ".extract";
    internal const string X64 = "x64";
    internal const string OfficialReleasePrefix = "official-";
    internal const string PublicationDateFormat = "dd/MM/yyyy HH:mm";
    internal const string ReleaseDisplaySeparator = " · ";
    internal const int DownloadBufferSize = 81920;
    internal const double CompletedProgress = 1d;
    internal const long MinimumPeHeaderSize = 0x40;
    internal const ushort DosSignature = 0x5A4D;
    internal const int PeHeaderOffsetPosition = 0x3c;
    internal const int PeSignatureAndArchitectureSize = 6;
    internal const uint PeSignature = 0x00004550;
    internal const ushort Amd64MachineType = 0x8664;
}
