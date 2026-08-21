using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GWGUI.Emulation.Atari.Cores;

namespace GWGUI.Emulation.Atari;

internal static class AtariSavedStateFunctions
{
    internal static AtariSavedStateHeader CreateHeader(AtariMachineConfiguration configuration,
        IAtariCore core, ReadOnlySpan<byte> state) => new(
        AtariStateConstants.CurrentFormatVersion,
        core.Emulator,
        core.CoreName,
        core.CoreVersion,
        core.CoreSha256,
        configuration.Model,
        ConfigurationHash(configuration),
        ContentHash(configuration),
        HashBytes(state));

    internal static void Validate(AtariSavedStateHeader header, AtariMachineConfiguration configuration,
        IAtariCore core)
    {
        if (header.FormatVersion != AtariStateConstants.CurrentFormatVersion)
            throw Invalid(AtariErrorCode.StateIncompatible, AtariStateConstants.UnsupportedFormatError);
        if (header.Core != core.Emulator || !string.Equals(header.CoreName, core.CoreName, StringComparison.Ordinal)
            || !string.Equals(header.CoreVersion, core.CoreVersion, StringComparison.Ordinal)
            || !string.Equals(header.CoreSha256, core.CoreSha256, StringComparison.OrdinalIgnoreCase))
            throw Invalid(AtariErrorCode.StateIncompatible, AtariStateConstants.CoreMismatchError);
        if (header.Model != configuration.Model)
            throw Invalid(AtariErrorCode.StateIncompatible, AtariStateConstants.ModelMismatchError);
        if (!string.Equals(header.ContentSha256, ContentHash(configuration), StringComparison.OrdinalIgnoreCase))
            throw Invalid(AtariErrorCode.StateIncompatible, AtariStateConstants.ContentMismatchError);
        if (!string.Equals(header.ConfigurationSha256, ConfigurationHash(configuration),
                StringComparison.OrdinalIgnoreCase))
            throw Invalid(AtariErrorCode.StateIncompatible, AtariStateConstants.ConfigurationMismatchError);
    }

    internal static void ValidatePayloadSize(int length)
    {
        if (length <= AtariStateConstants.EmptyLength)
            throw Invalid(AtariErrorCode.StateInvalid, AtariStateConstants.EmptyPayloadError);
        if (length > AtariConstants.MaximumStateSize)
            throw Invalid(AtariErrorCode.StateInvalid, AtariStateConstants.PayloadTooLargeError);
    }

    internal static AtariEmulationException Invalid(AtariErrorCode code, string message,
        Exception? innerException = null) => new(AtariErrorCategory.State, code, message, innerException: innerException);

    internal static string HashBytes(ReadOnlySpan<byte> value) =>
        Convert.ToHexString(SHA256.HashData(value));

    internal static string ConfigurationHash(AtariMachineConfiguration configuration)
    {
        var fingerprint = new AtariStateConfigurationFingerprint(
            configuration.SchemaVersion,
            configuration.Model,
            configuration.Core,
            configuration.AudioEnabled,
            configuration.VideoRenderer,
            ContentEntries(configuration),
            configuration.Options.OrderBy(pair => pair.Key, StringComparer.Ordinal).ToArray(),
            InputFingerprint(configuration.Input),
            configuration.Media.OrderBy(MediaOrder).ThenBy(media => media.Path, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            configuration.Firmwares.OrderBy(firmware => firmware.Category).ToArray());
        return HashBytes(JsonSerializer.SerializeToUtf8Bytes(fingerprint, AtariStateConstants.JsonOptions));
    }

    internal static string ContentHash(AtariMachineConfiguration configuration) =>
        HashBytes(JsonSerializer.SerializeToUtf8Bytes(ContentEntries(configuration),
            AtariStateConstants.JsonOptions));

    private static IReadOnlyList<AtariStateContentEntry> ContentEntries(
        AtariMachineConfiguration configuration)
    {
        var entries = configuration.Firmwares
            .OrderBy(firmware => firmware.Category)
            .Select(firmware => new AtariStateContentEntry(AtariStateConstants.FirmwareCategory,
                firmware.Category.ToString(), HashPath(firmware.Path)))
            .Concat(configuration.Media.OrderBy(MediaOrder)
                .ThenBy(media => media.Path, StringComparer.OrdinalIgnoreCase)
                .Select(media => new AtariStateContentEntry(AtariStateConstants.MediaCategory,
                    $"{media.Category}:{media.Slot}:{media.MountOrder}", HashPath(media.Path))))
            .ToArray();
        return entries;
    }

    private static int MediaOrder(AtariMediaConfiguration media) => media.MountOrder;

    internal static bool IsHeaderValid(AtariSavedStateHeader header) =>
        header.FormatVersion > AtariStateConstants.EmptyLength
        && Enum.IsDefined(header.Core)
        && Enum.IsDefined(header.Model)
        && !string.IsNullOrWhiteSpace(header.CoreName)
        && !string.IsNullOrWhiteSpace(header.CoreVersion)
        && !string.IsNullOrWhiteSpace(header.CoreSha256)
        && !string.IsNullOrWhiteSpace(header.ConfigurationSha256)
        && !string.IsNullOrWhiteSpace(header.ContentSha256)
        && !string.IsNullOrWhiteSpace(header.StateSha256);

    private static AtariStateInputFingerprint InputFingerprint(AtariInputConfiguration input) => new(
        (input.KeyboardMappings ?? new Dictionary<string, GWGUI.Emulation.EmulationKey>())
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => new KeyValuePair<string, string>(pair.Key, pair.Value.ToString()))
            .ToArray(),
        (input.Controllers ?? [])
            .OrderBy(controller => controller.Port)
            .Select(controller => new AtariStateControllerFingerprint(
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
        if (!Directory.Exists(path)) throw new FileNotFoundException(AtariStateConstants.ContentPathMissingError, path);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        Span<byte> length = stackalloc byte[sizeof(int)];
        foreach (var file in Directory.EnumerateFiles(path, AtariStateConstants.AllFilesSearchPattern,
                     SearchOption.AllDirectories)
                     .Order(StringComparer.OrdinalIgnoreCase))
        {
            var relative = Path.GetRelativePath(path, file)
                .Replace(Path.DirectorySeparatorChar, AtariStateConstants.CanonicalDirectorySeparator);
            var name = Encoding.UTF8.GetBytes(relative);
            BinaryPrimitives.WriteInt32LittleEndian(length, name.Length);
            hash.AppendData(length);
            hash.AppendData(name);
            using var stream = File.OpenRead(file);
            var buffer = new byte[AtariStateConstants.HashBufferSize];
            int read;
            while ((read = stream.Read(buffer)) > AtariStateConstants.EmptyLength)
                hash.AppendData(buffer.AsSpan(AtariStateConstants.FirstBufferIndex, read));
        }
        return Convert.ToHexString(hash.GetHashAndReset());
    }

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }
}
