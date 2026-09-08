using GWGUI.App.Localization.Extensions;
using GWGUI.App.Services.Emulation;
using GWGUI.App.Views.Controls.Emulation.Options;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Interfaces;
using GWGUI.Tests.Application.TestInfrastructure;
using GWGUI.VideoPresentation.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
namespace GWGUI.Tests.Interface.EmulationViews;

internal static class MachineConfigurationScenarios
{
    internal sealed record Configuration(string ModuleId, Guid Id, string MachineId, string Value = "off") : IEmulationConfiguration;
    internal sealed class Module
    {
        public readonly string Id = "synthetic-ui-" + Guid.NewGuid().ToString("N");
        public List<IEmulationConfiguration> Saved = [];
        public Exception? SaveError;
        public List<IReadOnlyDictionary<string,string?>> Applied = [];
        public IEmulationModule Service;
        public Module()
        {
            Service = ControlledDependencies.Simulate<IEmulationModule>((method,args) => method.Name switch
            {
                "get_Id" => Id,
                "get_DisplayResourceKey" => "Emulation.Model",
                "get_Machines" => new EmulationMachineDefinition[] { new("a","Emulation.Model"),new("b","Emulation.Model") },
                "CreateConfiguration" => new Configuration(Id,Guid.NewGuid(),(string)args[0]!),
                "Describe" => Describe((Configuration)args[1]!),
                "LoadConfigurationsAsync" => ValueTask.FromResult<IReadOnlyList<IEmulationConfiguration>>(Saved.ToArray()),
                "SaveConfigurationAsync" => Save((IEmulationConfiguration)args[0]!),
                "ApplySettings" => Apply((Configuration)args[0]!, (IReadOnlyDictionary<string,string?>)args[1]!),
                _ => throw new InvalidOperationException(method.Name)
            });
        }
        private ValueTask Save(IEmulationConfiguration configuration)
        {
            if(SaveError is { } error) return ValueTask.FromException(error);
            Saved.RemoveAll(x => x.Id == configuration.Id); Saved.Add(configuration); return ValueTask.CompletedTask;
        }
        private Configuration Apply(Configuration configuration, IReadOnlyDictionary<string,string?> values)
        { Applied.Add(new Dictionary<string,string?>(values)); return configuration with { Value = values["toggle"] ?? "off" }; }
        private static EmulationMachineSettings Describe(Configuration configuration) => new(configuration.MachineId,
            new(new Dictionary<EmulationMachineTab,bool> { [EmulationMachineTab.General] = true,[EmulationMachineTab.Cpu] = true }),
            [new("general",EmulationMachineTab.General,"Emulation.Model",
                [new("toggle",EmulationMachineTab.General,"general","Emulation.Value.Enabled",EmulationSettingsEditor.Toggle,configuration.Value,EnabledValue:"on",DisabledValue:"off"),
                 new("hidden",EmulationMachineTab.General,"general","Emulation.Model",EmulationSettingsEditor.Text,"hidden",IsVisible:false)]),
             new("cpu",EmulationMachineTab.Cpu,"Emulation.Model",
                [new("locked",EmulationMachineTab.Cpu,"cpu","Emulation.Model",EmulationSettingsEditor.Text,"fixed",IsEnabled:false)])]);
        public void Cleanup() { EmulationConfigurationDraftStore.Remove(Id,"a"); EmulationConfigurationDraftStore.Remove(Id,"b"); }
    }
    internal static IEnumerable<T> Controls<T>(DependencyObject root) where T:DependencyObject
    {
        if(root is T control) yield return control;
        foreach(var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
            foreach(var descendant in Controls<T>(child)) yield return descendant;
    }
    public static async Task ConfigurationEditing(bool failed)
    {
        var module = new Module(); var files = new MemoryVideoProfileFiles(); var errors = new List<Exception>();
        var store = new VideoPresentationProfileStore("virtual-video",fileSystem:files,schedule:action => { action(); return Task.CompletedTask; });
        var view = new EmulationModuleSettingsSection(module.Service,store,errors.Add);
        try
        {
            var tabs = Assert.IsType<TabControl>(view.Content);
            Assert.Equal(new[] { EmulationMachineTab.General,EmulationMachineTab.Cpu },tabs.Items.Cast<TabItem>().Select(x => (EmulationMachineTab)x.Tag));
            Assert.DoesNotContain(Controls<TextBox>(view),x => x.Text == "hidden");
            Assert.False(Controls<TextBox>(view).Single(x => x.Text == "fixed").IsEnabled);
            var toggle = Assert.Single(Controls<CheckBox>(view)); Assert.False(toggle.IsChecked);
            toggle.IsChecked = true; await Dispatcher.Yield(DispatcherPriority.ContextIdle);
            Assert.Empty(module.Saved); Assert.Empty(files.Writes);
            Assert.True(EmulationConfigurationDraftStore.TryGet(module.Id,"a",out var draft));
            Assert.Equal("on",Assert.IsType<Configuration>(draft).Value);
            if(failed) module.SaveError = new IOException("synthetic configuration save");
            var create = Controls<Button>(view).Single(x => Equals(x.Content,LocExtension.Get("Common.Create")));
            create.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); await Dispatcher.Yield(DispatcherPriority.ContextIdle);
            if(failed)
            {
                Assert.Single(errors); Assert.Empty(module.Saved);
                Assert.True(EmulationConfigurationDraftStore.TryGet(module.Id,"a",out _));
                Assert.True(Assert.Single(Controls<CheckBox>(view)).IsChecked);
                module.SaveError = null; create.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                await Dispatcher.Yield(DispatcherPriority.ContextIdle);
            }
            var saved = Assert.IsType<Configuration>(Assert.Single(module.Saved)); Assert.Equal("on",saved.Value);
            Assert.False(EmulationConfigurationDraftStore.TryGet(module.Id,"a",out _)); Assert.NotEmpty(files.Writes);
            await view.EditConfigurationAsync(saved); Assert.True(Assert.Single(Controls<CheckBox>(view)).IsChecked);
            module.Saved.Clear();
            Assert.Single(Controls<CheckBox>(view)).IsChecked = false;
            await Dispatcher.Yield(DispatcherPriority.ContextIdle);
            var immediatelySaved = Assert.IsType<Configuration>(Assert.Single(module.Saved));
            Assert.Equal("off", immediatelySaved.Value);
            Assert.Equal("off", Assert.IsType<Configuration>(view.CurrentConfiguration).Value);
            Assert.Equal(failed?1:0,errors.Count);
        }
        finally { module.Cleanup(); }
    }

