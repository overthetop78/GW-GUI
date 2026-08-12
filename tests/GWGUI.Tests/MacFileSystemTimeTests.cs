using GWGUI.MediaEngine.FileSystems.Apple.Macintosh;

namespace GWGUI.Tests;

/// <summary>Vérifie la conversion des horodatages Macintosh classiques.</summary>
public sealed class MacFileSystemTimeTests
{
    /// <summary>Vérifie l'absence associée à zéro et une date valide.</summary>
    [Fact]
    public void ConvertsZeroAndValidSeconds()
    {
        Assert.Null(MacFileSystemTime.FromSeconds(0));
        Assert.Equal(MacFileSystemTime.Epoch.AddSeconds(60), MacFileSystemTime.FromSeconds(60));
    }

    /// <summary>Vérifie qu'un dépassement produit une absence.</summary>
    [Fact]
    public void ReturnsNullOnOverflow() => Assert.Null(MacFileSystemTime.FromSeconds(long.MaxValue));
}
