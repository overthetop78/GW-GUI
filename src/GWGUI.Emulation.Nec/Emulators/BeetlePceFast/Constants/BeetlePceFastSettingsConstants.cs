namespace GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Constants;

internal static class BeetlePceFastSettingsConstants
{
    internal const int PceLastScanlineStart = 208;
    internal const int PceLastScanlineCount = 35;
    internal const int PceHorizontalWidthStart = 150;
    internal const int PceHorizontalWidthCount = 27;
    internal const int HorizontalWidthStep = 2;
    internal const int FrameSkipThresholdStart = 15;
    internal const int FrameSkipThresholdCount = 16;
    internal const int FrameSkipThresholdStep = 3;
    internal const int VolumeStart = 0;
    internal const int VolumeCount = 21;
    internal const int VolumeStep = 10;
    internal const int PceMouseSensitivityStart = 1;
    internal const int PceMouseSensitivityCount = 20;
    internal const string DefaultVolume = "100";
    internal const string DefaultPceHorizontalWidth = "352";
    internal const string DefaultPceFrameSkipThreshold = "33";
    internal const string DefaultPceInitialScanline = "3";
    internal const string DefaultPceLastScanline = "242";
    internal const string PaletteRgb = "RGB";
    internal const string PaletteComposite = "Composite";
    internal const string FrameSkipAuto = "auto";
    internal const string FrameSkipManual = "manual";
    internal const string ResourcePalette = "Emulation.Nec.Video.Palette";
    internal const string ResourcePaletteRgb = "Emulation.Nec.Video.Palette.Rgb";
    internal const string ResourcePaletteComposite = "Emulation.Nec.Video.Palette.Composite";
    internal const string ResourceHorizontalWidth = "Emulation.Nec.Video.HorizontalWidth";
    internal const string ResourceFrameSkip = "Emulation.Nec.Video.FrameSkip";
    internal const string ResourceFrameSkipDisabled = "Emulation.Nec.Video.FrameSkip.Disabled";
    internal const string ResourceFrameSkipAuto = "Emulation.Nec.Video.FrameSkip.Auto";
    internal const string ResourceFrameSkipManual = "Emulation.Nec.Video.FrameSkip.Manual";
    internal const string ResourceFrameSkipThreshold = "Emulation.Nec.Video.FrameSkipThreshold";
    internal const string ResourceCdVolume = "Emulation.Nec.Audio.CdVolume";
    internal const string ResourceAdpcmVolume = "Emulation.Nec.Audio.AdpcmVolume";
    internal const string ResourceCdPsgVolume = "Emulation.Nec.Audio.CdPsgVolume";
    internal const string ResourceAdpcmLowPass = "Emulation.Nec.Audio.AdpcmLowPass";
    internal const int PceFirstScanlineStart = 0;
    internal const int PceFirstScanlineCount = 41;
}
