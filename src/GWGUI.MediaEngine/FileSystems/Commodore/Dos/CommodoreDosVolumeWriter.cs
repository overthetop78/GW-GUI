using GWGUI.MediaEngine.Migration;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Commodore.Dos;

/// <summary>Crée un volume Commodore DOS D64, D71 ou D81 depuis un plan de migration validé.</summary>
public sealed class CommodoreDosVolumeWriter
{
    /// <summary>Reconstruit BAM, répertoire, chaînes de fichiers et secteurs latéraux REL.</summary>
    public SectorImage Create(MigrationPlan plan, string formatId, CommodoreDosWritePolicy? policy = null)
    {
        ArgumentNullException.ThrowIfNull(plan);
        return new CommodoreDosVolumeBuilder(plan, CommodoreDosWritableGeometry.Resolve(formatId), policy ?? new()).Build();
    }
}
