using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Functions;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.StorageConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Dictionaries;

/// <summary>Format Winchester du BBC : géométrie explicite, paramètres DSC et préparation ADFS par le SDK.</summary>
internal static class HardDiskFormats
{
    internal static IReadOnlyList<HardDiskImageFormat> All { get; } = GWGUI.Emulation.Acorn.Common.Functions.CoreHostFunctions.MediaProcessing
        .HardDiskDriveModels(EmulationHardDiskModelConstants.AcornWinchester).Select(model => new HardDiskImageFormat(
            model.Id, C.HardDiskExtensions[0], C.ScsiDisplay, model.MaximumBytes, model.Geometry.Capacity,
            [HardDiskPreparation.AcornAdfs, HardDiskPreparation.Blank])
        {
            MinimumBytes = model.AllowsCustomGeometry ? model.Geometry.SectorsPerTrack * model.Geometry.SectorSize : model.Geometry.Capacity,
            AllowedCapacitiesBytes = model.AllowsCustomGeometry ? null : [model.Geometry.Capacity],
            LogicalSectorSizes = new HashSet<int> { model.Geometry.SectorSize },
            Geometry = new DiskChsGeometry(model.Geometry.Heads, model.Geometry.SectorsPerTrack),
            FixedGeometry = model.AllowsCustomGeometry ? null : model.Geometry,
            SupportsExplicitGeometry = true,
            SupportsFixedAllocation = true,
            InvariantDisplayName = model.DisplayName,
            Operations = DiskFormatOperations.Create | DiskFormatOperations.Read | DiskFormatOperations.Modify,
            ImageCreator = BbcMicroDiskFunctions.CreateWinchester
        }).ToArray();
}
