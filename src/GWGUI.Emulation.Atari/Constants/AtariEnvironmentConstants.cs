namespace GWGUI.Emulation.Atari.Constants;

internal static class AtariEnvironmentConstants
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
    internal const ulong MouseCapability = 1UL << 2;
    internal const ulong KeyboardCapability = 1UL << 3;
    internal const ulong AnalogCapability = 1UL << 5;
    internal const string UnknownCommandDiagnosticFormat = "Unknown Atari environment command: {0}.";
    internal const string NativeLogArgumentMarker = "<native-argument>";
    internal const string NativeLogConversionCharacters = "diuoxXfFeEgGaAcspn";
    internal const float NoSensorInput = 0f;
}
