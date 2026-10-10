using System.IO;
using System.Security.Cryptography;

namespace GWGUI.Emulation.Fairchild.Common.Machines.Common.Functions;

internal static class FirmwareFunctions
{
    internal static string Field(FirmwareCategory role) => MediaRuntimeConstants.ConsoleFirmwareFieldPrefix + role;
    internal static bool IsField(string field) => Enum.GetValues<FirmwareCategory>().Any(role => field == Field(role));
    internal static IReadOnlyList<FirmwareCategory> Roles(MachineModel model) => Enum.IsDefined(model)
        ? [FirmwareCategory.Bios1, FirmwareCategory.Bios2] : throw new ArgumentOutOfRangeException(nameof(model));

    internal static IReadOnlyList<FirmwareConfiguration> Apply(MachineConfiguration configuration,
        IReadOnlyDictionary<string, string?> values)
    {
        var result = configuration.Firmwares.ToList();
        foreach (var role in Roles(configuration.Model))
        {
            if (!values.TryGetValue(Field(role), out var path)) continue;
            result.RemoveAll(item => item.Category == role);
            if (!string.IsNullOrWhiteSpace(path)) result.Add(new(role, path, true));
        }
        return result;
    }

    internal static async ValueTask<EmulationFirmwareCandidate> ScanFileAsync(string path, MachineModel model,
        CancellationToken cancellationToken)
    {
        var absolute = Path.GetFullPath(path);
        var compatibility = EmulationFirmwareCompatibility.Incompatible;
        string? hash = null;
        string? destination = null;
        try
        {
            if (new FileInfo(absolute).Length == MediaRuntimeConstants.BiosBytes)
            {
                var bytes = await File.ReadAllBytesAsync(absolute, cancellationToken).ConfigureAwait(false);
                hash = Convert.ToHexString(SHA256.HashData(bytes));
                var md5 = Convert.ToHexString(MD5.HashData(bytes));
                foreach (var role in Roles(model))
                {
                    if (!md5.Equals(MediaRuntimeConstants.Md5(model, role), StringComparison.OrdinalIgnoreCase)) continue;
                    compatibility = EmulationFirmwareCompatibility.Compatible;
                    destination = Field(role);
                    break;
                }
            }
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
        return new(hash ?? absolute, absolute, Path.GetFileName(absolute), null, compatibility, destination);
    }

    internal static FirmwareConfiguration Select(EmulationFirmwareCandidate candidate)
    {
        if (candidate.Compatibility != EmulationFirmwareCompatibility.Compatible
            || !IsField(candidate.DestinationFieldId ?? string.Empty))
            throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareCannotBeSelected);
        var role = Enum.GetValues<FirmwareCategory>().Single(role => Field(role) == candidate.DestinationFieldId);
        return new(role, candidate.Path, true, true);
    }
}