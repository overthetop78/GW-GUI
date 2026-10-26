namespace GWGUI.MediaEngine.Contracts;

/// <summary>Un fichier source inchangé et les opérations qu’il enregistre.</summary>
internal sealed record RecordedMechanicalSoundSample(string FilePath, IReadOnlyList<RecordedMechanicalSoundOperation> Operations)
{
    /// <summary>Identifiant descriptif déduit du chemin, sans extension FLAC.</summary>
    public string Id => FilePath[..^GWGUI.MediaEngine.Constants.DiskImageFileExtensions.Flac.Length];

    /// <summary>Nom du fichier audio externe, extension comprise.</summary>
    public string FileName => FilePath[(FilePath.LastIndexOf('/') + 1)..];

    /// <summary>Nom original du fichier enregistré avant conversion FLAC.</summary>
    public required string SourceFileName { get; init; }

    /// <summary>Chemin original du fichier dans son dépôt source.</summary>
    public required string SourcePath { get; init; }

    /// <summary>Dépôt d’origine de l’enregistrement.</summary>
    public required string SourceRepository { get; init; }

    /// <summary>Révision du dépôt correspondant à l’enregistrement importé.</summary>
    public required string SourceRevision { get; init; }

    /// <summary>Empreinte d’intégrité du fichier original ; elle ne sert pas d’identifiant fonctionnel.</summary>
    public required string SourceSha256 { get; init; }
}
