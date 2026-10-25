using System.IO;

namespace GWGUI.MediaEngine.Interfaces;

/// <summary>Préparations de contenus pour un consommateur, réalisées par les lecteurs et convertisseurs existants.</summary>
public interface IMediaRuntimePreparation
{
    /// <summary>Extensions du catalogue MediaEngine, indexées par leur nom technique invariant.</summary>
    IReadOnlyDictionary<string, string> FileExtensions { get; }
    /// <summary>Taille en octets d'un secteur optique brut.</summary>
    int RawOpticalSectorSize { get; }
    /// <summary>Taille en octets d'un secteur audio optique.</summary>
    int AudioSectorSize { get; }
    /// <summary>Taille en octets des sous-canaux d'un secteur optique.</summary>
    int OpticalSubchannelSize { get; }
    /// <summary>Convertit une capture SCP vers une image ADF, ATR ou ST avec le convertisseur existant.</summary>
    /// <param name="sourcePath">Capture source empruntée, conservée intacte.</param>
    /// <param name="outputPath">Destination choisie et gérée par l'appelant.</param>
    /// <param name="outputExtension">Extension cible du catalogue, avec son point.</param>
    /// <param name="cancellationToken">Annulation transmise au convertisseur.</param>
    /// <returns>Attente de la conversion ; aucune session d'émulation n'est créée.</returns>
    Task ConvertScpAsync(string sourcePath, string outputPath, string outputExtension, CancellationToken cancellationToken = default);
    /// <summary>Lit les commandes CUE sans ouvrir leurs fichiers de pistes.</summary>
    IMediaCueSheet ReadCueSheet(IEnumerable<string> lines);
    /// <summary>Vérifie une disposition binaire unique avec pistes audio ou MODE1/2352 numérotées et indexées.</summary>
    /// <param name="sheet">Document retourné par ce service.</param>
    /// <param name="maximumTracks">Nombre maximal de pistes admis par le consommateur.</param>
    /// <returns>True si la disposition satisfait ces contraintes ; aucune exception métier n'est construite.</returns>
    bool IsSingleBinaryRawAudioCue(IMediaCueSheet sheet, int maximumTracks);
    /// <summary>Remplace le chemin du fichier unique en conservant pistes, catalogue et métadonnées.</summary>
    IMediaCueSheet WithSingleCueFile(IMediaCueSheet sheet, string declaredPath);
    /// <summary>Écrit un document préparé dans un flux emprunté, qui reste ouvert après l'appel.</summary>
    Task WriteCueSheetAsync(IMediaCueSheet sheet, Stream destination, CancellationToken cancellationToken = default);
    /// <summary>Prépare les pistes CUE, convertit le PCM WAVE en secteurs audio et réécrit les chemins relatifs.</summary>
    /// <param name="sourceCue">Chemin du CUE source emprunté.</param>
    /// <param name="stagedCue">Chemin du CUE dans le répertoire possédé par l'appelant.</param>
    /// <param name="stageFile">Fonction du propriétaire copiant chaque piste et retournant son chemin préparé.</param>
    /// <param name="unsupported">Fabrique empruntée du diagnostic métier pour une piste WAVE non audio.</param>
    /// <param name="copyBufferBytes">Capacité du tampon de copie choisie par l'appelant, en octets.</param>
    /// <param name="cancellationToken">Annulation des lectures et écritures.</param>
    /// <returns>Attente de l'écriture ; les flux créés sont fermés dans finally et les sources restent intactes.</returns>
    Task PrepareCueWaveFilesAsync(string sourceCue, string stagedCue, Func<string, string> stageFile, Func<Exception> unsupported, int copyBufferBytes, CancellationToken cancellationToken = default);
}
