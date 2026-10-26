using GWGUI.MediaEngine.Contracts;

namespace GWGUI.MediaEngine.Constants;

internal static partial class RecordedMechanicalSoundFiles
{
    /// <summary>Profils des lecteurs de bande définis par matériel.</summary>
    internal static partial class Tape
    {
        /// <summary>Regroupe les magnétophones enregistrés.</summary>
        internal static IReadOnlyList<RecordedMechanicalSoundProfile> Catalogs => [UnidentifiedCassettePlayer];
    }
}
