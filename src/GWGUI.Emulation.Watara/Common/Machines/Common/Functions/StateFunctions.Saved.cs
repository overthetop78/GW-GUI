using System.IO;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GWGUI.Emulation.Watara.Common.Machines.Common.Functions;
internal static class SavedStateFunctions
{
    internal static SavedStateHeader CreateHeader(MachineConfiguration configuration,
        IEmulatorCore core,
        ReadOnlySpan<byte> state) => new(StateConstants.CurrentFormatVersion, core.Emulator, core.CoreName, core.CoreVersion, core.CoreSha256, configuration.Model, ConfigurationHash(configuration), ContentHash(configuration), HashBytes(state));
    internal static void Validate(SavedStateHeader header, MachineConfiguration configuration, IEmulatorCore core)
    {
        if (header.FormatVersion != StateConstants.CurrentFormatVersion)
            throw Invalid(EmulationMessageCode.SavedStateIncompatible, ErrorMessages.StateIncompatible);
        if (!EmulationStateFileFunctions.CoreIdentityMatches(header.Core == core.Emulator, header.CoreName, header.CoreVersion, header.CoreSha256, core))
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
        Exception? innerException = null) => GWGUI.Emulation.Services.EmulationErrorService.CreateError(EmulationMessageCategory.SavedState, code, innerException ?? new InvalidDataException(message));
    internal static string HashBytes(ReadOnlySpan<byte> bytes) => EmulationStateFileFunctions.HashBytes(bytes);
    internal static string ConfigurationHash(MachineConfiguration configuration)
    {
        var fingerprint = new StateConfigurationFingerprint(
            configuration.SchemaVersion,
            configuration.Model,
            configuration.Core,
            configuration.AudioEnabled,
            ContentEntries(configuration),
            configuration.Options.OrderBy(pair => pair.Key, StringComparer.Ordinal).ToArray(),
            InputFingerprint(configuration.Input),
            configuration.Media.OrderBy(MediaOrder).ThenBy(media => media.Path, StringComparer.OrdinalIgnoreCase).ToArray()
        );
        return HashBytes(JsonSerializer.SerializeToUtf8Bytes(fingerprint, StateConstants.JsonOptions));
    }

    internal static bool IsCompatibleConfigurationHash(MachineConfiguration configuration, string hash) => string.Equals(hash, ConfigurationHash(configuration), StringComparison.OrdinalIgnoreCase);
    internal static string ContentHash(MachineConfiguration configuration) => HashBytes(JsonSerializer.SerializeToUtf8Bytes(ContentEntries(configuration), StateConstants.JsonOptions));
    private static IReadOnlyList<StateContentEntry> ContentEntries(MachineConfiguration configuration) => configuration.Media.Where(media => media.IsInserted).OrderBy(MediaOrder).ThenBy(media => media.Path,
        StringComparer.OrdinalIgnoreCase).Select(media => new StateContentEntry(StateConstants.MediaCategory, $"{media.Category}:{media.Slot}", HashPath(media.Path))).ToArray();
    private static EmulationMediaSlot MediaOrder(MediaConfiguration media) => media.Slot;
    internal static bool IsHeaderValid(SavedStateHeader header) => EmulationStateFileFunctions.IsHeaderValid(header);
    private static StateInputFingerprint InputFingerprint(InputConfiguration input) => new((input.Controllers ?? []).OrderBy(controller => controller.Port).Select(controller => new StateControllerFingerprint(controller.Port,
        controller.Peripheral,
        controller.DeviceId,
        controller.DeadZonePercent,
        EmulationStateFileFunctions.OrderInputMappings(controller.Mappings).ToArray())).ToArray());
    private static string HashPath(string path) =>
        EmulationStateFileFunctions.HashPath(path, StateConstants.AllFilesSearchPattern, StateConstants.CanonicalDirectorySeparator, StateConstants.HashBufferSize, () => new FileNotFoundException(ErrorMessages.ContentFileMissing, path));


}
