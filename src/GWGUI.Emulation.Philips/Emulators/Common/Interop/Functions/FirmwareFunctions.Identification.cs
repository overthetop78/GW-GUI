using GWGUI.Emulation.Philips.Emulators.Common.Interop.Constants;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

namespace GWGUI.Emulation.Philips.Emulators.Common.Interop.Functions;

internal static class FirmwareIdentificationFunctions
{
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
                    destination = FirmwareFunctions.Field(FirmwareCategory.VoiceSamples);
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
                destination = FirmwareFunctions.Field(FirmwareCategory.SystemBios);
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

}
