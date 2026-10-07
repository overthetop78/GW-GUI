using ModelConstants = GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants;
using GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Contracts;

namespace GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Dictionaries;

using CommonModelConstants = GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.HardwareConstants;
using FamilyModelConstants = GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants;

internal static class ModelCatalog
{
    internal static AmigaModel A500 { get; } = new(FamilyModelConstants.A500, FamilyModelConstants.Amiga500, FamilyModelConstants.A500, [CpuModel.Motorola68000], ChipsetModel.OCS, RamCapacity._512KB, RamCapacity._512KB, RamCapacity.None, false, KickstartVersion._1_3, DriveCapacity.FourDrives, true, DriveCapacity.FourDrives, ControllerPortCount: CommonModelConstants.StandardControllerPorts);

    internal static AmigaModel A500PLUS { get; } = new(FamilyModelConstants.A500PLUS, FamilyModelConstants.Amiga500Plus, FamilyModelConstants.A500PLUS, [CpuModel.Motorola68000], ChipsetModel.ECS, RamCapacity._1MB, RamCapacity.None, RamCapacity.None, false, KickstartVersion._2_04, DriveCapacity.FourDrives, true, DriveCapacity.FourDrives, ControllerPortCount: CommonModelConstants.StandardControllerPorts);

    internal static AmigaModel A600 { get; } = new(FamilyModelConstants.A600, FamilyModelConstants.Amiga600, FamilyModelConstants.A600, [CpuModel.Motorola68000], ChipsetModel.ECS, RamCapacity._1MB, RamCapacity.None, RamCapacity.None, false, KickstartVersion._3_1, DriveCapacity.FourDrives, true, DriveCapacity.FourDrives, ControllerPortCount: CommonModelConstants.StandardControllerPorts);

    internal static AmigaModel A1000 { get; } = new(FamilyModelConstants.A1000, FamilyModelConstants.Amiga1000, FamilyModelConstants.A500OG, [CpuModel.Motorola68000], ChipsetModel.OCS, RamCapacity._512KB, RamCapacity.None, RamCapacity.None, false, KickstartVersion._1_2, DriveCapacity.FourDrives, true, DriveCapacity.DualDrives, ControllerPortCount: CommonModelConstants.StandardControllerPorts);

    internal static AmigaModel A1200 { get; } = new(FamilyModelConstants.A1200, FamilyModelConstants.Amiga1200, FamilyModelConstants.A1200, [CpuModel.Motorola68020], ChipsetModel.AGA, RamCapacity._2MB, RamCapacity.None, RamCapacity.None, false, KickstartVersion._3_1, DriveCapacity.FourDrives, true, DriveCapacity.FourDrives, ControllerPortCount: CommonModelConstants.StandardControllerPorts);

    internal static AmigaModel A2000 { get; } = new(FamilyModelConstants.A2000, FamilyModelConstants.Amiga2000, FamilyModelConstants.A2000, [CpuModel.Motorola68000], ChipsetModel.ECS, RamCapacity._1MB, RamCapacity.None, RamCapacity.None, false, KickstartVersion._3_1, DriveCapacity.FourDrives, true, DriveCapacity.EightDrives, ControllerPortCount: CommonModelConstants.StandardControllerPorts);

    internal static AmigaModel A3000 { get; } = new(FamilyModelConstants.A3000, FamilyModelConstants.Amiga3000, FamilyModelConstants.A2000, [CpuModel.Motorola68030], ChipsetModel.ECS, RamCapacity._2MB, RamCapacity.None, RamCapacity._8MB, false, KickstartVersion._3_1, DriveCapacity.FourDrives, true, DriveCapacity.EightDrives, ControllerPortCount: CommonModelConstants.StandardControllerPorts);

    internal static AmigaModel A4000 { get; } = new(FamilyModelConstants.A4000, FamilyModelConstants.Amiga4000, FamilyModelConstants.A4040, [CpuModel.Motorola68040, CpuModel.Motorola68030], ChipsetModel.AGA, RamCapacity._2MB, RamCapacity.None, RamCapacity._8MB, false, KickstartVersion._3_1, DriveCapacity.FourDrives, true, DriveCapacity.EightDrives, ControllerPortCount: CommonModelConstants.StandardControllerPorts);

    internal static IReadOnlyList<AmigaModel> All { get; } =
    [
        A500,
        A500PLUS,
        A600,
        A1000,
        A1200,
        A2000,
        A3000,
        A4000
    ];
}
