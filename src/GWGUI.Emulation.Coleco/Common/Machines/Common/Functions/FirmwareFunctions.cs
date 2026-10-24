using CommonCoreHostFunctions = GWGUI.Emulation.Coleco.Common.Functions.CoreHostFunctions;
using System.IO;
using System.Security.Cryptography;

namespace GWGUI.Emulation.Coleco.Common.Machines.Common.Functions;

internal static class FirmwareFunctions
{
    internal static string Field(FirmwareCategory role) => "coleco_firmware_" + role;
    internal static bool IsField(string field) => Enum.GetValues<FirmwareCategory>().Any(role => Field(role) == field);
    internal static IReadOnlyList<FirmwareCategory> Roles(MachineModel model) => model == MachineModel.Adam ? [FirmwareCategory.Os7, FirmwareCategory.Eos, FirmwareCategory.SmartWriter] : [FirmwareCategory.Os7];

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
        FirmwareCategory? role = null;
        var compatibility = EmulationFirmwareCompatibility.Incompatible;
        string? hash = null;
        try
        {
            var length = new FileInfo(absolute).Length;
            if (length != FirmwareConstants.Os7Bytes && length != FirmwareConstants.SmartWriterBytes)
                return new(absolute, absolute, Path.GetFileName(absolute), null, compatibility, null);
            var bytes = await File.ReadAllBytesAsync(absolute, cancellationToken).ConfigureAwait(false);
            hash = CommonCoreHostFunctions.StateFiles.HashBytes(bytes);
            var crc = CommonCoreHostFunctions.FirmwareProcessing.ComputeCrc32(bytes);
            role = crc switch
            {
                0x3AA93EF3 => FirmwareCategory.Os7,
                0x05A37A34 => FirmwareCategory.Eos,
                0x58D86A2A => FirmwareCategory.SmartWriter,
                _ => null
            };
            if (role is not null && Roles(model).Contains(role.Value)) compatibility = EmulationFirmwareCompatibility.Compatible;
            else
            {
                var name = Path.GetFileNameWithoutExtension(absolute);
                role = bytes.Length == FirmwareConstants.SmartWriterBytes && model == MachineModel.Adam
                    ? FirmwareCategory.SmartWriter
                    : bytes.Length == FirmwareConstants.Os7Bytes ? name.Contains("eos", StringComparison.OrdinalIgnoreCase) && model == MachineModel.Adam ? FirmwareCategory.Eos : FirmwareCategory.Os7 : null;
                if (role is not null) compatibility = EmulationFirmwareCompatibility.PartiallyCompatible;
            }
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
        return new(hash ?? absolute, absolute, Path.GetFileName(absolute), null, compatibility, compatibility == EmulationFirmwareCompatibility.Incompatible || role is null ? null : Field(role.Value));
    }

    internal static FirmwareConfiguration Select(EmulationFirmwareCandidate candidate)
    {
        if (candidate.Compatibility == EmulationFirmwareCompatibility.Incompatible)
            throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareCannotBeSelected);
        foreach (var role in Enum.GetValues<FirmwareCategory>())
            if (candidate.DestinationFieldId == Field(role)) return new(role, candidate.Path, true);
        throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareCannotBeSelected);
    }


}
