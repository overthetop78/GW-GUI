using GWGUI.App.Localization.Extensions;
using GWGUI.App.Localization.Sources;
using GWGUI.App.Functions.Localization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;
using GWGUI.App.Views.Controls.Read;
using GWGUI.App.Views.Controls.Write;
using GWGUI.App.Views.Controls.Conversion;
using GWGUI.Tests.Interface.Navigation;
namespace GWGUI.Tests.Interface.Localization;
internal static class LocalizedViewsScenarios
{
    public static void Layout(string language)
    {
        var source = LocalizationSource.Instance;
        var old = (source.Culture, source.UiCulture);
        try
        {
            var culture = UiLanguageResolver.GetCulture(language);
            source.SetCultures(culture, UiLanguageResolver.GetUiCulture(language));
            var read = new ReadTabSection(); var write = new WriteTabSection(); var convert = new ConversionTabSection();
            var longPath = "virtual/" + string.Concat(Enumerable.Repeat("long-directory/", 50)) + "synthetic.scp";
            read.FolderBlock.Input.Text = longPath; write.SourceBlock.Input.Text = longPath; convert.SourceBlock.Input.Text = longPath;
            read.CompletionBlock.Visibility = Visibility.Visible;
            var summary = read.CompletionBlock.SummaryTextBlock;
            summary.Text = string.Join(" ", Enumerable.Repeat(LocExtension.Get("Read.ScpSummaryTitle"), 500));
            foreach (var (section, command, path) in new (UserControl, Button, TextBox)[] {
                (read, read.ExecuteActionButton, read.FolderBlock.Input),
                (write, write.ExecuteActionButton, write.SourceBlock.Input),
                (convert, convert.ExecuteActionButton, convert.SourceBlock.Input) })
            {
                section.FlowDirection = culture.TextInfo.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
                section.Measure(new Size(1280, 720)); section.Arrange(new Rect(0, 0, 1280, 720)); section.UpdateLayout();
                WindowLayoutScenarios.AssertInside(command, section);
                Assert.InRange(path.ActualWidth, 1, 1280);
                Assert.Equal(longPath, path.Text);
                Assert.Equal(section.FlowDirection, command.FlowDirection);
                Assert.Equal(Visibility.Visible, command.Visibility); Assert.True(command.IsEnabled);
            }
            Assert.Equal(TextWrapping.Wrap, summary.TextWrapping);
            Assert.InRange(summary.ActualWidth, 1, 1280);
            Assert.True(summary.ActualHeight > summary.FontSize * 2);
            var scroll = NavigationScenarios.Visuals<ScrollViewer>(read).First();
            Assert.Equal(ScrollBarVisibility.Auto, scroll.VerticalScrollBarVisibility);
            Assert.True(scroll.ExtentHeight >= scroll.ViewportHeight);
        }
        finally { source.SetCultures(old.Culture, old.UiCulture); }
    }
    public static void Change(string language)
    {
        var source=LocalizationSource.Instance;
        var old=(source.Culture,source.UiCulture);
        try
        {
            var label=new TextBlock();
            label.SetBinding(TextBlock.TextProperty,LocExtension.CreateBinding("Read.ImageToCreate"));
            source.SetCultures(UiLanguageResolver.GetCulture(language),UiLanguageResolver.GetUiCulture(language));
            label.GetBindingExpression(TextBlock.TextProperty)!.UpdateTarget();
            Assert.Equal(BindingStatus.Active,label.GetBindingExpression(TextBlock.TextProperty)!.Status);
            Assert.Equal(LocExtension.Get("Read.ImageToCreate"),label.Text);
            foreach(var key in LocExtension.GetDefinedKeys(source.UiCulture))
            {
                var value=LocExtension.Get(key);
                Assert.False(string.IsNullOrWhiteSpace(value),"Empty resource: "+key+" in "+language);
                Assert.NotEqual("["+key+"]",value);
            }
            Assert.False(label.IsLoaded);
        }
        finally{source.SetCultures(old.Culture,old.UiCulture);}
    }
}
