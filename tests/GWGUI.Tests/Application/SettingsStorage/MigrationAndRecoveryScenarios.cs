using GWGUI.Domain.Settings;
using GWGUI.Domain.Settings.Engines;
using GWGUI.Tests.Application.TestInfrastructure;
namespace GWGUI.Tests.Application.SettingsStorage;
internal static class MigrationAndRecoveryScenarios
{
    public static void LegacyStructures(int version)
    {
        var settings=new AppSettings { SchemaVersion=version, EmulationStorageFolder=" virtual-emulation ", EmulationCaptureFolder="", EmulationStateFolder="", LastDiskImageFolder=" last " };
        settings.Read.FormatId="amiga.amigadoshd"; settings.Read.EnabledOptions.Add("internal-reader"); settings.Write.EnabledOptions.Add("internal-writer");
        settings.Conversion.SelectedFormats=["amiga.amigadoshd"]; settings.Conversion.ExplicitExtensions["amiga.amigadoshd"]=[".adf"]; settings.Conversion.TagPattern="CUSTOM {tag}";
        settings.Profiles.Add(new() { EnabledOptions=["format:amiga.amigadoshd","internal-reader","internal-writer"],Values=new(){{"extensions:amiga.amigadoshd",".adf"}} });
        settings.UnconfiguredControllers.Add(new());
        SettingsMigrator.Migrate(settings);
        var id=version<=1?"amiga.amigados_hd":"amiga.amigadoshd";
        Assert.Equal(id,settings.Read.FormatId); Assert.Equal(id,Assert.Single(settings.Conversion.SelectedFormats)); Assert.Equal(".adf",Assert.Single(settings.Conversion.ExplicitExtensions[id]));
        Assert.Contains("format:"+id,settings.Profiles[0].EnabledOptions); Assert.Equal(".adf",settings.Profiles[0].Values["extensions:"+id]);
        Assert.DoesNotContain("internal-reader",settings.Profiles[0].EnabledOptions); Assert.DoesNotContain("internal-writer",settings.Profiles[0].EnabledOptions);
        Assert.Equal(OperationEngine.Internal,settings.Engines.PhysicalRead); Assert.Equal(OperationEngine.Internal,settings.Engines.PhysicalWrite);
        Assert.Equal(OperationEngine.Internal,settings.Engines.Conversion); Assert.Equal(OperationEngine.Internal,settings.Engines.ExplorerRead);
        Assert.Equal(version<=3?"[{FAMILY}-{FORMAT}] ":"CUSTOM {FAMILY}-{FORMAT}",settings.Conversion.TagPattern);
        Assert.Equal(version<=6?0:1,settings.UnconfiguredControllers.Count);
        Assert.Equal("virtual-emulation",settings.EmulationStorageFolder); Assert.Equal(Path.Combine("virtual-emulation","Captures"),settings.EmulationCaptureFolder);
        Assert.Equal(Path.Combine("virtual-emulation","States"),settings.EmulationStateFolder); Assert.Equal("last",settings.LastDiskImageFolder);
        var json=System.Text.Json.JsonSerializer.Serialize(settings); SettingsMigrator.Migrate(settings); Assert.Equal(json,System.Text.Json.JsonSerializer.Serialize(settings));
    }
    public static void Migrate(int version)
    {
        var settings=new AppSettings { SchemaVersion=version, Language=" fr-FR " };
        settings.Read.EnabledOptions.Add("internal-reader");
        var migrated=SettingsMigrator.Migrate(settings);
        Assert.Equal(8,migrated.SchemaVersion);
        Assert.Equal("fr-FR",migrated.Language);
        if(version<8)
        {
            Assert.Equal(OperationEngine.Internal,migrated.Engines.PhysicalRead);
            Assert.DoesNotContain("internal-reader",migrated.Read.EnabledOptions);
        }
        Assert.Same(migrated,SettingsMigrator.Migrate(migrated));
    }
    public static async Task Recover()
    {
        var files=new SyntheticData();
        files.Seed(SettingsRoundTripScenarios.PathName,"{broken");
        files.Seed(SettingsRoundTripScenarios.PathName+".bak","{\"SchemaVersion\":8,\"Language\":\"pl-PL\"}");
        var loaded=await SettingsRoundTripScenarios.Store(files).LoadAsync();
        Assert.Equal("pl-PL",loaded.Language);
        Assert.Equal("{broken",files.Text(SettingsRoundTripScenarios.PathName+".invalid-20260906-120000000"));
        Assert.Equal(files.Text(SettingsRoundTripScenarios.PathName+".bak"),files.Text(SettingsRoundTripScenarios.PathName));
    }
    public static async Task FailedSave()
    {
        var files=new SyntheticData { FailOperation="move" };
        files.Seed(SettingsRoundTripScenarios.PathName,"original");
        await Assert.ThrowsAsync<IOException>(()=>SettingsRoundTripScenarios.Store(files).SaveAsync(new AppSettings()));
        Assert.Equal("original",files.Text(SettingsRoundTripScenarios.PathName));
        Assert.Equal("original",files.Text(SettingsRoundTripScenarios.PathName+".bak"));
        Assert.True(files.Files.ContainsKey(SettingsRoundTripScenarios.PathName+".tmp"));
    }
    public static async Task FutureSchema()
    {
        var files=new SyntheticData();
        const string original="{\"SchemaVersion\":999}";
        files.Seed(SettingsRoundTripScenarios.PathName,original);
        await SettingsRoundTripScenarios.Store(files).LoadAsync();
        Assert.Equal(original,files.Text(SettingsRoundTripScenarios.PathName));
        Assert.Equal(original,files.Text(SettingsRoundTripScenarios.PathName+".invalid-20260906-120000000"));
        Assert.Throws<NotSupportedException>(()=>SettingsMigrator.Migrate(new AppSettings{SchemaVersion=999}));
    }
}
