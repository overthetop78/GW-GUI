using System.Text;

namespace GWGUI.Emulation.HardDisks.FileSystems;

internal static class HfsVolumeLabel
{
    internal static string Normalize(string label)
    {
        ArgumentNullException.ThrowIfNull(label);
        if (label.Any(c => c > '\u00ff' && c is not (>= '\u0300' and <= '\u036f')))
            throw new ArgumentException("This HFS label profile does not cover this Unicode range.");
        // Latin-1 canonical decompositions are stable across the Unicode versions used by HFS+.
        // Accept both composed and decomposed input, without claiming the full HFS Unicode tables.
        var composed = label.Normalize(NormalizationForm.FormC);
        if (composed.Length == 0 || composed.Any(c => c < 32 || c is >= '\u007f' and <= '\u009f' || c > '\u00ff' || c is ':' or '/'))
            throw new ArgumentException("This HFS label profile supports printable Latin-1 characters without ':' or '/'.");
        var decomposed = composed.Normalize(NormalizationForm.FormD);
        if (decomposed.Length > 255) throw new ArgumentException("The decomposed HFS label exceeds 255 UTF-16 code units.");
        return decomposed;
    }
}
