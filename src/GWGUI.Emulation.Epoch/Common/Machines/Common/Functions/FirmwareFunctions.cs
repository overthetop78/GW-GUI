using CommonCoreHostFunctions = GWGUI.Emulation.Epoch.Common.Functions.CoreHostFunctions;
using System.IO;
using System.Security.Cryptography;

namespace GWGUI.Emulation.Epoch.Common.Machines.Common.Functions;

internal static class FirmwareFunctions
{
    internal static string Field(FirmwareCategory role) => FirmwareConstants.ConsoleFirmwareFieldPrefix + role;
    internal static bool IsField(string field) => field == Field(FirmwareCategory.Console);
    internal static IReadOnlyList<FirmwareCategory> Roles(MachineModel model) => model is MachineModel.SuperCassetteVision or MachineModel.SuperCassetteVisionYeno or MachineModel.SuperCassetteVisionLady ? [FirmwareCategory.Console] : [];

    internal static IReadOnlyList<FirmwareConfiguration> Apply(MachineConfiguration configuration, IReadOnlyDictionary<string, string?> values)
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

    internal static async ValueTask<EmulationFirmwareCandidate> ScanFileAsync(string path, MachineModel model, CancellationToken cancellationToken)
    {
        var absolute = Path.GetFullPath(path);
        var compatibility = EmulationFirmwareCompatibility.Incompatible;
        string? hash = null;
        try
        {
            if (Roles(model).Count > 0 && new FileInfo(absolute).Length == FirmwareConstants.ConsoleFirmwareBytes)
            {
                var bytes = await File.ReadAllBytesAsync(absolute, cancellationToken).ConfigureAwait(false);
                hash = CommonCoreHostFunctions.StateFiles.HashBytes(bytes);
                compatibility = Convert.ToHexString(MD5.HashData(bytes)).Equals(FirmwareConstants.ConsoleFirmwareMd5, StringComparison.OrdinalIgnoreCase) ? EmulationFirmwareCompatibility.Compatible : EmulationFirmwareCompatibility.PartiallyCompatible;
            }
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
        return new(hash ?? absolute, absolute, Path.GetFileName(absolute), null, compatibility, compatibility == EmulationFirmwareCompatibility.Incompatible ? null : Field(FirmwareCategory.Console));
    }

    internal static FirmwareConfiguration Select(EmulationFirmwareCandidate candidate)
    {
        if (candidate.Compatibility == EmulationFirmwareCompatibility.Incompatible || !IsField(candidate.DestinationFieldId ?? string.Empty))
            throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareCannotBeSelected);
        return new(FirmwareCategory.Console, candidate.Path, true, candidate.Compatibility == EmulationFirmwareCompatibility.Compatible);
    }
}
