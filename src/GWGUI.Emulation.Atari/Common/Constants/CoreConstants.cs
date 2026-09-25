namespace GWGUI.Emulation.Atari.Common.Constants;

public static class CommonConstants
{
    public const int CurrentConfigurationSchemaVersion = 1;
    public const int MaximumControllerPortCount = 4;
    public const int MinimumControllerPort = 0;
    internal const uint ExternalCoreApiVersion = 1;
    internal const int MaximumStateSize = 16 * 1024 * 1024;
    internal const int MessageInterfaceVersion = 1;
    internal const int PixelFormat0Rgb1555 = 0;
    internal const int PixelFormatXrgb8888 = 1;
    internal const int PixelFormatRgb565 = 2;
    internal const uint NoInputState = 0;
    internal const byte NativeBooleanFalse = 0;
    internal const byte NativeBooleanTrue = 1;
    internal const char SupportedExtensionSeparator = '|';
    internal const char ExtensionPrefix = '.';
    internal const int FirstBufferIndex = 0;
    internal const int FirstCollectionIndex = 0;
    internal const int Sha256HexLength = 64;
    internal const int EmptyCollectionCount = 0;
    internal const uint EmptyNativeCollectionCount = 0;
    internal const uint EmptyFrameDimension = 0;
    internal const nuint EmptyNativeSize = 0;
    internal const int InactiveState = 0;
    internal const string PathContextKey = "path";
    internal const string VersionContextKey = "version";
    internal const string ExpectedContextKey = "expected";
    internal const string ActualContextKey = "actual";
    internal const string ExtensionContextKey = "extension";
    internal const string SupportedExtensionsContextKey = "supportedExtensions";
    internal const string SystemDirectoryName = "System";
    internal const string ContentDirectoryName = "Content";
    internal const string SavesDirectoryName = "Saves";
    internal const string AssetsDirectoryName = "Assets";
}

public static class CoreHostConstants
{
    public const string CommandLineArgument = "--atari-core-host";
    internal const string HostName = "Atari";
    internal const string PipePrefix = "gwgui-atari-";
    internal const string VideoMapPrefix = "gwgui-atari-video-";
    internal const string LocalPipeServerName = ".";
    internal const string UniqueNameFormat = "N";
    internal const int ProtocolVersion = 3;
    internal const int MaximumPipeInstances = 1;
    internal const int PipeBufferSize = 8 * 1024 * 1024;
    internal const int ConnectionTimeoutMilliseconds = 15_000;
    internal const int ResponseTimeoutSeconds = 30;
    internal const int GracefulExitTimeoutMilliseconds = 5_000;
    internal const int NativeOperationSuccess = 0;
    internal const int MinimumVideoSlotCapacity = 64 * 1024;
    internal const string VideoMapGenerationSeparator = "-";
    internal const char ExtensionListSeparator = '|';
    internal const long InitialVideoSequence = 0L;
    internal const int InitialDiagnosticCount = 0;
    internal const byte InitializeCommand = 1;
    internal const byte RunFrameCommand = 2;
    internal const byte HardResetCommand = 3;
    internal const byte StopCommand = 4;
    internal const byte InsertMediaCommand = 5;
    internal const byte EjectMediaCommand = 6;
    internal const byte SaveStateCommand = 7;
    internal const byte LoadStateCommand = 8;
    internal const byte SetOptionCommand = 9;
    internal const byte SelectDiskCommand = 10;
    internal const byte DisposeCommand = 11;
    internal const byte SaveMediaChangesCommand = 12;
    internal const byte GetDiskStatusCommand = 13;
    internal const byte HasUnsavedMediaChangesCommand = 14;
    internal const byte SetControllerPortDeviceCommand = 15;
    internal const byte SuccessResponse = 1;
    internal const byte FailureResponse = 2;
}

internal static class CoreHostErrors
{
    internal const string InvalidConfiguration = "The Atari host configuration is invalid.";
    internal const string NotInitialized = "The Atari host is not initialized.";
    internal const string AlreadyInitialized = "The Atari core process is already initialized.";
    internal const string ExecutableMissing = "The GW GUI executable used to host the Atari core was not found.";
    internal const string ProcessStartFailed = "The Atari core host process could not be started.";
    internal const string ProcessUnavailable = "The Atari core process is no longer available.";
    internal const string ProcessNotInitialized = "The Atari core process is not initialized.";
    internal const string SharedVideoUnavailable = "The shared Atari video buffer is unavailable.";
    internal const string ResponseUnavailable = "The Atari host response is unavailable.";
    internal const string ResponseTimeout = "The Atari core process did not answer within the allowed time and was stopped.";
    internal const string RequestCancelled = "Communication with the Atari core process was cancelled and the process was stopped.";
    internal const string CommunicationFailed = "Communication with the Atari core process failed.";
    internal const string ProtocolVersionMismatchFormat = "The Atari host protocol version {0} is not supported; expected {1}.";
    internal const string UnknownCommandFormat = "Unknown Atari host command {0}.";
    internal const string InvalidResponseLengthFormat = "The Atari core process sent invalid response length {0}.";
    internal const string ProcessExitSuffixFormat = " It exited with code {0}.";
}

internal static class CoreHostFunctionsConstants
{
    internal const string Windows = "windows";
}

internal static class CoreHostValues
{
    internal const string Windows = "windows";
}

internal static class CoreIdentityConstants
{
    internal const string Hatari = "Hatari";
    internal const string Atari800 = "Atari800";
    internal const string Stella = "Stella 2023";
    internal const string ProSystem = "ProSystem";
    internal const string BeetleLynx = "Beetle Lynx";
    internal const string VirtualJaguar = "Virtual Jaguar";
}

internal static class CoreLifecycleConstants
{
    internal const uint NoDevice = 0;
    internal const uint DefaultJoypadDevice = 1;
    internal const string JoypadDeviceName = "RetroPad";
    internal const string KeyboardDeviceName = "Keyboard";
    internal const string MouseDeviceName = "Mouse";
    internal const string JoystickDeviceName = "Joystick";
    internal const string AnalogDeviceName = "Analog";
    internal const string PaddleDeviceName = "Paddle";
    internal const string LightGunDeviceName = "Lightgun";
    internal const string NumericKeypadDeviceName = "Keypad";
    internal const string DrivingControllerDeviceName = "Driving";
    internal const string ProLineControllerDeviceName = JoypadDeviceName;
    internal const string EnhancedControllerDeviceName = JoypadDeviceName;
}
