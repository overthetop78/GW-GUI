using GWGUI.App.Options.Controllers;
using GWGUI.App.Options.States;
using GWGUI.App.Views.Controls.Options;
using GWGUI.Domain.Settings;
using GWGUI.Domain.Settings.Profiles;
using System.Windows;
using System.Windows.Controls;
namespace GWGUI.Tests.Interface.ProfileViews;
internal static class ProfileEditingScenarios
{
    public static void Name(string? value)
    {
        var accepted = 0; var warnings = 0;
        var window = new GWGUI.App.Views.Dialogs.Profiles.ProfileNameWindow(value, () => accepted++, () => warnings++);
        var root = Assert.IsType<Grid>(window.Content); root.Measure(new Size(440, 170)); root.Arrange(new Rect(0, 0, 440, 170));
        var save = Assert.Single(GWGUI.Tests.Interface.Navigation.NavigationScenarios.Visuals<Button>(root), button => button.IsDefault);
        save.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        var valid = !string.IsNullOrWhiteSpace(value);
        Assert.Equal(valid ? 1 : 0, accepted); Assert.Equal(valid ? 0 : 1, warnings);
        Assert.Equal(value?.Trim() ?? "", window.ProfileName);
        if (!valid) {
            Assert.IsType<TextBox>(window.FindName("NameText")).Text = " corrected ";
            save.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal(1, accepted); Assert.Equal(1, warnings); Assert.Equal("corrected", window.ProfileName);
        }
        Assert.False(window.IsLoaded);
    }
    public static void Save(int operation,int decision)
    {
        var kind = (GWGUI.Domain.Profiles.OperationKind)operation;
        var collection = new GWGUI.App.Services.Profiles.OperationProfileCollection();
        var requested = 0; var confirmations = 0;
        var business = GWGUI.Tests.Application.TestInfrastructure.ControlledDependencies.Simulate<GWGUI.App.Interfaces.Services.Dialogs.IBusinessDialogService>((method,_) => { Assert.Equal("PromptProfileName",method.Name); requested++; return decision==0?null:"sample"; });
        var dialogs = GWGUI.Tests.Application.TestInfrastructure.ControlledDependencies.Simulate<GWGUI.App.Interfaces.Services.Dialogs.IMessageDialogService>((method,args) =>
        { Assert.Equal("Show",method.Name); Assert.Equal("Profile.Replace",args[0]); confirmations++; return decision==2?GWGUI.App.Enums.Services.Dialogs.UserDialogResult.Yes:GWGUI.App.Enums.Services.Dialogs.UserDialogResult.No; });
        var controller = new GWGUI.App.Services.Profiles.OperationProfileController(collection,business,dialogs,(key,_)=>key);
        GWGUI.Domain.Profiles.OperationProfile Profile(string name,string value) => new("original",kind,name,new Dictionary<string,string>{{"option",value}},new HashSet<string>{"verify"});
        if(decision is 1 or 2) collection.For(kind).Save(Profile("sample","before"));
        var result = controller.Save(kind,name=>Profile(name,"after") with {Id="replacement"});
        Assert.Equal(1,requested); Assert.Equal(decision is 1 or 2?1:0,confirmations);
        if(decision is 2 or 3) { Assert.NotNull(result); Assert.Equal(decision==2?"original":"replacement",result.Id); Assert.Equal("after",result.Values["option"]); }
        else Assert.Null(result);
        if(decision==1) Assert.Equal("before",collection.For(kind).GetAll().Single(x=>!x.IsSystem).Values["option"]);
        var selector = new ComboBox(); controller.Refresh(selector,kind,result?.Id);
        Assert.Equal(result?.Id??GWGUI.Domain.Profiles.OperationProfile.Default(kind).Id,Assert.IsType<GWGUI.Domain.Profiles.OperationProfile>(selector.SelectedItem).Id);
        foreach(var other in Enum.GetValues<GWGUI.Domain.Profiles.OperationKind>().Where(x=>x!=kind)) Assert.Single(collection.For(other).GetAll());
        controller.Refresh(selector,kind,"deleted-id"); Assert.True(Assert.IsType<GWGUI.Domain.Profiles.OperationProfile>(selector.SelectedItem).IsSystem);
    }
    internal sealed class Context
    {
        internal AppSettings Settings{get;}=new(){Profiles=[
            new ProfileSettings{Id="one",Operation="Read",Name="first",Values=new(){{"format","kept"}}},
            new ProfileSettings{Id="two",Operation="Read",Name="second"},
            new ProfileSettings{Id="write",Operation="Write",Name="first"}]};
        internal OptionsProfilesSection View{get;}=new();
        internal ProfileOptionsState State;
        internal ProfileOptionsController Controller;
        internal int Saves,Warnings;
        internal Context(string? name=null,bool delete=false)
        {
            State=new(Settings.Profiles);
            Controller=new(new Window(),View,State,()=>{Saves++;return Task.CompletedTask;},(key,_)=>key,
                initial=>{Assert.Equal("first",initial);return name;},
                selected=>{Assert.Equal("first",selected);return delete;},()=>Warnings++);
            View.ReadProfiles.SelectedItem=State.Read[0];
        }
        internal void Click(int index)
        {
            var menu=View.ReadProfiles.ContextMenu;
            menu.PlacementTarget=View.ReadProfiles;
            var item=(MenuItem)menu.Items[index];
            item.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent,item));
        }
    }
    public static void Rename(string? name,int saves,int warnings)
    {
        var context=new Context(name);
        context.Click(0);
        Assert.Equal(saves,context.Saves);Assert.Equal(warnings,context.Warnings);
        Assert.Equal(saves==1?name:"first",context.State.Read.Single(x=>x.Id=="one").Name);
        Assert.Equal("first",Assert.Single(context.State.Write).Name);
    }
}
