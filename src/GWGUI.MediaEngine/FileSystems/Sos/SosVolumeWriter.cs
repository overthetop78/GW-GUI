using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Conversion.Migration;

using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.FileSystems.Sos;

/// <summary>Crée un volume Apple III SOS en conservant son profil distinct du conteneur ProDOS.</summary>
public sealed class SosVolumeWriter
{
    /// <summary>Crée la structure de volume commune ProDOS/SOS puis ajoute le marqueur d'amorçage SOS.</summary>
    public SectorImage Create(MigrationPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var baseImage = new ProDosMigrationImageBuilder().Create(plan, DiskImageFormatIds.AppleIIProDos140);
        var blocks = baseImage.AvailableBlocks.Select(block => block with { Data = block.Data.ToArray() }).OrderBy(block => block.LogicalBlock).ToArray();
        var boot = blocks.Single(block => block.LogicalBlock == 0).Data.ToArray();
        SosBootFormat.Marker.CopyTo(boot.AsSpan(SosBootFormat.MarkerOffset));
        blocks[0] = blocks[0] with { Data = boot };
        return new(DiskImageFormatIds.AppleIIISos, baseImage.BlockSize, baseImage.Cylinders, baseImage.Heads, baseImage.SectorsPerTrack, blocks);
    }
}
