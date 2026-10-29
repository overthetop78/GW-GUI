using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Functions;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.StorageConstants;
using W = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.WinchesterModelConstants;
using S = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.AcornScsiConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Dictionaries;

/// <summary>Format Winchester du BBC : géométrie explicite, paramètres DSC et préparation ADFS par le SDK.</summary>
internal static class HardDiskFormats
{
    internal static IReadOnlyList<HardDiskImageFormat> All { get; } = Array.AsReadOnly<HardDiskImageFormat>(
    [
        Create(W.Model110, "Acorn Winchester 110", new(W.Cylinders110, W.Heads110, S.SectorsPerTrack, S.SectorSize)),
        Create(W.Model130, "Acorn Winchester 130", new(W.Cylinders130, W.Heads130, S.SectorsPerTrack, S.SectorSize)),
        Create(W.Custom, "Acorn SCSI", new(W.Cylinders110, W.Heads110, S.SectorsPerTrack, S.SectorSize), custom: true)
    ]);

    private static HardDiskImageFormat Create(string id, string name, DiskImageGeometry geometry, bool custom = false) =>
        new(id, C.HardDiskExtensions[0], C.ScsiDisplay, custom ? W.MaximumBytes : geometry.Capacity, geometry.Capacity,
            [HardDiskPreparation.AcornAdfs, HardDiskPreparation.Blank])
        {
            MinimumBytes = custom ? geometry.SectorsPerTrack * geometry.SectorSize : geometry.Capacity,
            AllowedCapacitiesBytes = custom ? null : [geometry.Capacity],
            LogicalSectorSizes = new HashSet<int> { geometry.SectorSize },
            Geometry = new DiskChsGeometry(geometry.Heads, geometry.SectorsPerTrack),
            FixedGeometry = custom ? null : geometry,
            SupportsExplicitGeometry = true,
            SupportsFixedAllocation = true,
            InvariantDisplayName = name,
            Operations = DiskFormatOperations.Create | DiskFormatOperations.Read | DiskFormatOperations.Modify,
            ImageCreator = BbcMicroDiskFunctions.CreateWinchester
        };
}
