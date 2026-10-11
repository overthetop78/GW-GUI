using System.IO;
using System.Security.Cryptography;

namespace GWGUI.Emulation.Sharp.Common.Machines.Common.Functions;

internal static class FirmwareFunctions
{
    internal static string Field(FirmwareCategory role) => FirmwareConstants.FieldPrefix + role;
    internal static bool IsField(string field) => Enum.GetValues<FirmwareCategory>().Any(role => field == Field(role));
    internal static IReadOnlyList<FirmwareCategory> Roles(MachineModel model) => ConfigurationFunctions.Family(model) == MachineFamily.X68000 ? [FirmwareCategory.Ipl, FirmwareCategory.Font] : [FirmwareCategory.Ipl];
    internal static bool IsRequired(MachineModel model, FirmwareCategory role) => ConfigurationFunctions.Family(model) == MachineFamily.X68000 && Roles(model).Contains(role);

    internal static IReadOnlyList<FirmwareConfiguration> Apply(MachineConfiguration configuration, IReadOnlyDictionary<string, string?> values)
    {
        var result = configuration.Firmwares.ToList();
        foreach (var role in Roles(configuration.Model))
        {
            if (!values.TryGetValue(Field(role), out var path)) continue;
            result.RemoveAll(item => item.Category == role);
            if (!string.IsNullOrWhiteSpace(path)) result.Add(new(role, path, IsRequired(configuration.Model, role)));
        }
        return result;
    }

    internal static async ValueTask<EmulationFirmwareCandidate> ScanFileAsync(string path, MachineModel model, Emulator core, CancellationToken cancellationToken)
    {
        if (!ConfigurationFunctions.IsCompatible(model, core)) throw new ArgumentOutOfRangeException(nameof(core));
        var absolute = Path.GetFullPath(path);
        string? hash = null;
        string? destination = null;
        var compatibility = EmulationFirmwareCompatibility.Incompatible;
        try
        {
            var length = new FileInfo(absolute).Length;
            if (length is FirmwareConstants.X68000IplBytes or FirmwareConstants.X68000FontBytes or FirmwareConstants.X1IplBytes or FirmwareConstants.X1TurboIplBytes)
            {
                var bytes = await File.ReadAllBytesAsync(absolute, cancellationToken).ConfigureAwait(false);
                hash = Convert.ToHexString(SHA256.HashData(bytes));
                var md5 = Convert.ToHexString(MD5.HashData(bytes));
                if (ConfigurationFunctions.Family(model) == MachineFamily.X68000)
                {
                    if (length == FirmwareConstants.X68000IplBytes && new[] { FirmwareConstants.X68000IplMd5, FirmwareConstants.XviIplMd5, FirmwareConstants.CompactIplMd5, FirmwareConstants.X68030IplMd5 }.Contains(md5, StringComparer.OrdinalIgnoreCase)) destination = Field(FirmwareCategory.Ipl);
                    if (length == FirmwareConstants.X68000FontBytes && md5.Equals(FirmwareConstants.X68000FontMd5, StringComparison.OrdinalIgnoreCase)) destination = Field(FirmwareCategory.Font);
                }
                else
                {
                    var expectedBytes = model == MachineModel.X1 ? FirmwareConstants.X1IplBytes : FirmwareConstants.X1TurboIplBytes;
                    var expectedMd5 = model == MachineModel.X1 ? FirmwareConstants.X1IplMd5 : FirmwareConstants.X1TurboIplMd5;
                    if (length == expectedBytes && md5.Equals(expectedMd5, StringComparison.OrdinalIgnoreCase)) destination = Field(FirmwareCategory.Ipl);
                }
                if (destination is not null) compatibility = EmulationFirmwareCompatibility.Compatible;
            }
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
        return new(hash ?? absolute, absolute, Path.GetFileName(absolute), null, compatibility, destination);
    }

    internal static FirmwareConfiguration Select(EmulationFirmwareCandidate candidate, MachineModel model)
    {
        if (candidate.Compatibility != EmulationFirmwareCompatibility.Compatible || !IsField(candidate.DestinationFieldId ?? string.Empty)) throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareCannotBeSelected);
        var role = Roles(model).Single(role => Field(role) == candidate.DestinationFieldId);
        return new(role, candidate.Path, IsRequired(model, role), true);
    }
}
