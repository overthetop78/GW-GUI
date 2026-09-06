using GWGUI.Domain.Settings;
using GWGUI.Infrastructure.Settings;
using GWGUI.Tests.Application.TestInfrastructure;
using System.Text.Json;
namespace GWGUI.Tests.Application.SettingsStorage;
internal static class SettingsRoundTripScenarios
{
    internal const string PathName="virtual/settings.json";
    internal static JsonSettingsStore Store(SyntheticData files)=>new(PathName,files,()=>new DateTime(2026,9,6,12,0,0,DateTimeKind.Utc));
    public static async Task Save()
    {
        var files=new SyntheticData();
        files.Seed(PathName,"old contents");
        await Store(files).SaveAsync(new AppSettings { Language="fr-FR" });
        using var json=JsonDocument.Parse(files.Text(PathName));
        Assert.Equal("fr-FR",json.RootElement.GetProperty("Language").GetString());
        Assert.Equal(SettingsMigrator.CurrentVersion,json.RootElement.GetProperty("SchemaVersion").GetInt32());
        Assert.Equal("old contents",files.Text(PathName+".bak"));
        Assert.False(files.Files.ContainsKey(PathName+".tmp"));
        Assert.Equal("fr-FR",(await Store(files).LoadAsync()).Language);
    }
    public static async Task Load()
    {
        var files=new SyntheticData();
        Assert.NotNull(await Store(files).LoadAsync());
        Assert.Empty(files.Files);
        files.Seed(PathName,"{\"SchemaVersion\":8,\"Language\":\"de-DE\"}");
        var result=await Store(files).LoadAsync();
        Assert.Equal("de-DE",result.Language);
        Assert.NotNull(result.Read);
        Assert.NotNull(result.Write);
    }
}
