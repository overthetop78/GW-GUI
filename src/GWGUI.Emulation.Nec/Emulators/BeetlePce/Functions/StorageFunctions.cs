using StorageConstants = GWGUI.Emulation.Nec.Emulators.BeetlePce.Constants.StorageConstants;

namespace GWGUI.Emulation.Nec.Emulators.BeetlePce.Functions;

internal static class StorageFunctions
{
    internal static IReadOnlyList<EmulationMediaDevice> Devices(Model model)
    {
        var devices = new List<EmulationMediaDevice>();
        if (model.SupportsCartridgeSlot)
            devices.Add(new(EmulationMediaSlot.Cartridge0, EmulationMediaType.Cartridge,
                StorageConstants.CartridgeExtensions, RequiresMachineRecreation: true,
                DisplayLabel: StorageConstants.CartridgeLabel, IsPermanent: model.HasBuiltInCartridgeSlot));
        if (model.SupportsCdDrive)
            devices.Add(new(EmulationMediaSlot.Cd0, EmulationMediaType.CompactDisc,
                StorageConstants.CompactDiscExtensions, RequiresMachineRecreation: true,
                DisplayLabel: StorageConstants.CompactDiscLabel, IsPermanent: true));
        return devices;
    }
}
