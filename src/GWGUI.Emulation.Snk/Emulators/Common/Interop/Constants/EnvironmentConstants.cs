using GWGUI.Emulation;

namespace GWGUI.Emulation.Snk.Emulators.Common.Interop.Constants;


internal static class EnvironmentConstants
{
    internal const int MaximumInputDescriptorCount = 4096;
    internal const int MaximumControllerPortCount = 32;
    internal const int MaximumControllerTypeCount = 256;
    internal const int MaximumMemoryDescriptorCount = 4096;
    internal const int NextCharacterOffset = 1;
    internal const uint FirstRotation = 0;
    internal const uint LastRotation = 3;
    internal const uint NoRotation = 0;
    internal const uint EnglishLanguage = 0;
    internal const ulong JoypadCapability = 1UL << 1;
    internal const ulong SupportedInputCapabilities = (1UL << 1) | (1UL << 2) | (1UL << 3);
    internal const string UnknownCommandDiagnosticResourceKey = "Emulation.Snk.Error.UnknownEnvironmentCommand";
    internal const string NativeLogArgumentMarker = "<native-argument>";
    internal const string NativeLogConversionCharacters = "diuoxXfFeEgGaAcspn";
    internal const float NoSensorInput = 0f;
    internal const uint SetAudioBufferStatusCallback = 62;
    internal const uint SetMinimumAudioLatency = 63;
    internal const int MaximumAudioLatencyMilliseconds = 500;
    internal const int MinimumAudioBufferValue = 0;
    internal const uint MaximumAudioOccupancyPercent = 100;
}
