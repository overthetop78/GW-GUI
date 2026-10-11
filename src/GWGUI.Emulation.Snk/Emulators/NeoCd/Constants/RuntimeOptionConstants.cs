namespace GWGUI.Emulation.Snk.Emulators.NeoCd.Constants;

internal static class RuntimeOptionConstants
{
    internal const string Region = "neocd_region";
    internal const string OverscanH = "neocd_overscan_h";
    internal const string Cdspeedhack = "neocd_cdspeedhack";
    internal const string Loadskip = "neocd_loadskip";
    internal const string CpuOverclock = "neocd_cpu_overclock";
    internal const string PerContentSaves = "neocd_per_content_saves";
    internal const string AspectRatio = "neocd_aspect_ratio";
    internal const string Bios = "neocd_bios";
    internal const string On = "On";
    internal const string Off = "Off";
    internal const string SystemDirectory = "neocd";
    internal const string SelectedFirmware = "selected.bin";
    internal const int BiosBytes = 524288;
    internal const int IdentificationBytes = 512;
    internal const bool NeedsFullPath = true;
    internal static readonly IReadOnlyList<string> DiscExtensions = Array.AsReadOnly(new[] { ".cue", ".chd" });
}
