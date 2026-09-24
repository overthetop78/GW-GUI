using System.Collections.ObjectModel;
using GWGUI.MediaEngine.Enums;

namespace GWGUI.MediaEngine.Contracts;

/// <summary>Associates one CUE source file declaration with its ordered tracks.</summary>
public sealed class CueFileDescriptor
{
    public CueFileDescriptor(
        string declaredPath,
        CueFileKind kind,
        IReadOnlyList<CueTrackDeclaration> tracks)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(declaredPath);
        ArgumentNullException.ThrowIfNull(tracks);
        if (tracks.Count == 0) throw new ArgumentException("A CUE file declaration requires at least one track.", nameof(tracks));

        DeclaredPath = declaredPath;
        Kind = kind;
        Tracks = new ReadOnlyCollection<CueTrackDeclaration>(tracks.ToArray());
    }

    public string DeclaredPath { get; }
    public CueFileKind Kind { get; }
    public IReadOnlyList<CueTrackDeclaration> Tracks { get; }
}
