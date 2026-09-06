using GWGUI.App.Options.Controllers;
using GWGUI.App.Views.Controls.Options;
using GWGUI.Domain.Settings;
using GWGUI.Domain.Settings.Engines;
namespace GWGUI.Tests.Interface.SettingsViews;
internal static class SettingsValidationScenarios
{
    public static async Task LogSize(string text, int expected)
    {
        var settings = new AppSettings(); settings.Logging.MaximumKilobytes = 12;
        var section = new OptionsLogsSection(); var saves = 0;
        var controller = new LoggingOptionsController(section, settings, () => false,
            () => { saves++; return Task.CompletedTask; }, key => key, error => throw error, "virtual-logs");
        section.Measure(new System.Windows.Size(1200, 800)); section.Arrange(new System.Windows.Rect(0, 0, 1200, 800));
        var input = GWGUI.Tests.Interface.Navigation.NavigationScenarios.Visuals<System.Windows.Controls.TextBox>(section).First();
        var row = Assert.IsType<GWGUI.App.ViewModels.Options.LogOptionRow>(input.DataContext);
        var binding = input.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty)!;
        Assert.Equal(System.Windows.Data.UpdateSourceTrigger.Explicit, binding.ParentBinding.UpdateSourceTrigger);
        input.SetCurrentValue(System.Windows.Controls.TextBox.TextProperty, text);
        Assert.Equal(12, row.Settings.MaximumKilobytes);
        await controller.CommitMaximumSizeAsync(input);
        Assert.Equal(expected, row.Settings.MaximumKilobytes); Assert.Equal(expected.ToString(), input.Text);
        Assert.Equal(1, saves);
        Assert.All(controller.Options.Where(other => !ReferenceEquals(other, row)), other => Assert.Equal(12, other.Settings.MaximumKilobytes));
        Assert.Equal("virtual-logs", section.DirectoryText.Text);
    }
    public static void Tags()
    {
        var settings = new AppSettings { Language = "de-DE", DefaultImagesFolder = "virtual-folder" };
        settings.Conversion.AddTags = false;
        var section = new OptionsGeneralSection(); var initializing = true; var saves = 0;
        TagOptionsController? controller = null;
        controller = new TagOptionsController(section, settings, () => initializing,
            () => { controller!.ApplyTo(settings); saves++; return Task.CompletedTask; },
            (key, args) => args.Length == 0 ? key : key + "|" + args[0]);
        initializing = false; Assert.Equal(0, saves);
        section.TagPattern.Text = "{FAMILY}-{FORMAT}-{NAME}";
        Assert.Null(section.TagPresets.SelectedItem);
        Assert.Equal("Options.TagPatternPreview|PC-720-Disquette.ima", section.TagPreview.Text);
        section.UseTags.IsChecked = true;
        Assert.True(settings.Conversion.AddTags); Assert.Equal("{FAMILY}-{FORMAT}-{NAME}", settings.Conversion.TagPattern);
        section.UseTags.IsChecked = false;
        Assert.False(settings.Conversion.AddTags); Assert.Equal("{FAMILY}-{FORMAT}-{NAME}", settings.Conversion.TagPattern);
        Assert.True(section.TagPattern.IsEnabled); Assert.Equal(2, saves);
        var preset = Assert.IsType<GWGUI.App.ViewModels.Options.TagPresetOption>(section.TagPresets.Items[0]);
        section.TagPresets.SelectedItem = preset;
        Assert.Equal(preset.Pattern, section.TagPattern.Text); Assert.Equal(preset.Pattern, settings.Conversion.TagPattern);
        Assert.Equal(3, saves); Assert.Equal("de-DE", settings.Language); Assert.Equal("virtual-folder", settings.DefaultImagesFolder);
    }
    public static void Initialization()
    {
        var settings = new AppSettings { Language = "de" };
        var section = new OptionsEnginesSection();
        var initializing = true;
        var saves = 0;
        var controller = new EngineOptionsController(section, settings.Engines, () => initializing, () => { saves++; return Task.CompletedTask; });
        section.PhysicalRead.SelectedIndex = 1;
        section.PhysicalRead.SelectedIndex = -1;
        Assert.Equal(0, saves);
        controller.ApplyTo(settings);
        Assert.Equal(OperationEngine.GreaseweazleHostTools, settings.Engines.PhysicalRead);
        Assert.Equal("de", settings.Language);
        initializing = false;
        section.PhysicalRead.SelectedIndex = 0;
        Assert.Equal(1, saves);
    }
}
