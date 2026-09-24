using MediaSectorWritePlan = global::GWGUI.MediaFileSystems.Contracts.MediaSectorWritePlan;
using GWGUI.MediaFileSystems.Migration;

namespace GWGUI.MediaFileSystems.FileSystems.Commodore.Dos;

/// <summary>Crée un volume Commodore DOS D64, D71 ou D81 depuis un plan de migration validé.</summary>
public sealed class CommodoreDosVolumeWriter
{
    /// <summary>Reconstruit BAM, répertoire, chaînes de fichiers et secteurs latéraux REL.</summary>
    public MediaSectorWritePlan Create(MigrationPlan plan, string formatId, CommodoreDosWritePolicy? policy = null)
    {
        ArgumentNullException.ThrowIfNull(plan);
        return new CommodoreDosVolumeBuilder(plan, CommodoreDosWritableGeometry.Resolve(formatId), policy ?? new()).Build();
    }
}
