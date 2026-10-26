using GWGUI.MediaEngine.Contracts;

namespace GWGUI.MediaEngine.Constants;

internal static partial class RecordedMechanicalSoundFiles
{
    /// <summary>Profils Floppy définis dans leurs dossiers par marque et modèle.</summary>
    internal static partial class Floppy
    {
        /// <summary>Regroupe les définitions des matériels de cette catégorie.</summary>
        internal static IReadOnlyList<RecordedMechanicalSoundProfile> Catalogs =>
        [
            AlpsElectricDf354h148f,
            EpsonSd700,
            EpsonSd800,
            MitsumiD359m3d,
            NecFd1231h200010,
            NecFd1231h200504,
            NecFd1231h200710,
            PanasonicJu256,
            PanasonicJu4755,
            SamsungSfd321bE,
            TeacFd235hfA291,
            TeacFd235hfC291,
            Unidentified35InchDriveWithInsertionAndEjection,
            Unidentified35InchDriveWithTimedSeeks,
            Unidentified525InchDriveWithInsertionAndEjection,
            Unidentified525InchDriveWithTimedSeeks,
            UnidentifiedAmigaA500Drive,
            UnidentifiedBbcMicro525InchDrive,
        ];
    }
}
