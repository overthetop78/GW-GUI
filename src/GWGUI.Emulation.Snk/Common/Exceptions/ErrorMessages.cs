using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Snk.Common.Functions.CoreHostFunctions;
namespace GWGUI.Emulation.Snk.Common.Exceptions;

internal static class ErrorMessages
{
    internal static string InvalidArchitecture => ExceptionText.Get("Emulation.Snk.Error.CoreRejected");
    internal static string MissingPublishedDate => ExceptionText.Get("Emulation.Snk.Error.CoreRejected");
    internal static string MissingExpectedLibraryFormat => ExceptionText.Get("Emulation.Snk.Error.CoreRejected");
    internal static string InstalledLibraryLockedFormat => ExceptionText.Get("Emulation.Snk.Error.CoreRejected");
    internal static string EmptyInstallationRoot => ExceptionText.Get("Emulation.Snk.Error.CoreNotFound");
    internal static string EmptyVersion => ExceptionText.Get("Emulation.Snk.Error.CoreNotFound");
    internal static string DuplicateCore => ExceptionText.Get("Emulation.Snk.Error.CoreRejected");

    private static readonly IEmulationModuleLocalization Localization = CommonCoreHostFunctions.CreateLocalization( typeof(ErrorMessages).Assembly, "GWGUI.Emulation.Snk.Resources.Emulation");

    internal static string UnknownFirmware => FirmwareInvalid();
    internal static string RequiredFirmwareMissing => FirmwareMissing();
    internal static string FirmwareFileMissing => FirmwareMissing();
    internal static string FirmwareFileUnreadable => FirmwareInvalid();
    internal static string FirmwareIdentityAmbiguous => FirmwareInvalid();
    internal static string FirmwareCannotBeSelected => FirmwareInvalid();
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
    internal static string StorageExtensionInvalid => ContentUnsupported();
    internal static string StateInvalid => StateInvalidText();
    internal static string StateSizeInvalid => StateInvalidText();
    internal static string StateUnavailable => StateInvalidText();
    internal static string StateSaveFailed => StateInvalidText();
    internal static string StateLoadFailed => StateInvalidText();
    internal static string StateIncompatible => Text("Emulation.Snk.Error.StateIncompatible");
    internal static string MachineInvalidState => Text("Emulation.Snk.Error.Unexpected");
    internal static string MachineStopped => Text("Emulation.Snk.Error.Unexpected");

    private static string FirmwareMissing() => Text("Emulation.Snk.Error.FirmwareMissing");
    private static string FirmwareInvalid() => Text("Emulation.Snk.Error.FirmwareInvalid");
    private static string ContentNotFound() => Text("Emulation.Snk.Error.ContentNotFound");
    private static string ContentUnsupported() => Text("Emulation.Snk.Error.ContentUnsupported");
    private static string OptionInvalid() => Text("Emulation.Snk.Error.OptionInvalid");
    private static string CoreNotFound() => Text("Emulation.Snk.Error.CoreNotFound");
    private static string CoreRejected() => Text("Emulation.Snk.Error.CoreRejected");
    private static string StateInvalidText() => Text("Emulation.Snk.Error.StateInvalid");

    private static string Text(string resourceKey)
    {
        return Localization.TryGetString(resourceKey, System.Globalization.CultureInfo.CurrentUICulture, out var value) ? value : resourceKey;
    }
}
