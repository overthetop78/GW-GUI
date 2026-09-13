using System.Buffers.Binary;

namespace GWGUI.MediaEngine.Formats.Floppy.Atx;

/// <summary>Constantes d'identification du format Atari ATX.</summary>
public static class AtxFormat
{
    public const uint Signature = 0x58385441;
    public const int SignatureLength = sizeof(uint);

    public static byte[] CreateSignature()
    {
        var bytes = new byte[SignatureLength];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, Signature);
        return bytes;
    }
}
