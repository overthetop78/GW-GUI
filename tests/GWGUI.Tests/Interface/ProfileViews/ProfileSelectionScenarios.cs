using GWGUI.App.Views.Controls.Options;
using GWGUI.App.ViewModels.Operations.Options;
using GWGUI.Domain.Profiles;
using System.Windows;
using System.Windows.Controls;
namespace GWGUI.Tests.Interface.ProfileViews;
internal static class ProfileSelectionScenarios
{
    public static void SwitchAndReset(int operation)
    {
        switch (operation)
        {
            case 0:
                using (var read = new GWGUI.Tests.Interface.ReadViews.ReadOperationScenarios.Context())
                    Verify(read.View.ProfileBlock, OperationKind.Read, read.Controller.ProfileChanged, read.Controller.ResetProfile,
                        read.Model.Read.Tracks, value => read.Model.Read.ExpertArguments = value, () => read.Model.Read.ExpertArguments);
                break;
            case 1:
                var write = new GWGUI.Tests.Interface.WriteViews.WriteOperationScenarios.Context();
                Verify(write.View.ProfileBlock, OperationKind.Write, write.Controller.ProfileChanged, write.Controller.ResetProfile,
                    write.Model.Write.Tracks, value => write.Model.Write.ExpertArguments = value, () => write.Model.Write.ExpertArguments);
                break;
            default:
                var convert = new GWGUI.Tests.Interface.ConversionViews.ConversionOperationScenarios.Context();
                Verify(convert.View.ProfileBlock, OperationKind.Convert, convert.Controller.ProfileChanged, convert.Controller.ResetProfile,
                    convert.Model.Conversion.Tracks, value => convert.Model.Conversion.ExpertArguments = value, () => convert.Model.Conversion.ExpertArguments);
                break;
        }
    }

    private static void Verify(ProfileSection section, OperationKind kind, Action change, Action reset,
        ValueOptionViewModel tracks, Action<string> editExpert, Func<string> expert)
    {
        var first = new OperationProfile("first", kind, "first", new Dictionary<string, string> { ["tracks"] = "c=2-4:h=1", ["expert"] = "--raw" }, new HashSet<string> { "tracks" });
        var second = new OperationProfile("second", kind, "second", new Dictionary<string, string> { ["tracks"] = "c=1-1:h=0" }, new HashSet<string> { "tracks" });
        section.ProfileCombo.SelectionChanged += (_, _) => change();
        section.ResetButton.Click += (_, _) => reset();
        section.ProfileCombo.ItemsSource = new[] { OperationProfile.Default(kind), first, second };
        section.ProfileCombo.SelectedItem = first;
        Assert.True(tracks.Enabled); Assert.Equal("c=2-4:h=1", tracks.Value); Assert.Equal("--raw", expert());
        tracks.Value = "c=9-9:h=0"; editExpert("edited");
        section.ResetButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.Same(first, section.ProfileCombo.SelectedItem); Assert.Equal("c=2-4:h=1", tracks.Value); Assert.Equal("--raw", expert());
        section.ProfileCombo.SelectedItem = second;
        Assert.Equal("c=1-1:h=0", tracks.Value); Assert.Equal("", expert());
        Assert.Equal("c=2-4:h=1", first.Values["tracks"]);
        section.ProfileCombo.SelectedIndex = 0;
        Assert.False(tracks.Enabled); Assert.Equal("", expert());
        tracks.Enabled = true; editExpert("changed");
        section.ResetButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.True(Assert.IsType<OperationProfile>(section.ProfileCombo.SelectedItem).IsSystem);
        Assert.False(tracks.Enabled); Assert.Equal("", expert());
    }
    public static void Apply()
    {
        var context=new ProfileEditingScenarios.Context("changed");
        Assert.Same(context.State.Read,context.View.ReadProfiles.ItemsSource);
        Assert.Same(context.State.Write,context.View.WriteProfiles.ItemsSource);
        context.Click(0);
        context.Controller.ApplyTo(context.Settings);
        Assert.Equal("changed",context.Settings.Profiles.Single(p=>p.Id=="one").Name);
        Assert.Equal("kept",context.Settings.Profiles.Single(p=>p.Id=="one").Values["format"]);
        Assert.Equal("first",context.Settings.Profiles.Single(p=>p.Id=="write").Name);
    }
}
