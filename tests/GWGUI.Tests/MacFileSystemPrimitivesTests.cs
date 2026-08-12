using GWGUI.MediaEngine.FileSystems.Apple.Macintosh;

namespace GWGUI.Tests;

/// <summary>Vérifie les primitives communes aux systèmes de fichiers Macintosh.</summary>
public sealed class MacFileSystemPrimitivesTests
{
    /// <summary>Vérifie la lecture big-endian des entiers de 16 et 32 bits.</summary>
    [Fact]
    public void ReadsBigEndianIntegers()
    {
        byte[] bytes = [0x12, 0x34, 0x56, 0x78];
        Assert.Equal(0x1234, MacFileSystemPrimitives.ReadUInt16(bytes, 0));
        Assert.Equal(0x12345678u, MacFileSystemPrimitives.ReadUInt32(bytes, 0));
    }

    /// <summary>Vérifie une chaîne Pascal vide, maximale et contenant le séparateur Macintosh.</summary>
    [Fact]
    public void DecodesPascalStringsAndMacintoshSeparator()
    {
        Assert.Equal(string.Empty, MacFileSystemPrimitives.ReadPascalString([0], 0, 31));
        var maximum = new byte[32];
        maximum[0] = 31;
        Array.Fill(maximum, (byte)'A', 1, 31);
        Assert.Equal(new string('A', 31), MacFileSystemPrimitives.ReadPascalString(maximum, 0, 31));
        Assert.Equal("DOSSIER/FICHIER", MacFileSystemPrimitives.DecodeName("DOSSIER:FICHIER"u8));
    }
}
