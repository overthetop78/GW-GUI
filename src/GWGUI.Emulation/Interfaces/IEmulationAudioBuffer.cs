namespace GWGUI.Emulation.Interfaces;

/// <summary>Tampon audio borné partagé par les producteurs de trames et leurs consommateurs.</summary>
/// <remarks>L’implémentation possède sa file et ses compteurs ; les blocs reçus restent des données stables.
/// Le choix de consommation distingue un manque réel d’une simple fin de vidage de la file par une frame.</remarks>
public interface IEmulationAudioBuffer
{
    /// <summary>Nombre de trames audio actuellement en attente, lu sous le verrou du tampon.</summary>
    int BufferedFrames { get; }
    /// <summary>Nombre cumulé de blocs rognés ou retirés après un dépassement de capacité.</summary>
    long OverrunCount { get; }
    /// <summary>Nombre cumulé de lectures vides effectuées avec comptage des manques.</summary>
    long UnderrunCount { get; }
    /// <summary>Lecture directe du compteur de dépassements depuis le thread producteur natif propriétaire.</summary>
    long NativeOverrunCount { get; }
    /// <summary>Ajoute un bloc en appliquant les règles de capacité et de rétention communes.</summary>
    /// <param name="chunk">Bloc stable dont la fréquence détermine la capacité maximale du tampon.</param>
    void Enqueue(AudioChunk chunk);
    /// <summary>Ajoute un bloc avec les règles de rétention et de disposition du producteur natif.</summary>
    /// <typeparam name="TSource">Type du producteur emprunté pendant la lecture de fréquence.</typeparam>
    /// <param name="chunk">Bloc natif à retenir dans la file.</param>
    /// <param name="source">Producteur fournissant la fréquence courante.</param>
    /// <param name="readSampleRate">Lecture de la fréquence du producteur, en hertz.</param>
    /// <param name="multichannel">Active la rétention native multicanale existante.</param>
    void EnqueueNative<TSource>(AudioChunk chunk, TSource source, Func<TSource, int> readSampleRate, bool multichannel = false);
    /// <summary>Consomme un bloc et compte une lecture vide comme un manque de données.</summary>
    /// <param name="chunk">Bloc retiré, ou null lorsque la file est vide.</param>
    /// <returns>True lorsqu’un bloc a été retiré.</returns>
    bool TryDequeue(out AudioChunk? chunk);
    /// <summary>Consomme un bloc sans compter la fin normale du vidage comme un manque.</summary>
    /// <param name="chunk">Bloc retiré, ou null lorsque tous les blocs ont déjà été consommés.</param>
    /// <returns>True lorsqu’un bloc a été retiré.</returns>
    bool TryDequeueNative(out AudioChunk? chunk);
    /// <summary>Vide la file et détache le dernier bloc du producteur sous le même verrou.</summary>
    /// <param name="clearLatest">Détachement du dernier bloc, emprunté au propriétaire de session.</param>
    void ClearNative(Action clearLatest);
}
