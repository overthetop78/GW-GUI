using GWGUI.Emulation.HardDisks;

namespace GWGUI.Emulation.Atari.Common.Machines.Common.Functions;

internal static class HardDiskFormats
{
    private static readonly DiskFormatRegistry Formats = DiskFormatRegistry.CreateDefault();

    internal static IReadOnlyList<HardDiskImageFormat> For(MachineModel model)
    {
        if (ConfigurationFunctions.GetFamily(model) != MachineFamily.St) return [];
        // Conservative whole-image ceilings: a disk also fits a single TOS
        // partition, including the oldest TOS offered for each model. ACSI's
        // 21-bit sector addressing additionally caps it at 1 GiB.
        var maximumMiB = model switch
        {
            MachineModel.St or MachineModel.Stf or MachineModel.Stfm or MachineModel.MegaSt => 256L,
            MachineModel.Falcon => 1024L,
            _ => 512L
        };
        var result = new List<HardDiskImageFormat>();
        var storage = StModelCatalog.Get(model).Storage;
        if (storage.Contains(StStorageCapability.Acsi))
            result.Add(Formats.Describe(new("atari-acsi", ".vhd", "ACSI", maximumMiB * 1024 * 1024, 40L * 1024 * 1024,
                [HardDiskPreparation.Blank, HardDiskPreparation.AtariAhdiFat16])));
        if (storage.Contains(StStorageCapability.Ide))
            result.Add(Formats.Describe(new("atari-ide", ".ide", "IDE", maximumMiB * 1024 * 1024, 40L * 1024 * 1024,
                [HardDiskPreparation.Blank, HardDiskPreparation.AtariAhdiFat16])));
        return result;
    }
}