    public static async Task DraftLifecycle()
    {
        var module=new Module(); var other=new Module(); var errors=new List<Exception>();
        var files=new MemoryVideoProfileFiles(); var profiles=new VideoPresentationProfileStore("virtual",fileSystem:files,schedule:action=>{action();return Task.CompletedTask;});
        var view=new EmulationModuleSettingsSection(module.Service,profiles,errors.Add);
        var independent=new EmulationModuleSettingsSection(other.Service,profiles,errors.Add);
        try
        {
            Assert.Single(Controls<CheckBox>(view)).IsChecked=true; await Dispatcher.Yield(DispatcherPriority.ContextIdle);
            Assert.True(EmulationConfigurationDraftStore.TryGet(module.Id,"a",out var draft));
            Assert.False(Assert.Single(Controls<CheckBox>(independent)).IsChecked); Assert.Empty(other.Applied);
            await view.ReloadWhenOpenedAsync(); Assert.True(Assert.Single(Controls<CheckBox>(view)).IsChecked);
            Assert.Empty(module.Saved); Assert.Empty(files.Writes);
            await view.ReloadAfterConfigurationDeletedAsync(draft!.Id,"a");
            Assert.False(Assert.Single(Controls<CheckBox>(view)).IsChecked); Assert.False(EmulationConfigurationDraftStore.TryGet(module.Id,"a",out _));
            Assert.Single(Controls<CheckBox>(view)).IsChecked=true; await Dispatcher.Yield(DispatcherPriority.ContextIdle);
            var create=Controls<Button>(view).Single(button=>Equals(button.Content,LocExtension.Get("Common.Create")));
            create.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); await Dispatcher.Yield(DispatcherPriority.ContextIdle);
            var saved=Assert.IsType<Configuration>(Assert.Single(module.Saved)); Assert.Equal("on",saved.Value); Assert.NotEqual(draft.Id,saved.Id);
            await view.ReloadWhenOpenedAsync(); Assert.True(Assert.Single(Controls<CheckBox>(view)).IsChecked);
            Assert.Empty(errors); Assert.Empty(other.Saved);
        }
        finally { module.Cleanup(); other.Cleanup(); }
    }
}
