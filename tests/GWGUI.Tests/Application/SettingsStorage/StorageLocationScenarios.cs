using GWGUI.Domain.Settings;
using GWGUI.Tests.Application.TestInfrastructure;
namespace GWGUI.Tests.Application.SettingsStorage;
internal static class StorageLocationScenarios
{
    public static async Task Paths()
    {
        foreach(var portable in new[]{false,true})
        {
            var result=GWGUI.App.Services.Storage.StoragePaths.ResolveDataDirectory("virtual-app","virtual-roaming",path=>{
                Assert.Equal(Path.Combine("virtual-app","portable.flag"),path);
                return portable;
            });
            Assert.Equal(portable?Path.Combine("virtual-app","Data"):Path.Combine("virtual-roaming","GW GUI"),result);
        }
        var files=new SyntheticData();
        await SettingsRoundTripScenarios.Store(files).SaveAsync(new AppSettings());
        Assert.All(files.Calls,call=>Assert.StartsWith("virtual",call[(call.IndexOf(':')+1)..]));
        Assert.Equal(new[]{SettingsRoundTripScenarios.PathName},files.Files.Keys);
    }
}
