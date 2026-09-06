namespace GWGUI.Tests.Interface.ProfileViews;
internal static class ProfileDeletionScenarios
{
    public static void Delete(bool accepted)
    {
        var context=new ProfileEditingScenarios.Context(delete:accepted);
        context.Click(1);
        Assert.Equal(accepted?1:0,context.Saves);
        Assert.Equal(accepted?1:2,context.State.Read.Count);
        Assert.Single(context.State.Write);
        if (accepted) {
            Assert.Null(context.View.ReadProfiles.SelectedItem);
            context.View.ReadProfiles.SelectedIndex = 0;
            Assert.Equal("two", Assert.IsType<GWGUI.App.ViewModels.Options.ProfileOptionRow>(context.View.ReadProfiles.SelectedItem).Id);
        } else Assert.Equal("one", Assert.IsType<GWGUI.App.ViewModels.Options.ProfileOptionRow>(context.View.ReadProfiles.SelectedItem).Id);
        context.Controller.ApplyTo(context.Settings);
        Assert.Equal(!accepted,context.Settings.Profiles.Any(x=>x.Id=="one"));
    }
}
