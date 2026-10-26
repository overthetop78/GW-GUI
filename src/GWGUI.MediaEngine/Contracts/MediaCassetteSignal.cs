namespace GWGUI.MediaEngine.Contracts;

/// <summary>Signal mono d'une cassette, indépendant du système de fichiers et de la machine qui le lit.</summary>
/// <param name="Samples">Échantillons PCM signés sur seize bits, possédés par le résultat.</param>
/// <param name="SampleRate">Cadence du signal, en hertz ; elle reste distincte de la sortie sonore de l'émulateur.</param>
/// <param name="BlockPositions">Repères de blocs dans le signal préparé.</param>
/// <param name="Blocks">Données et métadonnées fournies par le décodeur de la machine demandée.</param>
public sealed record MediaCassetteSignal(ReadOnlyMemory<short> Samples, int SampleRate, IReadOnlyList<int>? BlockPositions = null, IReadOnlyList<MediaCassetteBlock>? Blocks = null);

/// <summary>Bloc validé par un décodeur existant ; sa position en échantillons rattache les données à la bande.</summary>
public sealed record MediaCassetteBlock(int SamplePosition, ReadOnlyMemory<byte> Data, bool? IntegrityValid, IReadOnlyDictionary<string, string> Metadata);
