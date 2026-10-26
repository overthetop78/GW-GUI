using GWGUI.MediaEngine.Contracts;

namespace GWGUI.MediaEngine.Constants;

internal static partial class RecordedMechanicalSoundFiles
{
    /// <summary>Profils Optical définis dans leurs dossiers par marque et modèle.</summary>
    internal static partial class Optical
    {
        /// <summary>Regroupe les définitions des matériels de cette catégorie.</summary>
        internal static IReadOnlyList<RecordedMechanicalSoundProfile> Catalogs =>
        [
            UnidentifiedCdRomTrayDrive,
        ];
    }
}
