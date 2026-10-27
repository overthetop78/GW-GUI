using System.IO;

namespace GWGUI.MediaEngine.Interfaces;

/// <summary>Préparations de contenus pour un consommateur, réalisées par les lecteurs et convertisseurs existants.</summary>
public interface IMediaRuntimePreparation
{
    /// <summary>Décrit les modèles de la famille déclarée, avec leurs formats, capacités et extensions centrales.</summary>
    IReadOnlyList<GWGUI.MediaEngine.Contracts.MediaFloppyDriveModel> FloppyDriveModels(string family, bool doubleDensity);
    /// <summary>Décrit les mécanismes HDD réellement proposés pour la famille.</summary>
    IReadOnlyList<GWGUI.MediaEngine.Contracts.MediaHardDiskDriveModel> HardDiskDriveModels(string family);
    /// <summary>Vérifie la géométrie réelle d'un support contre les capacités physiques du lecteur choisi.</summary>
    bool SupportsFloppyImage(string model, long cylinders, int heads, int sectorSize, int sectorsPerTrack, long sectorCount);
    /// <summary>Crée un disque vierge au format demandé par la fabrique et l'écrivain du catalogue.</summary>
    Task CreateBlankDiskAsync(string path, string formatId, CancellationToken cancellationToken = default);
    /// <summary>Crée le conteneur HDD et ses annexes, sans écraser une destination existante.</summary>
    Task CreateHardDiskAsync(string path, string formatId, GWGUI.MediaEngine.Contracts.HardDiskGeometry geometry, string? fileSystemId = null, CancellationToken cancellationToken = default, bool preallocate = false);
    /// <summary>Ouvre une image disque par ses lecteurs existants ; le consommateur possède et libère le résultat.</summary>
    Task<IMediaRuntimeDisk> OpenDiskAsync(string path, bool writable = false, string? formatId = null, CancellationToken cancellationToken = default, IMediaDriveMechanics? mechanics = null);

    /// <summary>Crée la mécanique possédée par un lecteur émulé, indépendamment de son image montée.</summary>
    IMediaDriveMechanics CreateDriveMechanics(string device);
    /// <summary>Lit le signal mono d'une cassette UEF, CSW, WAV ou RAW avec son profil explicite, sans modifier sa source.</summary>
    Task<GWGUI.MediaEngine.Contracts.MediaCassetteSignal> ReadCassetteAsync(string path, CancellationToken cancellationToken = default, string? machineId = null, IReadOnlyDictionary<string, string>? parameters = null);
    /// <summary>Formats de sortie qui conservent le signal complet d'un magnétophone.</summary>
    IReadOnlyList<string> CassetteRecordingExtensions { get; }
    /// <summary>Identifiants des profils de mécanisme disponibles pour Cassette, Floppy, HardDisk ou Optical.</summary>
    IReadOnlyList<string> MechanicalSoundProfiles(string device);
    /// <summary>Nom technique invariant d'un profil connu.</summary>
    string MechanicalSoundProfileDisplayName(string profile);
    /// <summary>Synthétise un fragment mono ; le consommateur possède sa sortie audio et son horloge.</summary>
    short[] RenderMechanicalSound(string device, string operation, int sampleRate, int sampleCount, long offset, string? profile = null, double volumePercent = GWGUI.MediaEngine.Constants.MechanicalSoundConstants.DefaultVolumePercent);
    double MechanicalSoundDurationSeconds(string device, string profile, string operation);
    short[] ProtectMechanicalAudioMix(IReadOnlyList<double> samples);
    /// <summary>Écrit atomiquement le signal complet sans décoder les fichiers qu'il contient.</summary>
    Task WriteCassetteAsync(string path, GWGUI.MediaEngine.Contracts.MediaCassetteSignal signal, CancellationToken cancellationToken = default, IReadOnlyDictionary<string, string>? parameters = null);
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
