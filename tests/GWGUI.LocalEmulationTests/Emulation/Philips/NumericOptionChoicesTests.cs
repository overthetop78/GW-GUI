using GWGUI.Emulation.Philips.Emulators.O2em.Dictionaries;
using GWGUI.Emulation.Philips.Emulators.SameCdi.Dictionaries;

namespace GWGUI.Tests.Emulation.Philips;

public sealed class NumericOptionChoicesTests
{
    [Theory]
    [InlineData("o2em_vkbd_transparency", "0", "0,25,50,75")]
    [InlineData("o2em_audio_volume", "50", "0,5,10,15,20,25,30,35,40,45,50,55,60,65,70,75,80,85,90,95,100")]
    [InlineData("o2em_voice_volume", "70", "0,5,10,15,20,25,30,35,40,45,50,55,60,65,70,75,80,85,90,95,100")]
    [InlineData("o2em_low_pass_range", "60", "5,10,15,20,25,30,35,40,45,50,55,60,65,70,75,80,85,90,95")]
    public void O2emPercentagesRetainNativeValuesAndDisplay(string key, string defaultValue, string expectedValues)
    {
        var option = Assert.Single(O2emOptionCatalog.All, option => option.Key == key);
        Assert.Equal(defaultValue, option.DefaultValue);
        Assert.Equal(defaultValue, option.CurrentValue);
        Assert.Equal(expectedValues.Split(','), option.Values.Select(choice => choice.Value));
        Assert.All(option.Values, choice =>
        {
            Assert.Equal(choice.Value + "%", choice.Label);
            Assert.Equal(choice.Label, choice.InvariantDisplayValue);
            Assert.Null(choice.ResourceKey);
        });
    }

    [Fact]
    public void CdiCpuPercentagesRetainFineStepsAndTranslatedDefault()
    {
        var option = Assert.Single(SameCdiOptionCatalog.All, option => option.Key == "same_cdi_cpu_overclock");
        const string expectedValues = "default,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,60,65,70,75,80,85,90,95,100,105,110,115,120,125,130,135,140,145,150";
        Assert.Equal("default", option.DefaultValue);
        Assert.Equal("default", option.CurrentValue);
        Assert.Equal(expectedValues.Split(','), option.Values.Select(choice => choice.Value));
        Assert.Equal("default", option.Values[0].Label);
        Assert.Equal("Emulation.Value.Default", option.Values[0].ResourceKey);
        Assert.Null(option.Values[0].InvariantDisplayValue);
        Assert.All(option.Values.Skip(1), choice =>
        {
            Assert.Equal(choice.Value, choice.Label);
            Assert.Equal(choice.Value + " %", choice.InvariantDisplayValue);
            Assert.Null(choice.ResourceKey);
        });
    }

    [Fact]
    public void CdiResolutionsRetainOrderDimensionsAndDefault()
    {
        var option = Assert.Single(SameCdiOptionCatalog.All, option => option.Key == "same_cdi_altres");
        const string expectedValues = "640x360,640x480,800x600,800x450,960x720,960x540,1024x768,1024x576,1280x960,1280x720,1600x1200,1600x900,1440x1080,1920x1080,1920x1440,2560x1440,2880x2160,3840x2160";
        Assert.Equal("640x480", option.DefaultValue);
        Assert.Equal("640x480", option.CurrentValue);
        Assert.Equal(expectedValues.Split(','), option.Values.Select(choice => choice.Value));
        Assert.All(option.Values, choice =>
        {
            Assert.Equal(choice.Value, choice.Label);
            Assert.Equal(choice.Value, choice.InvariantDisplayValue);
            Assert.Null(choice.ResourceKey);
        });
    }
}
