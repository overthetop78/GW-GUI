namespace GWGUI.MediaEngine.Containers.TeleDisk;

internal static class Td0Format
{
    public static ReadOnlySpan<byte> UncompressedSignature => "TD"u8;
    public static ReadOnlySpan<byte> AdvancedCompressionSignature => "td"u8;
    public const int SignatureLength = 2;
}
