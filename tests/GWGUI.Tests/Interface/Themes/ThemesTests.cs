using GWGUI.Domain.Settings;
using GWGUI.Tests.Application.TestInfrastructure;
namespace GWGUI.Tests.Interface.Themes;
[Collection("WPF")]
public class ThemesTests(StaExecutionScenarios sta)
{
    [Theory]
    [InlineData(AppTheme.Light,false,false)]
    [InlineData(AppTheme.Light,true,false)]
    [InlineData(AppTheme.Dark,false,true)]
    [InlineData(AppTheme.System,false,false)]
    [InlineData(AppTheme.System,true,true)]
    public Task ThemeUsesInjectedSystemChoice(AppTheme theme,bool systemDark,bool expected)=>sta.Run(()=>ThemeResourcesScenarios.Apply(theme,systemDark,expected));
    [Fact] public Task ExistingControlFollowsThemeChanges()=>sta.Run(ThemeResourcesScenarios.Refresh);
}
