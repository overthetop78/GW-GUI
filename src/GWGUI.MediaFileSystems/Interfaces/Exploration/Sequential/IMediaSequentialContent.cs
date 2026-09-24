

namespace GWGUI.MediaFileSystems.Exploration.Sequential;

/// <summary>Résultat du décodage d'une bande transmis à l'explorateur de fichiers.</summary>
public interface IMediaSequentialContent
{
    string? DecoderId { get; }
    IReadOnlyList<MediaSequentialDecodedBlock> Blocks { get; }
    IReadOnlyList<string> Diagnostics { get; }
}
