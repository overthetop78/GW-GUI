using CommonHost = GWGUI.Emulation.Acorn.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.HardDisks;

namespace GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Functions;

/// <summary>Accès BBC aux disquettes et disques durs, exclusivement par le service média du SDK.</summary>
internal static class BbcMicroDiskFunctions
{
    /// <summary>Transfère au cœur la possession de l'image ; les formats restent gérés par MediaEngine.</summary>
    internal static IEmulationDiskImage Open(string path, bool writable, string? formatId = null, IEmulationDriveMechanics? mechanics = null) => CommonHost.MediaProcessing.OpenDiskAsync(path, writable, formatId, mechanics: mechanics).GetAwaiter().GetResult();
    internal static EmulationDiskSector Read(IEmulationDiskImage image, long sector) => image.ReadSectorAsync(sector).GetAwaiter().GetResult();
    internal static void Save(IEmulationDiskImage image) => image.FlushAsync().GetAwaiter().GetResult();
    /// <summary>Réutilise la publication du conteneur et la préparation de volume fournies par le SDK.</summary>
    internal static void CreateWinchester(string path, long bytes, bool fixedSize, HardDiskPreparation preparation, DiskImageGeometry? geometry)
    {
        if (geometry is null || geometry.Capacity != bytes || preparation is not (HardDiskPreparation.Blank or HardDiskPreparation.AcornAdfs)) throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(geometry));
        CommonHost.MediaProcessing.CreateHardDiskAsync(path, MediaConstants.WinchesterFormatId, geometry, preparation == HardDiskPreparation.AcornAdfs ? MediaConstants.AcornAdfsFileSystemId : null, preallocate: fixedSize).GetAwaiter().GetResult();
    }

}
