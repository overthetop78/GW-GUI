namespace GWGUI.Emulation.Example.Emulators.Common.Interop.Constants;

/// <summary>Paramètres du matériel simulé et de sa charge utile d’état, indépendants des enveloppes SDK.</summary>
internal static class CoreConstants
{
    /// <summary>Cadence PAL de démonstration, en images par seconde.</summary>
    internal const int FramesPerSecond = 50;
    /// <summary>Identité du format binaire propre à la RAM et au compteur du simulateur.</summary>
    internal const int StateSignature = 0x45584D50;
    /// <summary>Version unique du format ; une autre version est rejetée avant toute modification de RAM.</summary>
    internal const int StateVersion = 1;
    /// <summary>Nombre d’octets fixes précédant la RAM : signature, version, modèle, compteur et longueur.</summary>
    internal const int StateHeaderLength = sizeof(int) * 4 + sizeof(long);
    /// <summary>Fréquence du son de contrôle facultatif, en hertz.</summary>
    internal const int ToneFrequency = 440;
    /// <summary>Amplitude PCM du son de contrôle, inférieure aux limites du type Int16.</summary>
    internal const int ToneAmplitude = 2000;
}
