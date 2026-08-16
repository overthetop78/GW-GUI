namespace GWGUI.Emulation.Atari;

public static class AtariConstants
{
    public const int CurrentConfigurationSchemaVersion = 1;
    public const int MaximumControllerPortCount = 4;
    public const int MinimumControllerPort = 0;
    internal const uint ExternalCoreApiVersion = 1;
    internal const int MaximumStateSize = 16 * 1024 * 1024;
    internal const int MaximumAudioFramesPerBatch = 64 * 1024;
    internal const int StereoChannelCount = 2;
    internal const int SingleAudioFrameCount = 1;
    internal const int MaximumCoreOptionCount = 1024;
    internal const int LegacyCoreOptionsVersion = 0;
    internal const int MessageInterfaceVersion = 1;
    internal const int PixelFormat0Rgb1555 = 0;
    internal const int PixelFormatXrgb8888 = 1;
    internal const int PixelFormatRgb565 = 2;
    internal const uint NoInputState = 0;
    internal const byte NativeBooleanFalse = 0;
    internal const byte NativeBooleanTrue = 1;
    internal const char LegacyOptionNameSeparator = ';';
    internal const char LegacyOptionValueSeparator = '|';
    internal const char SupportedExtensionSeparator = '|';
    internal const char ExtensionPrefix = '.';
    internal const int FirstBufferIndex = 0;
    internal const int FirstCollectionIndex = 0;
    internal const int Sha256HexLength = 64;
    internal const int LegacyOptionValueStartOffset = 1;
    internal const string PathContextKey = "path";
    internal const string VersionContextKey = "version";
    internal const string ExpectedContextKey = "expected";
    internal const string ActualContextKey = "actual";
    internal const string ExtensionContextKey = "extension";
    internal const string SupportedExtensionsContextKey = "supportedExtensions";
    internal const string SystemDirectoryName = "System";
    internal const string ContentDirectoryName = "Content";
    internal const string SavesDirectoryName = "Saves";
}
