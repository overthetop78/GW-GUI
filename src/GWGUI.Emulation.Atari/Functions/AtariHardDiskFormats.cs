using GWGUI.Emulation.HardDisks;

namespace GWGUI.Emulation.Atari.Functions;

internal static class AtariHardDiskFormats
{
    internal static IReadOnlyList<HardDiskImageFormat> For(AtariMachineModel model)
    {
        if (AtariConfigurationFunctions.GetFamily(model) != AtariMachineFamily.St) return [];
        // Conservative whole-image ceilings: a disk also fits a single TOS
        // partition, including the oldest TOS offered for each model. ACSI's
        // 21-bit sector addressing additionally caps it at 1 GiB.
        var maximumMiB = model switch
        {
            AtariMachineModel.St or AtariMachineModel.Stf or AtariMachineModel.Stfm or AtariMachineModel.MegaSt => 256L,
            AtariMachineModel.Falcon => 1024L,
            _ => 512L
        };
        var result = new List<HardDiskImageFormat>();
        var storage = AtariStModelCatalog.Get(model).Storage;
        if (storage.Contains(AtariStStorageCapability.Acsi))
            result.Add(new("atari-acsi", ".vhd", "ACSI", maximumMiB * 1024 * 1024, 40L * 1024 * 1024,
                [HardDiskPreparation.Blank, HardDiskPreparation.AtariAhdiFat16]));
        if (storage.Contains(AtariStStorageCapability.Ide))
            result.Add(new("atari-ide", ".ide", "IDE", maximumMiB * 1024 * 1024, 40L * 1024 * 1024,
                [HardDiskPreparation.Blank, HardDiskPreparation.AtariAhdiFat16]));
        return result;
    }
}
