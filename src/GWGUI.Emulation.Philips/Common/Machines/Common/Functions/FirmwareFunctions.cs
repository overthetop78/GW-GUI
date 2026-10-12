using System.IO;
using System.Security.Cryptography;
using System.IO.Compression;

namespace GWGUI.Emulation.Philips.Common.Machines.Common.Functions;

internal static class FirmwareFunctions
{
    internal static string Field(FirmwareCategory role) => MediaRuntimeConstants.ConsoleFirmwareFieldPrefix + role;
    internal static bool IsField(string field) => Enum.GetValues<FirmwareCategory>().Any(role => field == Field(role));
    internal static IReadOnlyList<FirmwareCategory> Roles(MachineModel model) =>
        ConfigurationFunctions.Family(model) switch
        {
            MachineFamily.P2000 => [],
            MachineFamily.Videopac => [FirmwareCategory.SystemBios, FirmwareCategory.VoiceSamples],
            _ => [FirmwareCategory.SystemBios]
        };

    internal static IReadOnlyList<FirmwareConfiguration> Apply(MachineConfiguration configuration, IReadOnlyDictionary<string, string?> values)
    {
        var result = configuration.Firmwares.ToList();
        foreach (var role in Roles(configuration.Model))
        {
            if (!values.TryGetValue(Field(role), out var path)) continue;
            result.RemoveAll(item => item.Category == role);
            if (!string.IsNullOrWhiteSpace(path)) result.Add(new(role, path, role == FirmwareCategory.SystemBios));
        }
        return result;
    }

    internal static async ValueTask<EmulationFirmwareCandidate> ScanFileAsync(string path, MachineModel model, Emulator core, CancellationToken cancellationToken)
    {
        var absolute = Path.GetFullPath(path);
        var compatibility = EmulationFirmwareCompatibility.Incompatible;
        string? hash = null;
        string? destination = null;
        try
        {
            if (ConfigurationFunctions.Family(model) == MachineFamily.Videopac)
            {
                if (new FileInfo(absolute).Length == MediaRuntimeConstants.VideopacBiosBytes)
                {
                    var bytes = await File.ReadAllBytesAsync(absolute, cancellationToken).ConfigureAwait(false);
                    hash = Convert.ToHexString(SHA256.HashData(bytes));
                    if (Convert.ToHexString(MD5.HashData(bytes)).Equals(MediaRuntimeConstants.Md5(model), StringComparison.OrdinalIgnoreCase))
                        compatibility = EmulationFirmwareCompatibility.Compatible;
                }
                if (compatibility != EmulationFirmwareCompatibility.Compatible)
                {
                    using var archive = ZipFile.OpenRead(absolute);
                    _ = VoiceSampleFunctions.Entries(archive);
                    compatibility = EmulationFirmwareCompatibility.Compatible;
                    destination = Field(FirmwareCategory.VoiceSamples);
                }
            }
            else if (ConfigurationFunctions.Family(model) == MachineFamily.Cdi)
            {
                using var archive = ZipFile.OpenRead(absolute);
                var cpu = archive.Entries.Any(entry => Match(entry, MediaRuntimeConstants.Cdi200RomFileName, MediaRuntimeConstants.CdiCpuRomBytes, MediaRuntimeConstants.Cdi200Sha1)
                    || Match(entry, MediaRuntimeConstants.Cdi220RomFileName, MediaRuntimeConstants.CdiCpuRomBytes, MediaRuntimeConstants.Cdi220Sha1));
                var controllers = core != Emulator.SameCdi ||
                    (archive.Entries.Any(entry => Match(entry, MediaRuntimeConstants.CdiServoRomFileName, MediaRuntimeConstants.CdiControllerRomBytes, MediaRuntimeConstants.CdiServoSha1))
                    && archive.Entries.Any(entry => Match(entry, MediaRuntimeConstants.CdiSlaveRomFileName, MediaRuntimeConstants.CdiControllerRomBytes, MediaRuntimeConstants.CdiSlaveSha1)));
                if (cpu && controllers) compatibility = EmulationFirmwareCompatibility.Compatible;
            }
            if (compatibility == EmulationFirmwareCompatibility.Compatible && destination is null)
                destination = Field(FirmwareCategory.SystemBios);
        }
        catch (InvalidDataException) { }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
        return new(hash ?? absolute, absolute, Path.GetFileName(absolute), null, compatibility, destination);
    }

    private static bool Match(ZipArchiveEntry entry, string fileName, long length, string sha1)
    {
        if (!entry.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase) || entry.Length != length) return false;
        using var stream = entry.Open();
        return Convert.ToHexString(SHA1.HashData(stream)).Equals(sha1, StringComparison.OrdinalIgnoreCase);
    }

    internal static FirmwareConfiguration Select(EmulationFirmwareCandidate candidate)
    {
        if (candidate.Compatibility != EmulationFirmwareCompatibility.Compatible
            || !IsField(candidate.DestinationFieldId ?? string.Empty))
            throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareCannotBeSelected);
        var role = Enum.GetValues<FirmwareCategory>().Single(role => Field(role) == candidate.DestinationFieldId);
        return new(role, candidate.Path, role == FirmwareCategory.SystemBios, true);
    }
}
