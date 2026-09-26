using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GWGUI.Emulation.Atari.Common.Machines.Common.Functions;

internal static class SavedStateFunctions
{
    internal static SavedStateHeader CreateHeader(MachineConfiguration configuration,
        IEmulatorCore core, ReadOnlySpan<byte> state) => new(
        StateConstants.CurrentFormatVersion,
        core.Emulator,
        core.CoreName,
        core.CoreVersion,
        core.CoreSha256,
        configuration.Model,
        ConfigurationHash(configuration),
        ContentHash(configuration),
        HashBytes(state));

    internal static void Validate(SavedStateHeader header, MachineConfiguration configuration,
        IEmulatorCore core)
    {
        if (header.FormatVersion != StateConstants.CurrentFormatVersion)
            throw Invalid(ErrorCode.StateIncompatible, ErrorMessages.StateIncompatible);
        if (header.Core != core.Emulator || !string.Equals(header.CoreName, core.CoreName, StringComparison.Ordinal)
            || !string.Equals(header.CoreVersion, core.CoreVersion, StringComparison.Ordinal)
            || !string.Equals(header.CoreSha256, core.CoreSha256, StringComparison.OrdinalIgnoreCase))
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

    internal static EmulationException Invalid(ErrorCode code, string message,
        Exception? innerException = null) => new(ErrorCategory.State, code, message, innerException: innerException);

    internal static string HashBytes(ReadOnlySpan<byte> value) =>
        Convert.ToHexString(SHA256.HashData(value));

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
            configuration.Media.OrderBy(MediaOrder).ThenBy(media => media.Path, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            configuration.Firmwares.OrderBy(firmware => firmware.Category).ToArray());
        return HashBytes(JsonSerializer.SerializeToUtf8Bytes(fingerprint, StateConstants.JsonOptions));
    }

    internal static bool IsCompatibleConfigurationHash(MachineConfiguration configuration, string hash)
    {
        if (string.Equals(hash, ConfigurationHash(configuration), StringComparison.OrdinalIgnoreCase))
            return true;
        // Format used before presentation profiles were moved out of the modules.
        // Try every historical numeric value; it never represented emulated machine state.
        for (var legacyValue = 0; legacyValue < StateConstants.LegacyPresentationValueCount; legacyValue++)
            if (string.Equals(hash, LegacyConfigurationHash(configuration, legacyValue),
                StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    internal static string LegacyConfigurationHash(MachineConfiguration configuration, int legacyValue)
    {
        var fingerprint = new StateConfigurationFingerprint(configuration.SchemaVersion,
            configuration.Model, configuration.Core, configuration.AudioEnabled,
            ContentEntries(configuration),
            configuration.Options.OrderBy(pair => pair.Key, StringComparer.Ordinal).ToArray(),
            InputFingerprint(configuration.Input),
            configuration.Media.OrderBy(MediaOrder).ThenBy(media => media.Path, StringComparer.OrdinalIgnoreCase).ToArray(),
            configuration.Firmwares.OrderBy(firmware => firmware.Category).ToArray());
        var document = JsonSerializer.SerializeToElement(fingerprint, StateConstants.JsonOptions);
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            foreach (var property in document.EnumerateObject())
            {
                property.WriteTo(writer);
                if (property.Name == StateConstants.LegacyAudioProperty)
                    writer.WriteNumber(StateConstants.LegacyPresentationProperty, legacyValue);
            }
            writer.WriteEndObject();
        }
        return HashBytes(stream.ToArray());
    }

    internal static string ContentHash(MachineConfiguration configuration) =>
        HashBytes(JsonSerializer.SerializeToUtf8Bytes(ContentEntries(configuration),
            StateConstants.JsonOptions));

    private static IReadOnlyList<StateContentEntry> ContentEntries(
        MachineConfiguration configuration)
    {
        var entries = configuration.Firmwares
            .OrderBy(firmware => firmware.Category)
            .Select(firmware => new StateContentEntry(StateConstants.FirmwareCategory,
                firmware.Category.ToString(), HashPath(firmware.Path)))
            .Concat(configuration.Media.OrderBy(MediaOrder)
                .ThenBy(media => media.Path, StringComparer.OrdinalIgnoreCase)
                .Select(media => new StateContentEntry(StateConstants.MediaCategory,
                    $"{media.Category}:{media.Slot}:{media.MountOrder}", HashPath(media.Path))))
            .ToArray();
        return entries;
    }

    private static int MediaOrder(MediaConfiguration media) => media.MountOrder;

    internal static bool IsHeaderValid(SavedStateHeader header) =>
        header.FormatVersion > StateConstants.EmptyLength
        && Enum.IsDefined(header.Core)
        && Enum.IsDefined(header.Model)
        && !string.IsNullOrWhiteSpace(header.CoreName)
        && !string.IsNullOrWhiteSpace(header.CoreVersion)
        && !string.IsNullOrWhiteSpace(header.CoreSha256)
        && !string.IsNullOrWhiteSpace(header.ConfigurationSha256)
        && !string.IsNullOrWhiteSpace(header.ContentSha256)
        && !string.IsNullOrWhiteSpace(header.StateSha256);

    private static StateInputFingerprint InputFingerprint(InputConfiguration input) => new(
        (input.KeyboardMappings ?? new Dictionary<string, GWGUI.Emulation.Enums.EmulationKey>())
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => new KeyValuePair<string, string>(pair.Key, pair.Value.ToString()))
            .ToArray(),
        (input.Controllers ?? [])
            .OrderBy(controller => controller.Port)
            .Select(controller => new StateControllerFingerprint(
                controller.Port,
                controller.Peripheral,
                controller.DeviceId,
                controller.DeadZonePercent,
                (controller.Mappings ?? new Dictionary<string, string>())
                    .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                    .ToArray()))
            .ToArray(),
        input.MouseDeviceId,
        input.CaptureMouse,
        input.ReleaseMouseKey.ToString());

    private static string HashPath(string path)
    {
        if (File.Exists(path)) return HashFile(path);
        if (!Directory.Exists(path)) throw new FileNotFoundException(ErrorMessages.ContentFileMissing, path);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        Span<byte> length = stackalloc byte[sizeof(int)];
        foreach (var file in Directory.EnumerateFiles(path, StateConstants.AllFilesSearchPattern,
                     SearchOption.AllDirectories)
                     .Order(StringComparer.OrdinalIgnoreCase))
        {
            var relative = Path.GetRelativePath(path, file)
                .Replace(Path.DirectorySeparatorChar, StateConstants.CanonicalDirectorySeparator);
            var name = Encoding.UTF8.GetBytes(relative);
            BinaryPrimitives.WriteInt32LittleEndian(length, name.Length);
            hash.AppendData(length);
            hash.AppendData(name);
            using var stream = File.OpenRead(file);
            var buffer = new byte[StateConstants.HashBufferSize];
            int read;
            while ((read = stream.Read(buffer)) > StateConstants.EmptyLength)
                hash.AppendData(buffer.AsSpan(BufferConstants.FirstBufferIndex, read));
        }
        return Convert.ToHexString(hash.GetHashAndReset());
    }

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }
}
