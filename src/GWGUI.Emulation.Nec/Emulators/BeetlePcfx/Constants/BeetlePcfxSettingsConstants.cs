namespace GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Constants;

internal static class BeetlePcfxSettingsConstants
{
    internal const int PcfxLastScanlineStart = 208;
    internal const int PcfxLastScanlineCount = 32;
    internal const int PcfxFirstScanlineStart = 0;
    internal const int PcfxFirstScanlineCount = 41;
    internal const int PcfxMouseSensitivityStart = 4;
    internal const int PcfxMouseSensitivityCount = 17;
    internal const string PcfxWidth256 = "256";
    internal const string PcfxWidth341 = "341";
    internal const string DefaultSoundQuality = "3";
    internal const string DefaultPcfxWidth = "1024";
    internal const string DefaultPcfxInitialScanline = "4";
    internal const string DefaultPcfxLastScanline = "235";
    internal const string ResourceDotClockWidth = "Emulation.Nec.Video.DotClockWidth";
    internal const string ResourceChromaInterpolation = "Emulation.Nec.Video.ChromaInterpolation";
    internal const string ResourceSoundQuality = "Emulation.Nec.Audio.SoundQuality";
    internal const string ResourceSuppressClicks = "Emulation.Nec.Audio.SuppressClicks";
    internal const string ResourceBuggyCodec = "Emulation.Nec.Audio.BuggyCodec";
}
