namespace GWGUI.Emulation.Interfaces;

/// <summary>Traitements audio communs accessibles aux modules par un contrat public.</summary>
/// <remarks>Le service ne possède ni sortie ni tampon natif. Les pointeurs restent valides pendant la copie ;
/// les tableaux renvoyés appartiennent à l’appelant. Les formats stéréo et multicanaux gardent leurs règles de taille.</remarks>
public interface IEmulationAudioProcessing
{
    /// <summary>Vérifie la latence avec les choix communs et la culture invariante.</summary>
    bool HasInvalidConfiguredLatency(string? value);

    /// <summary>Vérifie le volume entier dans les bornes fournies.</summary>
    bool HasInvalidConfiguredVolume(string? value, int minimum, int maximum);

    /// <summary>Résout la sortie configurée ; le choix automatique du module devient null.</summary>
    string? ResolveRuntimeOutput(IReadOnlyDictionary<string, string> options, string key, string defaultOutput);

    /// <summary>Résout la latence configurée ou utilise la valeur commune par défaut.</summary>
    int ResolveRuntimeLatency(IReadOnlyDictionary<string, string> options, string key);

    /// <summary>Écrit dans la sortie empruntée, avec la règle de volume et de conversion historique de cette opération.</summary>
    void WriteVolume(ref IAudioOutput? output, ref float volume, ReadOnlySpan<short> samples);

    /// <summary>Calcule la limite commune de frames tamponnées à partir de la fréquence en Hz.</summary>
    int MaximumBufferedFrames(int sampleRate);

    /// <summary>Conserve les frames natives les plus récentes avec la disposition des canaux demandée.</summary>
    AudioChunk RetainNativeNewestFrames(AudioChunk chunk, int maximumFrames, bool multichannel);

    /// <summary>Copie les frames stéréo avec le tableau non initialisé et les contrôles arithmétiques existants.</summary>
    short[] CopyBatch(nint data, int frameCount);

    /// <summary>Copie les frames stéréo avec la politique d’allocation du compteur natif.</summary>
    short[] CopyBatch(nint data, nuint frameCount);

    /// <summary>Copie les frames du nombre de canaux fourni, avec contrôle des dépassements de taille.</summary>
    short[] CopyBatch(nint data, nuint frameCount, uint channelCount);

    /// <summary>Crée une frame stéréo gauche et droite.</summary>
    short[] SingleFrame(short left, short right);

    /// <summary>Conserve les frames stéréo les plus récentes dans la limite fournie.</summary>
    AudioChunk RetainNewestFrames(AudioChunk chunk, int maximumFrames);

    /// <summary>Borne le volume selon les règles numériques communes.</summary>
    float NormalizeVolume(float volume);

    /// <summary>Applique le volume dans le tableau réutilisable ; à volume maximal, emprunte les échantillons d’entrée.</summary>
    ReadOnlySpan<short> ApplyVolume(ReadOnlySpan<short> samples, float volume, ref short[] buffer);
}
