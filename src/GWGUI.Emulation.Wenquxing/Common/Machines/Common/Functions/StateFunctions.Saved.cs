using System.IO;
using System.Security.Cryptography;
using System.Text.Json;

namespace GWGUI.Emulation.Wenquxing.Common.Machines.Common.Functions;
internal static class SavedStateFunctions
{
    internal static SavedStateHeader CreateHeader(MachineConfiguration configuration,
        IEmulatorCore core,
        ReadOnlySpan<byte> state) => new(StateConstants.CurrentFormatVersion, core.Emulator, core.CoreName, core.CoreVersion, core.CoreSha256, configuration.Model, ConfigurationHash(configuration), ContentHash(configuration), HashBytes(state));
    internal static void Validate(SavedStateHeader header, MachineConfiguration configuration, IEmulatorCore core)
    {
        if (header.FormatVersion != StateConstants.CurrentFormatVersion)
            throw Invalid(EmulationMessageCode.SavedStateIncompatible, ErrorMessages.StateIncompatible);
        if (header.Core != core.Emulator || !string.Equals(header.CoreName, core.CoreName, StringComparison.Ordinal) || !string.Equals(header.CoreVersion, core.CoreVersion, StringComparison.Ordinal) || !string.Equals(header.CoreSha256, core.CoreSha256, StringComparison.OrdinalIgnoreCase))
            throw Invalid(EmulationMessageCode.SavedStateIncompatible, ErrorMessages.StateIncompatible);
        if (header.Model != configuration.Model)
            throw Invalid(EmulationMessageCode.SavedStateIncompatible, ErrorMessages.StateIncompatible);
        if (!string.Equals(header.ContentSha256, ContentHash(configuration), StringComparison.OrdinalIgnoreCase))
            throw Invalid(EmulationMessageCode.SavedStateIncompatible, ErrorMessages.StateIncompatible);
        if (!IsCompatibleConfigurationHash(configuration, header.ConfigurationSha256))
            throw Invalid(EmulationMessageCode.SavedStateIncompatible, ErrorMessages.StateIncompatible);
    }

    internal static void ValidatePayloadSize(int length)
    {
        if (length <= StateConstants.EmptyLength)
            throw Invalid(EmulationMessageCode.SavedStateInvalid, ErrorMessages.StateInvalid);
        if (length > Array.MaxLength)
            throw Invalid(EmulationMessageCode.SavedStateInvalid, ErrorMessages.StateInvalid);
    }

    internal static EmulationMessageException Invalid(EmulationMessageCode code,
        string message,
        Exception? innerException = null) => new(new EmulationMessage(EmulationMessageCategory.SavedState, code, EmulationMessageSeverity.Error, EmulationMessageTarget.Dialog), innerException ?? new InvalidDataException(message));
    internal static string HashBytes(ReadOnlySpan<byte> value) => Convert.ToHexString(SHA256.HashData(value));
    internal static string ConfigurationHash(MachineConfiguration configuration)
    {
        var fingerprint = new StateConfigurationFingerprint(
            configuration.SchemaVersion,
            configuration.Model,
            configuration.Core,
            configuration.AudioEnabled,
            ContentEntries(configuration),
            configuration.Options.OrderBy(pair => pair.Key, StringComparer.Ordinal).ToArray(),
            InputFingerprint(configuration.Input, configuration.Model),
            configuration.Media.OrderBy(MediaOrder).ThenBy(media => media.Path, StringComparer.OrdinalIgnoreCase).ToArray()
        );
        return HashBytes(JsonSerializer.SerializeToUtf8Bytes(fingerprint, StateConstants.JsonOptions));
    }

    internal static bool IsCompatibleConfigurationHash(MachineConfiguration configuration, string hash) => string.Equals(hash, ConfigurationHash(configuration), StringComparison.OrdinalIgnoreCase);
    internal static string ContentHash(MachineConfiguration configuration) => HashBytes(JsonSerializer.SerializeToUtf8Bytes(ContentEntries(configuration), StateConstants.JsonOptions));
    private static IReadOnlyList<StateContentEntry> ContentEntries(MachineConfiguration configuration) => configuration.Firmware.Images.OrderBy(image => image.Key).Select(image => new StateContentEntry(image.Key.ToString(), "firmware", HashFile(image.Value))).ToArray();
    private static EmulationMediaSlot MediaOrder(MediaConfiguration media) => media.Slot;
    internal static bool IsHeaderValid(SavedStateHeader header) => header.FormatVersion
        > StateConstants.EmptyLength
        && Enum.IsDefined(header.Core)
        && Enum.IsDefined(header.Model)
        && !string.IsNullOrWhiteSpace(header.CoreName)
        && !string.IsNullOrWhiteSpace(header.CoreVersion)
        && !string.IsNullOrWhiteSpace(header.CoreSha256)
        && !string.IsNullOrWhiteSpace(header.ConfigurationSha256)
        && !string.IsNullOrWhiteSpace(header.ContentSha256)
        && !string.IsNullOrWhiteSpace(header.StateSha256);
    private static StateInputFingerprint InputFingerprint(InputConfiguration input, MachineModel model) => new(KeyboardDictionary.Bindings(input, model).OrderBy(pair => pair.Key, StringComparer.Ordinal).ToArray());
    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }
}