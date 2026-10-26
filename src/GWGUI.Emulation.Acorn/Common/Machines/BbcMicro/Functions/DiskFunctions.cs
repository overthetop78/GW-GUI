using CommonHost = GWGUI.Emulation.Acorn.Common.Functions.CoreHostFunctions;
using System.IO;
using GWGUI.Emulation.HardDisks;

namespace GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Functions;

/// <summary>Accès BBC aux disquettes et disques durs, exclusivement par le service média du SDK.</summary>
internal static class BbcMicroDiskFunctions
{
    /// <summary>Transfère au cœur la possession de l'image ; les formats restent gérés par MediaEngine.</summary>
    internal static IEmulationDiskImage Open(string path, bool writable, string? formatId = null) => CommonHost.MediaProcessing.OpenDiskAsync(path, writable, formatId).GetAwaiter().GetResult();
    internal static EmulationDiskSector Read(IEmulationDiskImage image, long sector) => image.ReadSectorAsync(sector).GetAwaiter().GetResult();
    internal static void Save(IEmulationDiskImage image) => image.FlushAsync().GetAwaiter().GetResult();
    /// <summary>Réutilise la publication du conteneur et la préparation de volume fournies par le SDK.</summary>
    internal static void CreateWinchester(string path, long bytes, bool fixedSize, HardDiskPreparation preparation, DiskImageGeometry? geometry)
    {
        if (geometry is null || geometry.Capacity != bytes || preparation is not (HardDiskPreparation.Blank or HardDiskPreparation.AcornAdfs)) throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(geometry));
        CommonHost.MediaProcessing.CreateHardDiskAsync(path, "hdd.acorn-winchester", geometry, preparation == HardDiskPreparation.AcornAdfs ? "acorn-adfs" : null).GetAwaiter().GetResult();
    }

    /// <summary>Conserve les deux fichiers d'une image redirigée ; retire seulement les copies créées si l'opération échoue.</summary>
    internal static string RedirectHardDisk(string source, string root, int unit)
    {
        var identity = CommonHost.StateFiles.HashDocument(Path.GetFullPath(source).ToUpperInvariant());
        var directory = Path.Combine(root, "HardDisks"); Directory.CreateDirectory(directory);
        var target = Path.Combine(directory, unit + "-" + identity[..16] + Path.GetExtension(source));
        var created = new List<string>();
        void Copy(string input, string output)
        {
            if (File.Exists(output)) return;
            using var sourceStream = File.OpenRead(input);
            using var targetStream = new FileStream(output, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            created.Add(output); sourceStream.CopyTo(targetStream);
        }
        try
        {
            Copy(source, target);
            var sourceDescriptor = Path.ChangeExtension(source, ".dsc"); var targetDescriptor = Path.ChangeExtension(target, ".dsc");
            if (File.Exists(sourceDescriptor)) Copy(sourceDescriptor, targetDescriptor);
            return target;
        }
        catch { foreach (var path in created) File.Delete(path); throw; }
    }
    /// <summary>Conserve les écritures redirigées dans une copie privée stable, sans réécrire l'image d'origine.</summary>
    internal static string RedirectFloppy(string source, string root, int drive)
    {
        var identity = CommonHost.StateFiles.HashDocument(Path.GetFullPath(source).ToUpperInvariant());
        var directory = Path.Combine(root, "Floppies"); Directory.CreateDirectory(directory);
        var target = Path.Combine(directory, drive + "-" + identity[..16] + Path.GetExtension(source));
        if (!File.Exists(target)) File.Copy(source, target);
        return target;
    }
}
