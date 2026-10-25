namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Constants;

/// <summary>Horloges et représentation PCM de l'adaptation sonore BBC Micro.</summary>
internal static class AudioConstants
{
    /// <summary>Horloge du PSG fournie par l'ULA vidéo, en hertz.</summary>
    internal const int SoundClock = 4_000_000;
    /// <summary>Cadence du bus CPU auquel le composant audio est raccordé, en hertz.</summary>
    internal const int CpuClock = (int)BbcMicroModelConstants.CpuFrequencyHz;
    /// <summary>Échantillons mono produits par seconde ; indépendant de la hauteur des notes du PSG.</summary>
    internal const int SampleRate = 44_100;
    /// <summary>Constante de temps du couplage continu : résistance 10 kΩ et condensateur 4,7 µF.</summary>
    internal const double CouplingTimeSeconds = 10_000 * 4.7e-6;
}
