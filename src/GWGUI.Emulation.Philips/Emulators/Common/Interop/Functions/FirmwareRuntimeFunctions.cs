using GWGUI.Emulation.Philips.Emulators.Common.Interop.Constants;
using System.IO;
using System.IO.Compression;

namespace GWGUI.Emulation.Philips.Emulators.Common.Interop.Functions;

internal static class FirmwareRuntimeFunctions
{
    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        if (configuration.Family == MachineFamily.P2000) return;
        var firmware = configuration.Firmwares.SingleOrDefault(item => item.Category == FirmwareCategory.SystemBios);
        if (firmware is null || !File.Exists(firmware.Path))
            throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareMissing, ErrorMessages.RequiredFirmwareMissing);
        var source = Path.GetFullPath(firmware.Path);
        if (configuration.Family == MachineFamily.Videopac)
        {
            if (new FileInfo(source).Length != MediaRuntimeConstants.VideopacBiosBytes)
                throw InvalidFirmware();
        }
        else
        {
            try
            {
                using var archive = ZipFile.OpenRead(source);
                var entries = archive.Entries.ToDictionary(entry => entry.Name, StringComparer.OrdinalIgnoreCase);
                var hasCpu = HasSize(entries, MediaRuntimeConstants.Cdi200RomFileName, MediaRuntimeConstants.CdiCpuRomBytes) || HasSize(entries, MediaRuntimeConstants.Cdi220RomFileName, MediaRuntimeConstants.CdiCpuRomBytes);
                if (!hasCpu || (configuration.Core == Emulator.SameCdi
                    && (!HasSize(entries, MediaRuntimeConstants.CdiSlaveRomFileName, MediaRuntimeConstants.CdiControllerRomBytes)
                    || !HasSize(entries, MediaRuntimeConstants.CdiServoRomFileName, MediaRuntimeConstants.CdiControllerRomBytes))))
                    throw InvalidFirmware();
            }
            catch (InvalidDataException) { throw InvalidFirmware(); }
        }
        var directory = configuration.Core == Emulator.SameCdi ? Path.Combine(systemDirectory, MediaRuntimeConstants.SameCdiDirectoryName, MediaRuntimeConstants.CdiBiosDirectoryName) : systemDirectory;
        Directory.CreateDirectory(directory);
        var destination = Path.Combine(directory, MediaRuntimeConstants.FileName(configuration.Model));
        if (!string.Equals(source, Path.GetFullPath(destination), StringComparison.OrdinalIgnoreCase))
            File.Copy(source, destination, overwrite: true);
        if (configuration.Family == MachineFamily.Videopac)
        {
            var voice = configuration.Firmwares.SingleOrDefault(item => item.Category == FirmwareCategory.VoiceSamples);
            if (voice is not null) PrepareVoice(voice.Path, systemDirectory);
        }
    }

    private static void PrepareVoice(string path, string systemDirectory)
    {
        try
        {
            using var archive = ZipFile.OpenRead(path);
            var entries = VoiceSampleFunctions.Entries(archive);
            var directory = Path.Combine(systemDirectory, VoiceSampleConstants.DirectoryName);
            Directory.CreateDirectory(directory);
            foreach (var entry in entries)
            {
                // Only native file names are retained; archive directories cannot escape the session.
                var destination = Path.Combine(directory, entry.Name.ToUpperInvariant());
                using var source = entry.Open();
                using var output = File.Create(destination);
                source.CopyTo(output);
            }
        }
        catch (InvalidDataException) { throw InvalidFirmware(); }
        catch (IOException) { throw InvalidFirmware(); }
        catch (UnauthorizedAccessException) { throw InvalidFirmware(); }
    }

    private static bool HasSize(IReadOnlyDictionary<string, ZipArchiveEntry> entries, string name, long bytes) => entries.TryGetValue(name, out var entry) && entry.Length == bytes;

    private static EmulationException InvalidFirmware() => new(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareFileUnreadable);
}
