namespace GWGUI.Emulation.Atari.Emulators.Libretro.Exceptions;

internal static class ErrorMessages
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(Modules.AtariEmulationModule).Assembly,
        "GWGUI.Emulation.Atari.Resources.Emulation");

    internal static string UnknownFirmware => FirmwareInvalid();
    internal static string RequiredFirmwareMissing => FirmwareMissing();
    internal static string FirmwareFileMissing => FirmwareMissing();
    internal static string FirmwareFileUnreadable => FirmwareInvalid();
    internal static string FirmwareIdentityAmbiguous => FirmwareInvalid();
    internal static string FirmwareCannotBeSelected => FirmwareInvalid();
    internal static string DuplicateStModelDefinition => OptionInvalid();
    internal static string UnknownStModel => OptionInvalid();
    internal static string DuplicateHardwareModelDefinition => OptionInvalid();
    internal static string UnknownHardwareModel => OptionInvalid();
    internal static string DuplicateCompatibilityDefinition => OptionInvalid();
    internal static string UnknownCompatibilityModel => OptionInvalid();
    internal static string MissingUnavailableExplanation => OptionInvalid();
    internal static string InvalidCompatibilityControllerCount => OptionInvalid();
    internal static string UnsupportedSchema => OptionInvalid();
    internal static string EmptyFirmwarePath => FirmwareMissing();
    internal static string DuplicateFirmware => FirmwareInvalid();
    internal static string IncompatibleFirmware => FirmwareInvalid();
    internal static string EmptyMediaPath => ContentNotFound();
    internal static string DuplicateMediaSlot => ContentUnsupported();
    internal static string IncompatibleMedia => ContentUnsupported();
    internal static string InvalidControllerPort => OptionInvalid();
    internal static string UnsupportedControllerDevice => OptionInvalid();
    internal static string DuplicateControllerPort => OptionInvalid();
    internal static string InvalidControllerDeadZone => OptionInvalid();
    internal static string CorePathMustBeAbsolute => CoreNotFound();
    internal static string CoreFileMissing => CoreNotFound();
    internal static string CoreExportMissing => CoreRejected();
    internal static string CoreApiVersionUnsupported => CoreRejected();
    internal static string CoreIdentityMismatch => CoreRejected();
    internal static string ContentFileMissing => ContentNotFound();
    internal static string ContentExtensionUnsupported => ContentUnsupported();
    internal static string OptionValueInvalidFormat => OptionInvalid();
    internal static string CoreAlreadyInitialized => CoreRejected();
    internal static string CoreNotInitialized => CoreRejected();
    internal static string ContentLoadFailed => ContentUnsupported();
    internal static string ContentRequired => ContentUnsupported();
    internal static string DynamicMediaUnsupported => ContentUnsupported();
    internal static string HatariFloppyRequired => ContentUnsupported();
    internal static string StorageExtensionInvalid => ContentUnsupported();
    internal static string StateInvalid => StateInvalidText();
    internal static string StateSizeInvalid => StateInvalidText();
    internal static string StateUnavailable => StateInvalidText();
    internal static string StateSaveFailed => StateInvalidText();
    internal static string StateLoadFailed => StateInvalidText();
    internal static string StateIncompatible =>
        Text("Emulation.Atari.Error.StateIncompatible");
    internal static string MachineInvalidState =>
        Text("Emulation.Atari.Error.Unexpected");
    internal static string MachineStopped =>
        Text("Emulation.Atari.Error.Unexpected");

    private static string FirmwareMissing() => Text("Emulation.Atari.Error.FirmwareMissing");
    private static string FirmwareInvalid() => Text("Emulation.Atari.Error.FirmwareInvalid");
    private static string ContentNotFound() => Text("Emulation.Atari.Error.ContentNotFound");
    private static string ContentUnsupported() => Text("Emulation.Atari.Error.ContentUnsupported");
    private static string OptionInvalid() => Text("Emulation.Atari.Error.OptionInvalid");
    private static string CoreNotFound() => Text("Emulation.Atari.Error.CoreNotFound");
    private static string CoreRejected() => Text("Emulation.Atari.Error.CoreRejected");
    private static string StateInvalidText() => Text("Emulation.Atari.Error.StateInvalid");

    private static string Text(string resourceKey)
    {
        return Localization.TryGetString(resourceKey, System.Globalization.CultureInfo.CurrentUICulture,
            out var value)
            ? value
            : resourceKey;
    }
}
