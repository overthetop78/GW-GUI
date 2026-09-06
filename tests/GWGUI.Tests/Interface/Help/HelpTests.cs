using GWGUI.App.Dictionaries.Localization;
namespace GWGUI.Tests.Interface.Help;
[Collection("WPF")]
public class HelpTests(GWGUI.Tests.Application.TestInfrastructure.StaExecutionScenarios sta)
{
    [Theory] [InlineData("fr-FR", "fr-FR")] [InlineData("ar-SA", "ar-SA")] [InlineData("zh-CN", "zh-Hans")]
    public Task MenuOpensLocalizedGuideAndHandlesBrowserFailure(string language, string expected) => sta.Run(() => HelpTargetScenarios.Command(language, expected));
    public static IEnumerable<object[]> Languages()=>UiLanguageCatalog.Available.Select(x=>new object[]{x.Code,x.Code});
    [Theory][MemberData(nameof(Languages))]
    [InlineData("zh-CN","zh-Hans")]
    [InlineData("zh-SG","zh-Hans")]
    [InlineData("zh-TW","zh-Hant")]
    [InlineData("zh-HK","zh-Hant")]
    [InlineData("fr","fr-FR")]
    [InlineData("unknown-language","en-US")]
    [InlineData("","en-US")]
    public void WikiTargetUsesResolvedLanguage(string language,string expected)=>HelpTargetScenarios.Target(language,expected);
}
