using System.IO;
using System.Security.Cryptography;
using MiniVmacConstants = GWGUI.Emulation.Apple.Emulators.MiniVmac.Constants.EmulatorConstants;

namespace GWGUI.Emulation.Apple.Emulators.MiniVmac.Functions;

internal static class FirmwareFunctions
{
    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        Directory.CreateDirectory(systemDirectory);
        var firmware = configuration.Firmwares.SingleOrDefault(item => item.Category == FirmwareCategory.SystemRom);
        if (firmware is null || !File.Exists(firmware.Path)) throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareMissing, ErrorMessages.RequiredFirmwareMissing);
        var source = Path.GetFullPath(firmware.Path);
        if (new FileInfo(source).Length < MiniVmacConstants.MinimumRomBytes) throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareFileUnreadable);
        var destination = Path.Combine(systemDirectory, MiniVmacConstants.RomFileName);
        if (!string.Equals(source, Path.GetFullPath(destination), StringComparison.OrdinalIgnoreCase)) File.Copy(source, destination, overwrite: true);
    }

    internal static async ValueTask<EmulationFirmwareCandidate> ScanFileAsync(string path, CancellationToken cancellationToken)
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
                compatibility = string.Equals(hash, MiniVmacConstants.RomMd5, StringComparison.OrdinalIgnoreCase) ? EmulationFirmwareCompatibility.Compatible : EmulationFirmwareCompatibility.PartiallyCompatible;
            }
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
        return new(hash ?? absolute, absolute, Path.GetFileName(absolute), null, compatibility, compatibility == EmulationFirmwareCompatibility.Incompatible ? null : SettingsConstants.Firmware);
    }
}
