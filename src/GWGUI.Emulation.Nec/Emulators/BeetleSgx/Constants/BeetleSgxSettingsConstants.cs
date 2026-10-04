namespace GWGUI.Emulation.Nec.Emulators.BeetleSgx.Constants;

internal static class BeetleSgxSettingsConstants
{
    internal const string HorizontalOverscan = "sgx_hoverscan";
    internal const string InitialScanline = "sgx_initial_scanline";
    internal const string LastScanline = "sgx_last_scanline";
    internal const string AspectRatio = "sgx_aspect_ratio";
    internal const string NoSpriteLimit = "sgx_nospritelimit";
    internal const string CdVolume = "sgx_cddavolume";
    internal const string AdpcmVolume = "sgx_adpcmvolume";
    internal const string CdPsgVolume = "sgx_cdpsgvolume";
    internal const string MouseSensitivity = "sgx_mouse_sensitivity";
    internal const string ForceSuperGrafx = "sgx_forcesgx";
    internal const string CdImageCache = "sgx_cdimagecache";
    internal const string CdBios = "sgx_cdbios";
    internal const string CdSpeed = "sgx_cdspeed";
    internal const string AspectAuto = "auto";
    internal const string AspectSixFive = "6:5";
    internal const string AspectFourThree = "4:3";
    internal const string DefaultHorizontalOverscan = "352";
    internal const string DefaultInitialScanline = "3";
    internal const string DefaultLastScanline = "242";
    internal const string DefaultVolume = "100";
    internal const string DefaultMouseSensitivity = "1.00";
    internal const int HorizontalOverscanStart = 300;
    internal const int HorizontalOverscanCount = 27;
    internal const int HorizontalOverscanStep = 2;
    internal const int InitialScanlineStart = 0;
    internal const int InitialScanlineCount = 41;
    internal const int LastScanlineStart = 208;
    internal const int LastScanlineCount = 35;
    internal const int VolumeStart = 0;
    internal const int VolumeCount = 21;
    internal const int VolumeStep = 10;
    internal const int MouseSensitivityStart = 4;
    internal const int MouseSensitivityCount = 17;
    internal const string ResourceAspectRatio = "Emulation.Video.AspectRatio";
    internal const string ResourceAuto = "Emulation.Video.Signal.Standard.Automatic";
    internal const string CdDriveEnabledOption = "storage.cdDriveEnabled";
    internal const string DefaultCdSpeed = "1";
    internal static readonly string[] CdSpeeds = ["1", "2", "4", "8"];
}
