using S = GWGUI.Emulation.Constants.EmulationMechanicalSoundConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;

/// <summary>Présentation des réglages déclarés par les lecteurs Acorn, indépendante de leur rendu App.</summary>
internal static class StoragePresentationConstants
{
    /// <summary>Groupe traduit des sons du lecteur.</summary>
    internal const string SoundGroup = "Emulation.Storage.Sound.Group";
    /// <summary>Groupe traduit du fonctionnement du lecteur.</summary>
    internal const string BehaviorGroup = "Emulation.Input.Behavior";
    /// <summary>Enregistrement BBC Micro ; les opérations manquantes utilisent les replis du service média.</summary>
    internal const string FloppySound = S.BbcMicroFloppy;
    /// <summary>Repli Winchester Miniscribe disponible ; aucun enregistrement Rodime attesté dans la banque.</summary>
    internal const string WinchesterSound = S.Winchester;
    /// <summary>Présélections de latence en millisecondes, incluant les anciennes valeurs toujours relisibles.</summary>
    internal static IReadOnlyList<int> LatencyPresets { get; } = Array.AsReadOnly<int>([0, 1, 2, 5, 10, 20, 50, 100, 250, 500, 1000, 2000, 5000, 10000]);
}
