using GWGUI.MediaFileSystems.Interfaces;

namespace GWGUI.MediaEngine.Contracts;

/// <summary>Describes the files and optional hints supplied to a media image reader.</summary>
public sealed record MediaSourceDescriptor(string PrimaryPath, IReadOnlyList<string> AssociatedPaths, long? KnownLength = null, string? RequestedFormatId = null) : IMediaSourceDescriptor
{
    /// <summary>Profil explicite du signal ou autres indications de lecture fournies par l'appelant.</summary>
    public IReadOnlyDictionary<string, string>? Parameters { get; init; }

    /// <summary>Lecteur emprunté pour suivre les accès au média ; il ne fait pas partie du format du fichier.</summary>
    public GWGUI.MediaEngine.Interfaces.IMediaDriveMechanics? Drive { get; init; }

    /// <summary>Suit les accès de la représentation ; un contrôleur runtime les signale lui-même.</summary>
    internal bool TrackDriveAccesses { get; init; } = true;

    /// <summary>Indique que les données sont consommées pour écrire le média de destination.</summary>
    internal bool DriveWriting { get; init; }
}
