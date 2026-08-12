using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.FileSystems.Amiga;

namespace GWGUI.Tests;

/// <summary>Vérifie les noms affichés variables des systèmes de fichiers.</summary>
public sealed class FileSystemDisplayNamesTests
{
    /// <summary>Vérifie les variantes AmigaDOS.</summary>
    [Theory]
    [InlineData(0, "AmigaDOS OFS")]
    [InlineData(1, "AmigaDOS FFS")]
    [InlineData(7, "AmigaDOS FFS Long Names")]
    [InlineData(8, "AmigaDOS")]
    public void ResolvesAmigaDosVariant(int dosType, string expected) => Assert.Equal(expected, FileSystemDisplayNames.AmigaDos((AmigaDosVariant)dosType));

    /// <summary>Vérifie les variantes CP/M Amstrad.</summary>
    [Theory]
    [InlineData(DiskImageFormatIds.AmstradCpc, "Amstrad CPC CP/M")]
    [InlineData(DiskImageFormatIds.AmstradPcw, "Amstrad PCW CP/M Plus")]
    public void ResolvesAmstradVariant(string formatId, string expected) => Assert.Equal(expected, FileSystemDisplayNames.AmstradCpm(formatId));

    /// <summary>Vérifie les variantes FAT12.</summary>
    [Theory]
    [InlineData(DiskImageFormatIds.Ibm720, "IBM PC FAT12")]
    [InlineData(DiskImageFormatIds.Msx2Dd, "MSX-DOS FAT12")]
    [InlineData(DiskImageFormatIds.AtariSt720, "Atari TOS FAT12")]
    public void ResolvesFatVariant(string formatId, string expected) => Assert.Equal(expected, FileSystemDisplayNames.Fat12(formatId));
}
