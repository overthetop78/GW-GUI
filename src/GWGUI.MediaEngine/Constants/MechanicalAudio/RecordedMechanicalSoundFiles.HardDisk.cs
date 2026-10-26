using GWGUI.MediaEngine.Contracts;

namespace GWGUI.MediaEngine.Constants;

internal static partial class RecordedMechanicalSoundFiles
{
    /// <summary>Profils HardDisk définis dans leurs dossiers par marque et modèle.</summary>
    internal static partial class HardDisk
    {
        /// <summary>Regroupe les définitions des matériels de cette catégorie.</summary>
        internal static IReadOnlyList<RecordedMechanicalSoundProfile> Catalogs =>
        [
            IbmH3171,
            Miniscribe8425,
            QuantumBigfoot2110at,
            SeagateSt3120a,
            UnidentifiedHardDiskDrive,
        ];
    }
}
