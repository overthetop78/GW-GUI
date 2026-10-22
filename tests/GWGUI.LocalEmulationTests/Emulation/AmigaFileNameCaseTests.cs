using Hst.Amiga;
using Hst.Amiga.FileSystems.FastFileSystem;

namespace GWGUI.Tests.Emulation;

public sealed class AmigaFileNameCaseTests
{
    [Theory]
    [InlineData('a', 'A', 'A')]
    [InlineData('z', 'Z', 'Z')]
    [InlineData('à', 'à', 'À')]
    [InlineData('é', 'é', 'É')]
    [InlineData('þ', 'þ', 'Þ')]
    [InlineData('÷', '÷', '÷')]
    [InlineData('ÿ', 'ÿ', 'ÿ')]
    [InlineData('Ā', 'Ā', 'Ā')]
    public void CharacterRulesKeepTheAmigaAsciiAndLatinOneLimits(char source, char standard, char international)
    {
        Assert.Equal(standard, AmigaTextHelper.ToUpper(source));
        Assert.Equal(standard, FastFileSystemDirectory.ToUpper(source));
        Assert.Equal(international, AmigaTextHelper.InternationalToUpper(source));
        Assert.Equal(international, FastFileSystemDirectory.IntlToUpper(source));
    }

    [Theory]
    [InlineData(false, "AàÉ÷ÿÞZ")]
    [InlineData(true, "AÀÉ÷ÿÞZ")]
    public void FileNameModesPreserveTheSameStringAndHashEquivalence(bool international, string expected)
    {
        const string source = "aàÉ÷ÿÞz";
        Assert.Equal(expected, AmigaTextHelper.ToUpper(source, international));
        Assert.Equal(expected, FastFileSystemDirectory.MyToUpper(source, international));
        Assert.Equal(FastFileSystemDirectory.GetHashValue(72, expected, international),
            FastFileSystemDirectory.GetHashValue(72, source, international));
    }
}
