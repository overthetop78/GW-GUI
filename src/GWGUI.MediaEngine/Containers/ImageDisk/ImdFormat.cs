namespace GWGUI.MediaEngine.Containers.ImageDisk;

internal static class ImdFormat
{
    public static ReadOnlySpan<byte> Signature => "IMD"u8;
    public const int SignatureLength = 3;
    public const byte CommentTerminator = 0x1A;
}
