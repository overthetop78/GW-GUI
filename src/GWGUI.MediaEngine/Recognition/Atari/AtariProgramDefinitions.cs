using System.Collections.Frozen;

namespace GWGUI.MediaEngine.Recognition.Atari;

/// <summary>Définit les extensions et la signature des programmes Atari ST.</summary>
internal static class AtariProgramDefinitions
{
    /// <summary>Extensions de programmes Atari ST reconnues.</summary>
    public static IReadOnlySet<string> Extensions { get; } = new[] { ".ttp", ".tos", ".acc", ".gtp" }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>Signature big-endian d'un exécutable Atari ST.</summary>
    public static ReadOnlySpan<byte> Signature => [0x60, 0x1a];
}
