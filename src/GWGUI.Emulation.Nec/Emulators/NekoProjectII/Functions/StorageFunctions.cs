using StorageConstants = GWGUI.Emulation.Nec.Emulators.NekoProjectII.Constants.StorageConstants;

namespace GWGUI.Emulation.Nec.Emulators.NekoProjectII.Functions;

internal static class StorageFunctions
{
    internal static IReadOnlyList<EmulationMediaDevice> Devices(Model model)
    {
        var devices = Enumerable.Range(StorageConstants.FirstDriveIndex, model.MaximumFloppyDriveCount)
            .Select(index => new EmulationMediaDevice(new EmulationMediaSlot(
                EmulationMediaCategory.FloppyDrive, index), EmulationMediaType.Floppy,
                StorageConstants.FloppyExtensions, RequiresMachineRecreation: true,
                DisplayLabel: StorageConstants.FloppyLabelPrefix + index, IsPermanent: true)).ToList();
        if (model.SupportsHardDrives)
            devices.AddRange(Enumerable.Range(StorageConstants.FirstDriveIndex, StorageConstants.DriveCount)
                .Select(index => new EmulationMediaDevice(new EmulationMediaSlot(
                    EmulationMediaCategory.HardDisk, index), EmulationMediaType.HardDisk,
                    StorageConstants.HardDiskExtensions, IsRemovable: false,
                    RequiresMachineRecreation: true,
                    DisplayLabel: StorageConstants.HardDiskLabelPrefix + index, IsPermanent: true)));
        return devices;
    }
}
