namespace GWGUI.MediaEngine.Contracts;

/// <summary>Fichier FLAC externe et actions mécaniques associées.</summary>
internal sealed record RecordedMechanicalSoundSample(string FilePath, IReadOnlyList<RecordedMechanicalSoundOperation> Operations)
{
    /// <summary>Identifiant du fichier utilisé comme clé du cache audio.</summary>
    public string Id => FilePath[..^GWGUI.MediaEngine.Constants.DiskImageFileExtensions.Flac.Length];
}
