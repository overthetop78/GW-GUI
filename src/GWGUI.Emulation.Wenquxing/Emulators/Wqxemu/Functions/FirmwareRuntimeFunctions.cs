using CommonCoreHostFunctions = GWGUI.Emulation.Wenquxing.Common.Functions.CoreHostFunctions;
using System.IO;
using GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Dictionaries;
using NativeFirmwareConstants = GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Constants.FirmwareConstants;

namespace GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Functions;
internal static class FirmwareRuntimeFunctions
{
    internal static bool IsCompatible(MachineConfiguration configuration, FirmwareCategory role, string path)
    {
        if (!FirmwareCatalog.Requirements(configuration.Model).Any(requirement => requirement.Role == role))
            return false;
        try
        {
            var info = new FileInfo(path);
            if (!info.Exists || info.Length == 0)
                return false;
            var extension = info.Extension.ToLowerInvariant();
            var accepted = role switch
            {
                FirmwareCategory.SystemRom => extension is ".rom" or ".bin",
                FirmwareCategory.NorFlash => extension is ".nor" or ".fls",
                FirmwareCategory.NandFlash => extension == ".nand",
                FirmwareCategory.NandFirstPlane => extension == ".nand0",
                _ => false
            };
            if (!accepted)
                return false;
            return role switch
            {
                FirmwareCategory.SystemRom => configuration.Model switch
                {
                    MachineModel.NC1020 => info.Length >= NativeFirmwareConstants.Nc1020RomMinimumBytes,
                    MachineModel.PC1000 => info.Length is NativeFirmwareConstants.Pc1000Rom12MBytes or NativeFirmwareConstants.Pc1000Rom16MBytes,
                    MachineModel.CC800 => info.Length >= NativeFirmwareConstants.Cc800RomMinimumBytes,
                    _ => false
                },
                FirmwareCategory.NorFlash => info.Length >= (configuration.Model is MachineModel.NC1020 or MachineModel.NC3000 ? NativeFirmwareConstants.Nor1MBytes : NativeFirmwareConstants.Nor512KBytes),
                _ => true
            };
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        var requirements = FirmwareCatalog.Requirements(configuration.Model);
        if (configuration.Firmware.Images.Keys.Any(role => !requirements.Any(requirement => requirement.Role == role)))
            throw Invalid(EmulationMessageCode.FirmwareIncompatible);
        var directory = Path.Combine(Path.GetFullPath(systemDirectory), NativeFirmwareConstants.SystemSubdirectory, ModelDictionary.NativeValues[configuration.Model]);
        Directory.CreateDirectory(directory);
        foreach (var requirement in requirements)
        {
            if (!configuration.Firmware.Images.TryGetValue(requirement.Role, out var source))
            {
                if (requirement.IsRequired)
                    throw Invalid(EmulationMessageCode.FirmwareMissing);
                continue;
            }

            if (!File.Exists(source))
                throw Invalid(EmulationMessageCode.FirmwareMissing);
            var destination = Path.Combine(directory, requirement.FileNames[0]);
            File.Copy(source, destination, overwrite: true);
            File.SetAttributes(destination, File.GetAttributes(destination) & ~FileAttributes.ReadOnly);
        }
    }

    private static EmulationMessageException Invalid(EmulationMessageCode code) => CommonCoreHostFunctions.Messages.CreateError(EmulationMessageCategory.Firmware, code, null);
}