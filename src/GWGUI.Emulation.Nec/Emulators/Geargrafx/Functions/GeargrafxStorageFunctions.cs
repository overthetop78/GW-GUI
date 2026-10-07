using GWGUI.Emulation.Nec.Common.Machines.LaserActive.Constants;
using GWGUI.Emulation.Nec.Common.Machines.PcEngineDuo.Constants;
using GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Constants;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Nec.Emulators.Geargrafx.Constants;

namespace GWGUI.Emulation.Nec.Emulators.Geargrafx.Functions;

internal static class GeargrafxStorageFunctions
{
    private const string CdDriveEnabledOption = "geargrafx.cdDriveEnabled";

    internal static IReadOnlyList<EmulationMediaDevice> Devices(Model model)
    {
        var devices = new List<EmulationMediaDevice>();
        if (model.SupportsCartridgeSlot)
            devices.Add(new EmulationMediaDevice(EmulationMediaSlot.Cartridge0,
                EmulationMediaType.Cartridge,
                [BeetlePceFastStorageConstants.PceExtension,
                    BeetlePceFastStorageConstants.BinExtension],
                RequiresMachineRecreation: true,
                DisplayLabel: BeetlePceFastStorageConstants.CartridgeSlotLabel,
                IsPermanent: model.HasBuiltInCartridgeSlot,
                ModelResourceKey: BeetlePceFastStorageConstants.HuCardSlotResourceKey));
        if (model.SupportsCdDrive)
        {
            var laserActive = model.Id == LaserActiveMachineConstants.Id;
            var extensions = laserActive
                ? new[] { GeargrafxConstants.MmiExtension }
                : new[] { StorageSettingsFunctionsConstants.Cue,
                    StorageSettingsFunctionsConstants.Chd };
            devices.Add(new EmulationMediaDevice(EmulationMediaSlot.Cd0,
                EmulationMediaType.CompactDisc, extensions,
                RequiresMachineRecreation: true,
                DisplayLabel: StorageSettingsFunctionsConstants.CdDriveLabel,
                IsPermanent: model.HasBuiltInCdDrive,
                ModelResourceKey: model.Id == PcEngineDuoMachineConstants.Id
                    ? BeetlePceFastStorageConstants.DuoCdRomResourceKey
                    : BeetlePceFastStorageConstants.CdRom2ResourceKey));
        }
        return devices;
    }

    internal static bool IsCompactDiscEnabled(Model model,
        IReadOnlyDictionary<string, string> options) =>
        model.HasBuiltInCdDrive || options.TryGetValue(CdDriveEnabledOption, out var value)
            && bool.TryParse(value, out var enabled) && enabled;

    internal static void ApplyOptions(Dictionary<string, string> options,
        IReadOnlyCollection<EmulationMediaSlot> configured) =>
        options[CdDriveEnabledOption] = configured.Contains(EmulationMediaSlot.Cd0).ToString();
}
