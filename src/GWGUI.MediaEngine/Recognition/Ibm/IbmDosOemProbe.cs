using GWGUI.MediaEngine.FileSystems.Fat;

namespace GWGUI.MediaEngine.Recognition.Ibm;

/// <summary>Reconnaît les identifiants OEM DOS documentés d'un secteur d'amorçage IBM.</summary>
internal static class IbmDosOemProbe
{
    /// <summary>Préfixes OEM DOS acceptés au début du champ fixe.</summary>
    private static IReadOnlyList<string> Prefixes { get; } = Array.AsReadOnly(new[] { "IBM", "MSDOS", "MSWIN", "DOS", "FRDOS", "FREEDOS", "COPYDISK" });

    /// <summary>Indique si le champ OEM normalisé commence par un identifiant DOS connu.</summary>
    public static bool IsKnownDosOem(ReadOnlySpan<byte> boot)
    {
        if (boot.Length < FatBpbLayout.OemOffset + FatBpbLayout.OemLength) return false;
        var oem = System.Text.Encoding.ASCII.GetString(boot.Slice(FatBpbLayout.OemOffset, FatBpbLayout.OemLength)).Trim(FatBpbLayout.NullPadding, FatBpbLayout.SpacePadding).ToUpperInvariant();
        return Prefixes.Any(prefix => oem.StartsWith(prefix, StringComparison.Ordinal));
    }
}
