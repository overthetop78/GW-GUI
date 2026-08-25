namespace GWGUI.Emulation.Amiga.Constants;

internal static class AmigaStateStoreConstants
{
    internal static readonly byte[] Magic = "GWAMIGA1"u8.ToArray();
    internal const string Tmp = ".tmp";
    internal const string TheFileIsNotAGWGUIAmigaState = "The file is not a GW GUI Amiga state.";
    internal const string TheAmigaStateHeaderLengthIsInvalid = "The Amiga state header length is invalid.";
    internal const string TheAmigaStateHeaderIsInvalid = "The Amiga state header is invalid.";
    internal const string TheAmigaStatePayloadIsCorrupted = "The Amiga state payload is corrupted.";
    internal const string TheAmigaMediaPathWasNotFound = "The Amiga media path was not found.";
    internal const string Value = "*";
}
