namespace GWGUI.Emulation.Nec.Emulators.Geargrafx.Constants;

internal static class GeargrafxFirmwareConstants
{
    internal const string SystemCard1 = "syscard1.pce";
    internal const string SystemCard2 = "syscard2.pce";
    internal const string SystemCard3 = "syscard3.pce";
    internal const string GameExpress = "gexpress.pce";
    internal const string PacN1 = "pac-n1.bin";
    internal const string PceLp1 = "pce-lp1.bin";
    internal const string PacN10 = "pac-n10.bin";
    internal static readonly IReadOnlySet<string> SupportedNames =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            SystemCard1, SystemCard2, SystemCard3, GameExpress,
            PacN1, PceLp1, PacN10
        };
}
