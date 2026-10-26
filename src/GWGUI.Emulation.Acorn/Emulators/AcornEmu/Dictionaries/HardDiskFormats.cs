using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Functions;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.StorageConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Dictionaries;

/// <summary>Format Winchester du BBC : géométrie explicite, paramètres DSC et préparation ADFS par le SDK.</summary>
internal static class HardDiskFormats
{
    internal static IReadOnlyList<HardDiskImageFormat> All { get; } = Array.AsReadOnly<HardDiskImageFormat>([
        new("acorn-winchester", C.HardDiskExtensions[0], "SCSI", C.MaximumHardDiskBytes, 40L * 1024 * 1024, [HardDiskPreparation.AcornAdfs, HardDiskPreparation.Blank])
        {
            MinimumBytes = 33 * 256, LogicalSectorSizes = new HashSet<int> { 256 }, Geometry = new DiskChsGeometry(4, 33),
            SupportsExplicitGeometry = true, SupportsFixedAllocation = false,
            Operations = DiskFormatOperations.Create | DiskFormatOperations.Read | DiskFormatOperations.Modify,
            ImageCreator = BbcMicroDiskFunctions.CreateWinchester
        }]);
}
