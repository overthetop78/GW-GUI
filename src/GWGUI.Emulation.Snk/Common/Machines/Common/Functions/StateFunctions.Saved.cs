using System.IO;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GWGUI.Emulation.Snk.Common.Machines.Common.Functions;
internal static class SavedStateFunctions
{
    internal static SavedStateHeader CreateHeader(MachineConfiguration configuration,
        IEmulatorCore core,
        ReadOnlySpan<byte> state) => new(StateConstants.CurrentFormatVersion, core.Emulator, core.CoreName, core.CoreVersion, core.CoreSha256, configuration.Model, ConfigurationHash(configuration), ContentHash(configuration), HashBytes(state));
    internal static void Validate(SavedStateHeader header, MachineConfiguration configuration, IEmulatorCore core)
    {
        if (header.FormatVersion != StateConstants.CurrentFormatVersion)
            throw Invalid(ErrorCode.StateIncompatible, ErrorMessages.StateIncompatible);
        if (header.Core != core.Emulator || !string.Equals(header.CoreName, core.CoreName, StringComparison.Ordinal) || !string.Equals(header.CoreVersion, core.CoreVersion, StringComparison.Ordinal) || !string.Equals(header.CoreSha256, core.CoreSha256, StringComparison.OrdinalIgnoreCase))
            throw Invalid(ErrorCode.StateIncompatible, ErrorMessages.StateIncompatible);
        if (header.Model != configuration.Model)
            throw Invalid(ErrorCode.StateIncompatible, ErrorMessages.StateIncompatible);
        if (!string.Equals(header.ContentSha256, ContentHash(configuration), StringComparison.OrdinalIgnoreCase))
            throw Invalid(ErrorCode.StateIncompatible, ErrorMessages.StateIncompatible);
        if (!IsCompatibleConfigurationHash(configuration, header.ConfigurationSha256))
            throw Invalid(ErrorCode.StateIncompatible, ErrorMessages.StateIncompatible);
    }

    internal static void ValidatePayloadSize(int length)
    {
        if (length <= StateConstants.EmptyLength)
            throw Invalid(ErrorCode.StateInvalid, ErrorMessages.StateInvalid);
        if (length > SavedStateConstants.MaximumStateSize)
            throw Invalid(ErrorCode.StateInvalid, ErrorMessages.StateInvalid);
    }

    internal static EmulationException Invalid(ErrorCode code, string message, Exception? innerException = null) => new(ErrorCategory.State, code, message, innerException: innerException);
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
            configuration.Media.OrderBy(MediaOrder).ThenBy(media => media.Path, StringComparer.OrdinalIgnoreCase).ToArray(),
            configuration.Firmwares.OrderBy(firmware => firmware.Category).ToArray()
        );
        return HashBytes(JsonSerializer.SerializeToUtf8Bytes(fingerprint, StateConstants.JsonOptions));
    }

    internal static bool IsCompatibleConfigurationHash(MachineConfiguration configuration, string hash) => string.Equals(hash, ConfigurationHash(configuration), StringComparison.OrdinalIgnoreCase);
    internal static string ContentHash(MachineConfiguration configuration) => HashBytes(JsonSerializer.SerializeToUtf8Bytes(ContentEntries(configuration), StateConstants.JsonOptions));
    private static IReadOnlyList<StateContentEntry> ContentEntries(MachineConfiguration configuration)
    {
        IReadOnlyList<string> initialPaths = configuration.InitialContent is null ? Array.Empty<string>() : new Engine().Adapter(configuration)is IEmulatorMediaAdapter adapter ? adapter.ResolveInitialContentPaths(configuration) : [configuration.InitialContent];
        var entries = configuration.Firmwares.OrderBy(firmware => firmware.Category).Select(firmware => new StateContentEntry(StateConstants.FirmwareCategory, firmware.Category.ToString(), HashPath(firmware.Path))).Concat(configuration.Media.OrderBy(MediaOrder).ThenBy(media => media.Path,
                StringComparer.OrdinalIgnoreCase).Select(media => new StateContentEntry(StateConstants.MediaCategory,
                $"{media.Category}:{media.Slot}",
                HashPath(media.Path)))).Concat(initialPaths.Select((path,
            index) => new StateContentEntry(StateConstants.InitialContentCategory, index.ToString(System.Globalization.CultureInfo.InvariantCulture) + ":" + Path.GetFileName(path), HashPath(path)))).ToArray();
        return entries;
    }

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
    private static StateInputFingerprint InputFingerprint(InputConfiguration input) => new((input.Controllers ?? []).OrderBy(controller => controller.Port).Select(controller => new StateControllerFingerprint(controller.Port,
        controller.Peripheral,
        controller.DeviceId,
        controller.DeadZonePercent,
        (controller.Mappings ?? new Dictionary<string, string>()).OrderBy(pair => pair.Key, StringComparer.Ordinal).ToArray())).ToArray());
    private static string HashPath(string path) =>
        EmulationStateFileFunctions.HashPath(path, StateConstants.AllFilesSearchPattern, StateConstants.CanonicalDirectorySeparator, StateConstants.HashBufferSize, () => new FileNotFoundException(ErrorMessages.ContentFileMissing, path));


}
