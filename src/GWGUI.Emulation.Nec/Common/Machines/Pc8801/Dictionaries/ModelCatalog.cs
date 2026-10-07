using GWGUI.Emulation.Nec.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Nec.Common.Machines.Common.Enums;
using MachineConstants = GWGUI.Emulation.Nec.Common.Machines.Pc8801.Constants.MachineConstants;

namespace GWGUI.Emulation.Nec.Common.Machines.Pc8801.Dictionaries;

internal static class ModelCatalog
{
    internal static Model MkII { get; } = MachineConstants.Definition with
    { Id = MachineConstants.MkIIId, DisplayName = MachineConstants.MkIIName,
      BackendModel = MachineConstants.MkIIId, BuiltInFloppyDriveCount = (int)DriveCount.Two };
    internal static Model MkIISr { get; } = MachineConstants.Definition with
    { Id = MachineConstants.MkIISrId, DisplayName = MachineConstants.MkIISrName,
      BackendModel = MachineConstants.MkIISrId, BuiltInFloppyDriveCount = (int)DriveCount.Two };
}
