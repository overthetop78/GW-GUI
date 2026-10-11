using System.IO;
using System.Security.Cryptography;

namespace GWGUI.Emulation.Sinclair.Common.Machines.Common.Functions;

internal static class FirmwareFunctions
{
    internal static string Field(FirmwareCategory role) => FirmwareConstants.FieldPrefix + role;
    internal static bool IsField(string field) => Enum.GetValues<FirmwareCategory>().Any(role => field == Field(role));
    internal static IReadOnlyList<FirmwareCategory> MachineRoles(MachineModel model) => model switch
    { MachineModel.Pentagon128K => [FirmwareCategory.Pentagon0, FirmwareCategory.Pentagon1, FirmwareCategory.TrDos], MachineModel.Pentagon512K or MachineModel.Pentagon1024K => [FirmwareCategory.Pentagon0, FirmwareCategory.Pentagon1, FirmwareCategory.TrDos, FirmwareCategory.Gluck], MachineModel.Scorpion256K => [FirmwareCategory.Scorpion0, FirmwareCategory.Scorpion1, FirmwareCategory.Scorpion2, FirmwareCategory.Scorpion3], _ => []
    };
    internal static IReadOnlyList<FirmwareCategory> Roles(MachineModel model) => ConfigurationFunctions.Family(model) == MachineFamily.Spectrum ? MachineRoles(model).Concat(new[] { FirmwareCategory.USpeech, FirmwareCategory.Sp0256 }).ToArray() : [];
    internal static bool IsRequired(MachineModel model, FirmwareCategory role) => MachineRoles(model).Contains(role);
    internal static int RequiredSize(FirmwareCategory role) => role is FirmwareCategory.USpeech or FirmwareCategory.Sp0256 ? FirmwareConstants.SpeechBankBytes : FirmwareConstants.MachineBankBytes;
    internal static string FileName(FirmwareCategory role) => role switch
    { FirmwareCategory.Pentagon0 => FirmwareConstants.Pentagon0FileName, FirmwareCategory.Pentagon1 => FirmwareConstants.Pentagon1FileName, FirmwareCategory.TrDos => FirmwareConstants.TrDosFileName, FirmwareCategory.Gluck => FirmwareConstants.GluckFileName, FirmwareCategory.Scorpion0 => FirmwareConstants.Scorpion0FileName, FirmwareCategory.Scorpion1 => FirmwareConstants.Scorpion1FileName, FirmwareCategory.Scorpion2 => FirmwareConstants.Scorpion2FileName, FirmwareCategory.Scorpion3 => FirmwareConstants.Scorpion3FileName, FirmwareCategory.USpeech => FirmwareConstants.USpeechFileName, FirmwareCategory.Sp0256 => FirmwareConstants.Sp0256FileName, _ => throw new ArgumentOutOfRangeException(nameof(role))
    };

    internal static IReadOnlyList<FirmwareConfiguration> Apply(MachineConfiguration configuration, IReadOnlyDictionary<string, string?> values)
    {
        var result = configuration.Firmwares.ToList();
        foreach (var role in Roles(configuration.Model))
        {
            if (!values.TryGetValue(Field(role), out var path)) continue;
            result.RemoveAll(item => item.Category == role);
            if (!string.IsNullOrWhiteSpace(path)) result.Add(new(role, path, IsRequired(configuration.Model, role), IsOriginalFirmware: false));
        }
        return result;
    }

    internal static async ValueTask<EmulationFirmwareCandidate> ScanFileAsync(string path, MachineModel model, Emulator core, CancellationToken cancellationToken)
    {
        if (!ConfigurationFunctions.IsCompatible(model, core)) throw new ArgumentOutOfRangeException(nameof(core));
        var absolute = Path.GetFullPath(path);
        string? hash = null;
        string? destination = null;
        var compatibility = EmulationFirmwareCompatibility.Unknown;
        try
        {
            var length = new FileInfo(absolute).Length;
            foreach (var role in Roles(model))
            {
                if (!Path.GetFileName(absolute).Equals(FileName(role), StringComparison.OrdinalIgnoreCase)) continue;
                if (length != RequiredSize(role)) { compatibility = EmulationFirmwareCompatibility.Incompatible; break; }
                await using var stream = File.OpenRead(absolute);
                hash = Convert.ToHexString(await SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false));
                destination = Field(role);
                compatibility = EmulationFirmwareCompatibility.PartiallyCompatible;
                break;
            }
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
        return new(hash ?? absolute, absolute, Path.GetFileName(absolute), null, compatibility, destination);
    }

    internal static FirmwareConfiguration Select(EmulationFirmwareCandidate candidate, MachineModel model)
    {
        if (candidate.Compatibility is not EmulationFirmwareCompatibility.Compatible and not EmulationFirmwareCompatibility.PartiallyCompatible || !IsField(candidate.DestinationFieldId ?? string.Empty)) throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareCannotBeSelected);
        var role = Roles(model).Single(role => Field(role) == candidate.DestinationFieldId);
        return new(role, candidate.Path, IsRequired(model, role), IsOriginalFirmware: false);
    }
}
