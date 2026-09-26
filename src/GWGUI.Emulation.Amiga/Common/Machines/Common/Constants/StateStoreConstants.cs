namespace GWGUI.Emulation.Amiga.Common.Machines.Common.Constants;

internal static class StateStoreConstants
{
    internal static readonly byte[] Magic = "GWAMIGA1"u8.ToArray();
    internal const string Tmp = ".tmp";
    internal const int MaximumHeaderLength = 1024 * 1024;
    internal const string Value = "*";
}
