using ModelConstants = GWGUI.Emulation.Commodore.Common.Machines.CommodoreCDTV.Constants.ModelConstants;
using GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Contracts;

namespace GWGUI.Emulation.Commodore.Common.Machines.CommodoreCDTV.Dictionaries;

using CommonModelConstants = GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.HardwareConstants;
using FamilyModelConstants = GWGUI.Emulation.Commodore.Common.Machines.CommodoreCDTV.Constants.ModelConstants;

internal static class ModelCatalog
{
    internal static AmigaModel CDTV { get; } = new(FamilyModelConstants.CDTV, FamilyModelConstants.DisplayName, FamilyModelConstants.CDTV,
            [CpuModel.Motorola68000], ChipsetModel.OCS, RamCapacity._1MB, RamCapacity.None, RamCapacity.None, true,
            KickstartVersion._1_3, DriveCapacity.SingleDrive, true, DriveCapacity.DualDrives, ControllerPortCount: CommonModelConstants.StandardControllerPorts,
            HasBuiltInFloppyDrive: false);

    internal static IReadOnlyList<AmigaModel> All { get; } =
    [
        CDTV
    ];
}
