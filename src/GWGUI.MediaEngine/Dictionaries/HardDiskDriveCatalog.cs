using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Images.Formats.HardDisk.Acorn;
using C = GWGUI.MediaEngine.Constants.AcornWinchesterModelConstants;

namespace GWGUI.MediaEngine.Dictionaries;

/// <summary>Géométries des mécanismes HDD prises en charge par les profils matériels déclarés.</summary>
internal static class HardDiskDriveCatalog
{
    private static readonly IReadOnlyList<MediaHardDiskDriveModel> Acorn = Array.AsReadOnly<MediaHardDiskDriveModel>(
    [
        new(C.Model110, "Acorn Winchester 110", new(C.Cylinders110, C.Heads110, AcornWinchesterFormat.SectorsPerTrack, AcornWinchesterFormat.SectorSize)),
        new(C.Model130, "Acorn Winchester 130", new(C.Cylinders130, C.Heads130, AcornWinchesterFormat.SectorsPerTrack, AcornWinchesterFormat.SectorSize)),
        new(C.Custom, "Acorn SCSI", new(C.Cylinders110, C.Heads110, AcornWinchesterFormat.SectorsPerTrack, AcornWinchesterFormat.SectorSize)) { AllowsCustomGeometry = true, MaximumBytes = C.MaximumAddressableBytes }
    ]);

    /// <summary>Décrit les profils de la famille sans inventer une géométrie à partir d'une taille arbitraire.</summary>
    internal static IReadOnlyList<MediaHardDiskDriveModel> Models(string family) => family == C.Family ? Acorn : throw new ArgumentOutOfRangeException(nameof(family));
}
