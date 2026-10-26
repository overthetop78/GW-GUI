using GWGUI.MediaFileSystems.Interfaces;

namespace GWGUI.MediaEngine.Contracts;

/// <summary>Describes the files and optional hints supplied to a media image reader.</summary>
public sealed record MediaSourceDescriptor(string PrimaryPath, IReadOnlyList<string> AssociatedPaths, long? KnownLength = null, string? RequestedFormatId = null) : IMediaSourceDescriptor
{
    /// <summary>Profil explicite du signal ou autres indications de lecture fournies par l'appelant.</summary>
    public IReadOnlyDictionary<string, string>? Parameters { get; init; }
}
