using GWGUI.Domain.Profiles;
namespace GWGUI.Tests.Application.Profiles;
internal static class ProfileStateScenarios
{
    public static void SessionFields()
    {
        var read=new GWGUI.App.ViewModels.Operations.ReadOperationViewModel { FileName="session-name",Folder="session-folder",SequenceValue="77",AutoNumber=true,ExpertArguments="--raw" };
        read.Tracks.Enabled=true; read.Tracks.Value="c=2-4:h=1";
        var values=read.CaptureValues(); var enabled=read.CaptureEnabledOptions();
        Assert.Equal(new[]{"adjust-speed","densel","diskdefs","expert","fake-index","pll","retries","revs","seek-retries","tracks"},values.Keys.Order());
        Assert.Equal("tracks",Assert.Single(enabled)); Assert.Equal("--raw",values["expert"]); Assert.Equal("c=2-4:h=1",values["tracks"]);
        read.ApplyOptions(new HashSet<string>(),new Dictionary<string,string>());
        Assert.Equal("session-name",read.FileName); Assert.Equal("session-folder",read.Folder); Assert.Equal("77",read.SequenceValue); Assert.True(read.AutoNumber);
        var write=new GWGUI.App.ViewModels.Operations.WriteOperationViewModel { SourcePath="session-source",ExpertArguments="--raw" }; write.NoVerify.Enabled=true;
        Assert.Equal(new[]{"densel","diskdefs","expert","fake-index","precomp","retries","tracks"},write.CaptureValues().Keys.Order());
        Assert.Equal("no-verify",Assert.Single(write.CaptureEnabledOptions())); write.ApplyOptions(new HashSet<string>(),new Dictionary<string,string>()); Assert.Equal("session-source",write.SourcePath);
        var conversion=new GWGUI.App.ViewModels.Conversion.ConversionOperationViewModel { SourcePath="session-input",OutputName="session-output",AddTags=true,ExpertArguments="--raw" };
        conversion.SetFormat("test.format",true,[".one",".two"]);
        var selected=conversion.CaptureProfileEnabled(); var captured=conversion.CaptureProfileValues();
        Assert.Equal(new[]{"format:test.format","tags"},selected.Order()); Assert.Equal(".one,.two",captured["extensions:test.format"]);
        Assert.Equal(new[]{"adjust-speed","diskdefs","expert","extensions:test.format","out-tracks","pll","tracks"},captured.Keys.Order());
        conversion.ApplyProfile(new HashSet<string>(),new Dictionary<string,string>()); Assert.Empty(conversion.SelectedFormats);
        Assert.Equal("session-input",conversion.SourcePath); Assert.Equal("session-output",conversion.OutputName);
        Assert.Equal(".one,.two",captured["extensions:test.format"]); Assert.Contains("tags",selected);
    }
    private static OperationProfile Profile(string id, string name, OperationKind operation = OperationKind.Read) =>
        new(id, operation, name, new Dictionary<string,string>{{"format","synthetic"}}, new HashSet<string>{"verify"});
    public static void Lifecycle(OperationKind operation)
    {
        var store = new InMemoryProfileStore(operation);
        var original = Assert.Single(store.GetAll());
        Assert.True(original.IsSystem);
        var profile = store.Save(Profile("first","sample",operation));
        Assert.Equal("synthetic", profile.Values["format"]);
        Assert.Contains("verify", profile.EnabledOptions);
        store.Rename(profile.Id, "renamed");
        Assert.Equal("renamed", store.GetAll().Single(p=>p.Id=="first").Name);
        var other = operation == OperationKind.Read ? OperationKind.Write : OperationKind.Read;
        Assert.Throws<ArgumentException>(()=>store.Save(Profile("wrong","other",other)));
        Assert.Throws<ArgumentException>(()=>new InMemoryProfileStore(operation,[Profile("wrong","other",other)]));
        store.Delete(profile.Id);
        Assert.Equal(original, Assert.Single(store.GetAll()));
    }
    public static void SystemProtection()
    {
        var store = new InMemoryProfileStore(OperationKind.Read);
        var system = Assert.Single(store.GetAll());
        Assert.Throws<InvalidOperationException>(()=>store.Save(system,true));
        Assert.Throws<InvalidOperationException>(()=>store.Rename(system.Id,"changed"));
        Assert.Throws<InvalidOperationException>(()=>store.Delete(system.Id));
        Assert.Throws<InvalidOperationException>(()=>store.Save(Profile("fake","Default"),true));
        Assert.Equal(system,Assert.Single(store.GetAll()));
    }
    public static void Conflicts()
    {
        var store = new InMemoryProfileStore(OperationKind.Read);
        store.Save(Profile("one","Alpha"));
        store.Save(Profile("two","Beta"));
        Assert.Throws<InvalidOperationException>(()=>store.Save(Profile("three","ALPHA")));
        Assert.Throws<InvalidOperationException>(()=>store.Rename("two","Alpha"));
        Assert.Throws<ArgumentException>(()=>store.Rename("one"," "));
        var replacement=store.Save(Profile("new-id","Alpha"),true);
        Assert.Equal("one",replacement.Id);
        Assert.Equal(3,store.GetAll().Count);
        Assert.Throws<KeyNotFoundException>(()=>store.Delete("missing"));
        Assert.Equal("Beta",store.GetAll().Single(p=>p.Id=="two").Name);
    }

    public static void Capture()
    {
        var profiles=new GWGUI.App.Services.Profiles.OperationProfileCollection();
        profiles.For(OperationKind.Read).Save(Profile("one","Read profile"));
        profiles.For(OperationKind.Write).Save(Profile("two","Write profile",OperationKind.Write));
        var captured=profiles.Capture();
        Assert.Equal(2,captured.Count);
        Assert.DoesNotContain(captured,p=>p.Id.StartsWith("default-"));
        Assert.Equal("synthetic",captured.Single(p=>p.Id=="one").Values["format"]);
        captured.Single(p=>p.Id=="one").Values["format"]="changed";
        Assert.Equal("synthetic",profiles.For(OperationKind.Read).GetAll().Single(p=>p.Id=="one").Values["format"]);
        profiles.Reset(captured);
        Assert.Equal("changed",profiles.For(OperationKind.Read).GetAll().Single(p=>p.Id=="one").Values["format"]);
        Assert.Single(profiles.For(OperationKind.Convert).GetAll());
    }
}
