using System.IO;
using System.Security.Cryptography;
using MiniVmacConstants = GWGUI.Emulation.Apple.Emulators.MiniVmac.Constants.EmulatorConstants;

namespace GWGUI.Emulation.Apple.Common.Machines.Common.Functions;

internal static partial class FirmwareFunctions
{
    internal static async ValueTask<EmulationFirmwareCandidate> ScanFileAsync(string path,
        CancellationToken cancellationToken)
    {
        var absolute = Path.GetFullPath(path);
        var compatibility = EmulationFirmwareCompatibility.Incompatible;
        string? hash = null;
        try
        {
            using var stream = File.OpenRead(absolute);
            if (stream.Length >= MiniVmacConstants.MinimumRomBytes)
            {
                hash = Convert.ToHexString(await MD5.HashDataAsync(stream, cancellationToken).ConfigureAwait(false));
                compatibility = string.Equals(hash, MiniVmacConstants.RomMd5, StringComparison.OrdinalIgnoreCase)
                    ? EmulationFirmwareCompatibility.Compatible : EmulationFirmwareCompatibility.PartiallyCompatible;
            }
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
        return new(hash ?? absolute, absolute, Path.GetFileName(absolute), null, compatibility,
            compatibility == EmulationFirmwareCompatibility.Incompatible ? null : SettingsConstants.Firmware);
    }

    internal static FirmwareConfiguration Select(EmulationFirmwareCandidate candidate)
    {
        if (candidate.Compatibility == EmulationFirmwareCompatibility.Incompatible
            || candidate.DestinationFieldId != SettingsConstants.Firmware)
            throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid,
                ErrorMessages.FirmwareCannotBeSelected);
        return new(FirmwareCategory.SystemRom, candidate.Path, true);
    }
}
