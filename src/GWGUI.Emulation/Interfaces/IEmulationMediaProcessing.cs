using System.IO;

namespace GWGUI.Emulation.Interfaces;

/// <summary>Préparations de médias accessibles aux modules par le SDK et réalisées par MediaEngine.</summary>
/// <remarks>Les modules conservent les choix de formats, contraintes du cœur et répertoires de session.
/// Les types des bibliothèques médias restent encapsulés derrière ces contrats.</remarks>
public interface IEmulationMediaProcessing
{
    /// <summary>Extensions du catalogue, indexées par leur nom invariant, par exemple Iso ou Scp.</summary>
    IReadOnlyDictionary<string, string> FileExtensions { get; }
    /// <summary>Taille en octets d'un secteur optique brut.</summary>
    int RawOpticalSectorSize { get; }
    /// <summary>Taille en octets d'un secteur audio optique.</summary>
    int AudioSectorSize { get; }
    /// <summary>Taille en octets des sous-canaux optiques.</summary>
    int OpticalSubchannelSize { get; }
    /// <summary>Convertit une capture SCP vers l'extension demandée par le module.</summary>
    /// <param name="sourcePath">Source empruntée, conservée intacte.</param>
    /// <param name="outputPath">Destination préparée et gérée par le module.</param>
    /// <param name="outputExtension">Extension ADF, ATR ou ST du catalogue, avec son point.</param>
    /// <param name="cancellationToken">Annulation transmise au convertisseur existant.</param>
    /// <returns>Attente de la conversion, sans démarrer de cœur.</returns>
    Task ConvertScpAsync(string sourcePath, string outputPath, string outputExtension, CancellationToken cancellationToken = default);
    /// <summary>Lit les lignes d'un CUE et conserve ses pistes et métadonnées sans ouvrir les fichiers déclarés.</summary>
    IEmulationCueSheet ReadCueSheet(IEnumerable<string> lines);
    /// <summary>Contrôle un fichier binaire unique avec pistes audio ou MODE1/2352, numérotées et indexées.</summary>
    /// <param name="sheet">Document retourné par ce service.</param>
    /// <param name="maximumTracks">Limite native choisie par le module.</param>
    /// <returns>Compatibilité de la disposition ; le module choisit son diagnostic en cas de refus.</returns>
    bool IsSingleBinaryRawAudioCue(IEmulationCueSheet sheet, int maximumTracks);
    /// <summary>Remplace le chemin du fichier unique sans modifier les pistes ou métadonnées du document source.</summary>
    IEmulationCueSheet WithSingleCueFile(IEmulationCueSheet sheet, string declaredPath);
    /// <summary>Écrit un document dans un flux emprunté qui reste ouvert après l'appel.</summary>
    Task WriteCueSheetAsync(IEmulationCueSheet sheet, Stream destination, CancellationToken cancellationToken = default);
    /// <summary>Prépare les fichiers du CUE et convertit leurs pistes WAVE en données audio brutes.</summary>
    /// <param name="sourceCue">CUE source emprunté.</param>
    /// <param name="stagedCue">CUE de destination appartenant à la session du module.</param>
    /// <param name="stageFile">Copie de session fournie par le propriétaire, retournant le chemin préparé.</param>
    /// <param name="unsupported">Diagnostic du module pour une piste WAVE qui n'est pas audio.</param>
    /// <param name="copyBufferBytes">Capacité du tampon de copie, en octets, choisie par le module.</param>
    /// <param name="cancellationToken">Annulation des lectures et écritures.</param>
    /// <returns>Attente de la préparation ; les flux créés par le traitement sont fermés dans finally.</returns>
    Task PrepareCueWaveFilesAsync(string sourceCue, string stagedCue, Func<string, string> stageFile, Func<Exception> unsupported, int copyBufferBytes, CancellationToken cancellationToken = default);
}
