using GWGUI.App.Views.Windows.Preferences;
using GWGUI.App.Views.Windows.EmulationPreferences;
using GWGUI.Domain.Settings;
using GWGUI.Domain.HostTools;
using GWGUI.Domain.Hardware;
using GWGUI.App.Views.Windows.EmulationModuleOptions;
using GWGUI.Tests.Interface.EmulationViews;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using GWGUI.Tests.Application.TestInfrastructure;
using GWGUI.VideoPresentation.Services;
namespace GWGUI.Tests.Interface.SettingsViews;
internal static class SettingsFailureScenarios
{
    public static async Task AutomaticSave()
    {
        var settings = new AppSettings { Theme = AppTheme.Light };
        var failure = new IOException("synthetic storage unavailable"); var fail = true;
        var saved = new List<AppTheme>(); var shown = new List<Exception>();
        var store = ControlledDependencies.Simulate<ISettingsStore>((method, args) => {
            Assert.Equal("SaveAsync", method.Name); saved.Add(Assert.IsType<AppSettings>(args[0]).Theme);
            return fail ? Task.FromException(failure) : Task.CompletedTask;
        });
        var window = new PreferencesWindow(settings, ControlledDependencies.Reject<IHardwareRegistry>(),
            ControlledDependencies.Reject<IGwInstallationManager>(), settingsStore: store, dataDirectory: "virtual-data",
            fileExists: _ => false, reportSaveError: shown.Add);
        Assert.Empty(saved); Assert.Empty(shown);
        window.GeneralSection.Themes.SelectedIndex = (int)AppTheme.Dark;
        await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.ContextIdle);
        Assert.Same(failure, Assert.Single(shown)); Assert.Equal(AppTheme.Dark, settings.Theme);
        Assert.Equal((int)AppTheme.Dark, window.GeneralSection.Themes.SelectedIndex);
        fail = false;
        window.GeneralSection.Themes.SelectedIndex = (int)AppTheme.Light;
        await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.ContextIdle);
        Assert.Equal(new[] { AppTheme.Dark, AppTheme.Light }, saved); Assert.Single(shown);
        Assert.Equal(AppTheme.Light, settings.Theme);
    }
    public static async Task Retry()
    {
        var settings = new AppSettings(); var fail = true; var snapshots = new List<string>();
        var expected = new IOException("synthetic settings failure");
        var store = ControlledDependencies.Simulate<ISettingsStore>((method,args) =>
        {
            Assert.Equal("SaveAsync",method.Name); var value = Assert.IsType<AppSettings>(args[0]);
            snapshots.Add(value.DefaultImagesFolder);
            return fail ? Task.FromException(expected) : Task.CompletedTask;
        });
        var view = new PreferencesWindow(settings,ControlledDependencies.Reject<IHardwareRegistry>(),
            ControlledDependencies.Reject<IGwInstallationManager>(),settingsStore:store, dataDirectory:"virtual-data", fileExists: _ => false);
        view.GeneralSection.ImagesFolder.Text = "  virtual-first  ";
        Assert.Same(expected,await Assert.ThrowsAsync<IOException>(view.PersistSettingsAsync));
        Assert.Equal("  virtual-first  ",view.GeneralSection.ImagesFolder.Text);
        Assert.Equal("virtual-first",settings.DefaultImagesFolder);
        view.GeneralSection.ImagesFolder.Text = "virtual-second"; fail = false;
        await view.PersistSettingsAsync();
        Assert.Equal(new[] {"virtual-first","virtual-second"},snapshots);
        Assert.Equal("virtual-second",settings.DefaultImagesFolder);
    }

    public static async Task EmulationAutomaticSave()
    {
        var settings = new AppSettings();
        var failure = new IOException("synthetic emulation settings failure");
        var fail = true;
        var saves = 0;
        var shown = new List<Exception>();
        var store = ControlledDependencies.Simulate<ISettingsStore>((method, args) =>
        {
            Assert.Equal("SaveAsync", method.Name);
            Assert.Same(settings, Assert.IsType<AppSettings>(args[0]));
            saves++;
            return fail ? Task.FromException(failure) : Task.CompletedTask;
        });
        var window = new EmulationPreferencesWindow(settings, settingsStore: store,
            dataDirectory: "virtual-data", reportSaveError: shown.Add);

        await window.SaveFromEditorAsync();
        Assert.Same(failure, Assert.Single(shown));
        fail = false;
        await window.SaveFromEditorAsync();
        Assert.Equal(2, saves);
        Assert.Single(shown);
    }

    public static async Task ModuleWindowSaveFailure()
    {
        var module = new MachineConfigurationScenarios.Module
        {
            SaveError = new IOException("synthetic module settings failure")
        };
        var shown = new List<Exception>();
        var profileFiles = new MemoryVideoProfileFiles();
        var profiles = new VideoPresentationProfileStore("virtual-module-video", fileSystem: profileFiles,
            schedule: action => { action(); return Task.CompletedTask; });
        var window = new EmulationModuleOptionsWindow(module.Service, showError: shown.Add, profiles: profiles);
        try
        {
            var create = MachineConfigurationScenarios.Controls<Button>(window)
                .Single(button => Equals(button.Content,
                    GWGUI.App.Localization.Extensions.LocExtension.Get("Common.Create")));
            create.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            await Dispatcher.Yield(DispatcherPriority.ContextIdle);
            Assert.Same(module.SaveError, Assert.Single(shown));
            Assert.Empty(module.Saved);
        }
        finally
        {
            window.Close();
            module.Cleanup();
        }
    }
}
