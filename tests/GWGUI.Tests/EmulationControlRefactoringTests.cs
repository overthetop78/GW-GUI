using GWGUI.App.Controls;
using GWGUI.App.Input;
using GWGUI.Emulation;
using GWGUI.Emulation.Amiga;

namespace GWGUI.Tests;

public sealed class EmulationControlRefactoringTests
{
    [Theory]
    [InlineData("disk.adf", AmigaMediaKind.Floppy)]
    [InlineData("disk.hdf", AmigaMediaKind.HardDrive)]
    [InlineData("disc.chd", AmigaMediaKind.CompactDisc)]
    [InlineData("game.lha", AmigaMediaKind.WhdLoad)]
    [InlineData("machine.uae", AmigaMediaKind.Configuration)]
    public void MediaKind_IsInferredFromExtension(string path, AmigaMediaKind expected) =>
        Assert.Equal(expected, EmulationOptionValueConverter.InferMediaKind(path));

    [Theory]
    [InlineData("1.50", 150)]
    [InlineData("1,50×", 150)]
    [InlineData("0", 1)]
    [InlineData("20", 1000)]
    public void MouseSpeed_IsNormalizedAndClamped(string ratio, int expected) =>
        Assert.Equal(expected, EmulationOptionValueConverter.MouseSpeedPercentage(ratio));

    [Fact]
    public void ConfigurationOption_UsesFallbackWhenMissing()
    {
        var configuration = new AmigaMachineConfiguration("A500", "kick.rom", Options: new Dictionary<string, string>
        {
            ["known"] = "stored"
        });
        Assert.Equal("stored", AmigaConfigurationDocuments.GetOption(configuration, "known", "fallback"));
        Assert.Equal("fallback", AmigaConfigurationDocuments.GetOption(configuration, "missing", "fallback"));
    }

    [Fact]
    public void KeyboardMap_IgnoresUnknownSourceKeys()
    {
        var map = EmulationShortcutMap.KeyboardMap(new Dictionary<string, EmulationKey>
        {
            [nameof(EmulationKey.A)] = EmulationKey.F1,
            ["NotAnAmigaKey"] = EmulationKey.F2
        });
        Assert.Single(map);
        Assert.Equal(EmulationKey.A, map[EmulationKey.F1]);
    }
}
