using System.Text;

namespace GWGUI.Emulation.HardDisks.FileSystems;

internal static class ClassicMacLabel
{
    private static readonly Encoding Encoding=CodePagesEncodingProvider.Instance.GetEncoding(10000,
        EncoderFallback.ExceptionFallback,DecoderFallback.ExceptionFallback)!;
    internal static byte[] Encode(string label)
    {
        ArgumentNullException.ThrowIfNull(label);
        var bytes=Encoding.GetBytes(label);
        if(bytes.Length is <1 or >27 || bytes.Any(b=>b<32 || b==127 || b==(byte)':'))
            throw new ArgumentException("Classic Macintosh volume labels require 1–27 Mac Roman bytes without control characters or ':'.",nameof(label));
        return bytes;
    }
}
