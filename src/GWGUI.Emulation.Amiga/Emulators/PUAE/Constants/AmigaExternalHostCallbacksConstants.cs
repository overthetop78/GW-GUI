namespace GWGUI.Emulation.Amiga.Emulators.PUAE.Constants;

internal static class AmigaExternalHostCallbacksConstants
{
    internal const string UnknownAmigaCoreOption = "Unknown Amiga core option.";
    internal const string OptionKickstart = "puae_kickstart";
    internal const string Extended = "-extended";
    internal const uint JoypadDevice = 1;
    internal const uint MouseDevice = 2;
    internal const uint KeyboardDevice = 3;
    internal const uint AnalogDevice = 5;
    internal const uint JoypadMask = 256;
    internal const int CoreOptionPointerFieldsBeforeValues = 6;
    internal const int CoreOptionValueFieldCount = 2;
    internal const int CoreOptionTerminatorFieldCount = 1;
    internal const int CoreOptionDescriptionPointerIndex = 3;
    internal const int CoreOptionCategoryPointerIndex = 5;
    internal const int MaximumCoreOptionDefinitions = 1024;
    internal const int MaximumCoreOptionValues = 128;
    internal const string UnsupportedPixelFormatTemplate = "Pixel format {0} is not supported.";
    internal const string InvalidOptionValueTemplate =
        "Invalid value '{0}' for Amiga option '{1}'.";

    internal static string UnsupportedPixelFormat(int value) =>
        string.Format(System.Globalization.CultureInfo.InvariantCulture,
            UnsupportedPixelFormatTemplate, value);

    internal static string InvalidOptionValue(string value, string key) =>
        string.Format(System.Globalization.CultureInfo.InvariantCulture,
            InvalidOptionValueTemplate, value, key);
}
