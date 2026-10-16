using System.IO;
using NativeFirmwareConstants = GWGUI.Emulation.Panasonic.Emulators.Opera.Constants.FirmwareConstants;

namespace GWGUI.Emulation.Panasonic.Emulators.Opera.Functions;

internal static class FirmwareRuntimeFunctions
{
    internal static bool IsCompatible(MachineConfiguration configuration, FirmwareRole role, string path)
    {
        if (configuration.Model != MachineModel.ThreeDO || !Enum.IsDefined(role)) return false;
        try
        {
            var file = new FileInfo(path);
            return file.Exists && file.Extension.ToLowerInvariant() is ".bin" or ".rom"
                && (file.Length == NativeFirmwareConstants.RomSizeBytes || role == FirmwareRole.FontRom && file.Length == NativeFirmwareConstants.Fz1KanjiRomSizeBytes);
        }
        catch (IOException) { return false; }
        catch (UnauthorizedAccessException) { return false; }
    }
    internal static string FileName(MachineConfiguration configuration, FirmwareRole role)
    {
        var files = role == FirmwareRole.SystemRom ? NativeFirmwareConstants.SystemFiles : NativeFirmwareConstants.FontFiles;
        var fallback = role == FirmwareRole.SystemRom ? NativeFirmwareConstants.DefaultSystemFile : NativeFirmwareConstants.DefaultFontFile;
        if (!configuration.Firmware.Images.TryGetValue(role, out var path)) return fallback;
        return files.FirstOrDefault(file => string.Equals(file, Path.GetFileName(path), StringComparison.OrdinalIgnoreCase)) ?? fallback;
    }
    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        Directory.CreateDirectory(systemDirectory);
        foreach (var image in configuration.Firmware.Images)
        {
            if (!Enum.IsDefined(image.Key)) throw new ArgumentException(nameof(configuration));
            if (!File.Exists(image.Value)) throw Missing();
            var destination = Path.Combine(systemDirectory, FileName(configuration, image.Key));
            File.Copy(image.Value, destination, overwrite: true);
            File.SetAttributes(destination, File.GetAttributes(destination) & ~FileAttributes.ReadOnly);
        }
    }
    private static EmulationMessageException Missing() => new(new EmulationMessage(EmulationMessageCategory.Firmware, EmulationMessageCode.FirmwareMissing, EmulationMessageSeverity.Error, EmulationMessageTarget.Dialog));
}
