using ModelConstants = GWGUI.Emulation.Commodore.Common.Machines.AmigaCD32.Constants.ModelConstants;
using GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Contracts;

namespace GWGUI.Emulation.Commodore.Common.Machines.AmigaCD32.Dictionaries;

using CommonModelConstants = GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.HardwareConstants;
using FamilyModelConstants = GWGUI.Emulation.Commodore.Common.Machines.AmigaCD32.Constants.ModelConstants;

internal static class ModelCatalog
{
    internal static AmigaModel CD32 { get; } = new(FamilyModelConstants.CD32, FamilyModelConstants.DisplayName, FamilyModelConstants.CD32,
            [CpuModel.Motorola68020], ChipsetModel.AGA, RamCapacity._2MB, RamCapacity.None, RamCapacity.None, true,
            KickstartVersion._3_1, DriveCapacity.None, false, DriveCapacity.None, CommonModelConstants.StandardMouseButtons, true, ControllerPortCount: CommonModelConstants.StandardControllerPorts,
            HasBuiltInFloppyDrive: false, HasKeyboard: false);

    internal static IReadOnlyList<AmigaModel> All { get; } =
    [
        CD32
    ];
}
