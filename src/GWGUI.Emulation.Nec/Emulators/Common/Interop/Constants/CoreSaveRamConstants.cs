namespace GWGUI.Emulation.Nec.Emulators.Common.Interop.Constants;

internal static class CoreSaveRamConstants
{
    internal const uint SaveRamType = 0;
    internal const int MaximumSize = 1024 * 1024;
    internal const string FileName = "backup.srm";
    internal const string TemporaryIdFormat = "N";
    internal const string TemporaryNameSeparator = ".";
    internal const string TemporarySuffix = ".tmp";
    internal const int FirstByteOffset = 0;
}
