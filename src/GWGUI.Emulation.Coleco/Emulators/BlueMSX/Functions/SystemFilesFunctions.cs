using System.IO;
using GWGUI.Emulation.Coleco.Emulators.BlueMSX.Constants;

namespace GWGUI.Emulation.Coleco.Emulators.BlueMSX.Functions;

internal static class SystemFilesFunctions
{
    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        if (configuration.Model != MachineModel.ColecoVision)
            throw new ArgumentOutOfRangeException(nameof(configuration));
        var machineDirectory = Path.Combine(systemDirectory, SystemFilesConstants.MachinesDirectory,
            SystemFilesConstants.MachineDirectory);
        Directory.CreateDirectory(machineDirectory);
        File.WriteAllText(Path.Combine(machineDirectory, SystemFilesConstants.ConfigurationFileName),
            SystemFilesConstants.Configuration);
        var firmware = configuration.Firmwares.SingleOrDefault(entry => entry.Category == FirmwareCategory.Os7);
        if (firmware is not null)
        {
            var destination = Path.Combine(machineDirectory, SystemFilesConstants.FirmwareFileName);
            if (!string.Equals(Path.GetFullPath(firmware.Path), Path.GetFullPath(destination), StringComparison.OrdinalIgnoreCase))
                File.Copy(firmware.Path, destination, true);
        }
    }
}
