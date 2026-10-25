namespace GWGUI.Emulation.Commodore.Common.Machines.Common.Constants;

internal static class MemoryConstants
{
    internal const int BytesPerKib = 1024;
    internal const int KibPerMib = 1024;
    internal const long BytesPerMib = (long)BytesPerKib * KibPerMib;
    internal const string KibLabelFormat = "{0} KiB";
    internal const string MibLabelFormat = "{0} MiB";
    internal const string FractionalKibLabelFormat = "{0:0.#} KiB";
    internal const string FractionalMibLabelFormat = "{0:0.##} MiB";
}
