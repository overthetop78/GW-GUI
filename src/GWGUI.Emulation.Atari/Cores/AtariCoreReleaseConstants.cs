namespace GWGUI.Emulation.Atari.Cores;

internal static class AtariCoreReleaseConstants
{
    internal const string ReleaseIdPrefix = "official-";
    internal const string ReleaseVersionFormat = "yyyyMMdd-HHmmss";
    internal const string TemporaryDownloadExtension = ".download";
    internal const string TemporaryExtractExtension = ".extract";
    internal const string TemporaryManifestExtension = ".temporary";
    internal const string UnknownDiagnosticValue = "unknown";
    internal const string WindowsX86Architecture = "x86";
    internal const string WindowsArm64Architecture = "arm64";
    internal const int DownloadBufferSize = 81_920;
    internal const double CompletedProgress = 1D;
    internal const int PeExportDirectoryMinimumSize = 40;
    internal const int PeExportNumberOfNamesOffset = 24;
    internal const int PeExportAddressOfNamesOffset = 32;
    internal const int ExportNameRvaSize = sizeof(uint);
    internal const int MaximumExportNameLength = 4_096;
    internal const ushort WindowsX86Machine = 0x014c;
    internal const ushort WindowsX64Machine = 0x8664;
    internal const ushort WindowsArm64Machine = 0xaa64;
}
