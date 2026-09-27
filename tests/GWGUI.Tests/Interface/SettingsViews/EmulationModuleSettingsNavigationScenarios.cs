using System.Windows;
using System.Windows.Controls;
using System.Net.Http;
using GWGUI.App.Contracts.Emulation.Machine;
using GWGUI.App.Contracts.Views.Emulation.Settings;
using GWGUI.App.Controllers.Emulation.Options;
using GWGUI.App.Functions.Views.Emulation.Settings;
using GWGUI.App.Views.Controls.Emulation.Options;
using GWGUI.App.Views.Windows.EmulationModuleOptions;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Amiga.Modules;
using GWGUI.Tests.Application.TestInfrastructure;
using GWGUI.Tests.Interface.EmulationViews;

namespace GWGUI.Tests.Interface.SettingsViews;

internal static class EmulationModuleSettingsNavigationScenarios
{
    internal static void VideoFieldsKeepTheirHelpContent()
    {
        var control = new ComboBox();
        var grid = EmulationSettingsLayout.VideoSettingsFields(new EmulationVideoSettingsField(
            "Resolution", control, Explanation: "Short help", DetailedExplanation: "Detailed help"));

        var label = Assert.Single(MachineConfigurationScenarios.Controls<EmulationSettingsFieldLabel>(grid));
        Assert.Equal(string.Empty, label.Text);
        Assert.Contains(label.Inlines, inline => inline is System.Windows.Documents.InlineUIContainer);
    }

    internal static async Task VerticalMachinesPreserveConfigurationStateAndTabs()
    {
        var module = new MachineConfigurationScenarios.Module();
        module.Saved.Add(module.Service.CreateConfiguration("a"));
        var section = new EmulationModuleSettingsSection(module.Service);
        try
        {
            await section.ReloadWhenOpenedAsync();
            var machines = Assert.Single(MachineConfigurationScenarios.Controls<ListBox>(section));
            var choices = machines.Items.Cast<EmulationMachineChoice>().ToArray();
            Assert.Equal(2, choices.Length);
            Assert.True(choices.Single(choice => choice.Definition.Id == "a").HasSavedConfiguration);
            Assert.False(choices.Single(choice => choice.Definition.Id == "b").HasSavedConfiguration);
            Assert.Contains(machines.ItemContainerStyle.Triggers.OfType<DataTrigger>(), trigger =>
                Equals(trigger.Value, true));
            Assert.Equal(2, Assert.Single(MachineConfigurationScenarios.Controls<TabControl>(section)).Items.Count);
            machines.SelectedItem = choices.Single(choice => choice.Definition.Id == "b");
            await System.Windows.Threading.Dispatcher.Yield(
                System.Windows.Threading.DispatcherPriority.ContextIdle);
            Assert.Equal("b", section.CurrentConfiguration.MachineId);
        }
        finally { module.Cleanup(); }
    }

    internal static async Task UnsavedMachineChoosesExactlyOneEmulator()
    {
        IEmulationConfiguration configuration = new MachineConfigurationScenarios.Configuration(
            "synthetic", Guid.NewGuid(), "machine-a", "emulator-a");
        var useCount = 0;
        var saved = false;
        var manager = ControlledDependencies.Simulate<IEmulationEmulatorManager>((method, arguments) => method.Name switch
        {
            "GetEmulatorInstallationAsync" => ValueTask.FromResult(
                new EmulationEmulatorInstallation(
                    ((MachineConfigurationScenarios.Configuration)configuration).Value, "1.0.0")),
            "GetEmulatorInstallationsAsync" => ValueTask.FromResult<IReadOnlyList<EmulationEmulatorInstallation>>(
                [new("emulator-a", "1.0.0"), new("emulator-b", null)]),
            "UseEmulatorAsync" => UseEmulator((string)arguments[1]!),
            _ => throw new InvalidOperationException(method.Name)
        });
        var controller = new EmulationEmulatorManagementController(manager,
            () => configuration,
            value => configuration = value,
            () => saved);
        try
        {
            controller.ConfigurationChanged += (_, _) => useCount++;
            var panel = Assert.IsType<EmulationCoreManagementPanel>(controller.CreateView());
            await controller.RefreshAsync();
            Assert.Equal(["emulator-a", "emulator-b"], panel.Emulators.Items
                .Cast<EmulationEmulatorInstallation>().Select(item => item.EmulatorId));
            Assert.Equal("emulator-a", panel.Emulators.SelectedValue);
            Assert.Equal(0, useCount);
            Assert.True(panel.Emulators.IsEnabled);
            Assert.Equal(Visibility.Visible, panel.Installed.Visibility);
            Assert.Equal(Visibility.Visible, panel.Search.Visibility);
            Assert.Equal(Visibility.Collapsed, panel.Versions.Visibility);
            Assert.Equal(Visibility.Collapsed, panel.Download.Visibility);

            panel.Emulators.SelectedValue = "emulator-b";
            await System.Windows.Threading.Dispatcher.Yield(
                System.Windows.Threading.DispatcherPriority.ContextIdle);
            Assert.Equal("emulator-b", ((MachineConfigurationScenarios.Configuration)configuration).Value);
            Assert.Equal(1, useCount);
            Assert.Equal("emulator-b", panel.Emulators.SelectedValue);

            saved = true;
            await controller.RefreshAsync();
            Assert.False(panel.Emulators.IsEnabled);
        }
        finally
        {
            await controller.DisposeAsync();
        }

        ValueTask<IEmulationConfiguration> UseEmulator(string selected) =>
            ValueTask.FromResult<IEmulationConfiguration>(
                ((MachineConfigurationScenarios.Configuration)configuration) with { Value = selected });
    }

    internal static async Task EmulatorVersionsAreSelectedAndInstalledExplicitly()
    {
        IEmulationConfiguration configuration = new MachineConfigurationScenarios.Configuration(
            "synthetic", Guid.NewGuid(), "machine-a", "emulator-a");
        var installedVersion = "reference";
        string? installedRelease = null;
        var installation = () => new EmulationEmulatorInstallation("emulator-a", installedVersion);
        var manager = ControlledDependencies.Simulate<IEmulationEmulatorManager>((method, arguments) =>
            method.Name switch
            {
                "GetEmulatorInstallationAsync" => ValueTask.FromResult(installation()),
                "GetEmulatorInstallationsAsync" =>
                    ValueTask.FromResult<IReadOnlyList<EmulationEmulatorInstallation>>([installation()]),
                "FindEmulatorReleasesAsync" =>
                    ValueTask.FromResult<IReadOnlyList<EmulationEmulatorRelease>>(
                    [
                        new("latest", "Latest", "latest"),
                        new("reference", "Reference", "reference", IsRequired: true)
                    ]),
                "InstallEmulatorAsync" => Install((EmulationEmulatorRelease)arguments[1]!),
                _ => throw new InvalidOperationException(method.Name)
            });
        var controller = new EmulationEmulatorManagementController(manager,
            () => configuration, value => configuration = value, () => false);
        try
        {
            var panel = Assert.IsType<EmulationCoreManagementPanel>(controller.CreateView());
            await controller.RefreshAsync();

            panel.Search.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            await System.Windows.Threading.Dispatcher.Yield(
                System.Windows.Threading.DispatcherPriority.ContextIdle);

            var releases = panel.Versions.Items.Cast<EmulationEmulatorRelease>().ToArray();
            Assert.Equal(2, releases.Length);
            var reference = Assert.Single(releases, release => release.Id == "reference");
            Assert.True(reference.IsRequired);
            Assert.Contains("Reference", reference.DisplayName, StringComparison.Ordinal);
            Assert.NotEqual("Reference", reference.DisplayName);
            Assert.Equal("reference", Assert.IsType<EmulationEmulatorRelease>(
                panel.Versions.SelectedItem).Id);
            Assert.Equal(Visibility.Visible, panel.Download.Visibility);

            panel.Versions.SelectedItem = releases.Single(release => release.Id == "latest");
            panel.Download.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            await System.Windows.Threading.Dispatcher.Yield(
                System.Windows.Threading.DispatcherPriority.ContextIdle);

            Assert.Equal("latest", installedRelease);
        }
        finally
        {
            await controller.DisposeAsync();
        }

        ValueTask<string> Install(EmulationEmulatorRelease release)
        {
            installedRelease = release.Id;
            installedVersion = release.Version;
            return ValueTask.FromResult("installed-core.dll");
        }
    }

    internal static async Task EmulatorControllerDisposalCancelsAndDetaches()
    {
        IEmulationConfiguration configuration = new MachineConfigurationScenarios.Configuration(
            "synthetic", Guid.NewGuid(), "machine-a", "emulator-a");
        var operationStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var operationCancelled = false;
        var useCount = 0;
        var manager = ControlledDependencies.Simulate<IEmulationEmulatorManager>((method, arguments) => method.Name switch
        {
            "GetEmulatorInstallationAsync" => ValueTask.FromResult(
                new EmulationEmulatorInstallation(
                    ((MachineConfigurationScenarios.Configuration)configuration).Value, "1.0.0")),
            "GetEmulatorInstallationsAsync" => ValueTask.FromResult<IReadOnlyList<EmulationEmulatorInstallation>>(
                [new("emulator-a", "1.0.0"), new("emulator-b", null)]),
            "UseEmulatorAsync" => UseEmulatorAsync((CancellationToken)arguments[2]!),
            _ => throw new InvalidOperationException(method.Name)
        });
        var controller = new EmulationEmulatorManagementController(manager,
            () => configuration,
            value => configuration = value,
            () => false);
        var panel = Assert.IsType<EmulationCoreManagementPanel>(controller.CreateView());
        try
        {
            controller.ConfigurationChanged += (_, _) => useCount++;
            await controller.RefreshAsync();
            panel.Emulators.SelectedValue = "emulator-b";
            await operationStarted.Task.WaitAsync(TimeSpan.FromSeconds(1));

            await controller.DisposeAsync();

            Assert.True(operationCancelled);
            Assert.Equal(0, useCount);
            Assert.Equal("emulator-a", ((MachineConfigurationScenarios.Configuration)configuration).Value);
            panel.Emulators.SelectedValue = "emulator-a";
            panel.Emulators.SelectedValue = "emulator-b";
            await System.Windows.Threading.Dispatcher.Yield(
                System.Windows.Threading.DispatcherPriority.ContextIdle);
            Assert.Equal(0, useCount);
        }
        finally
        {
            await controller.DisposeAsync();
        }

        async ValueTask<IEmulationConfiguration> UseEmulatorAsync(CancellationToken cancellationToken)
        {
            operationStarted.TrySetResult();
            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }
            finally
            {
                operationCancelled = cancellationToken.IsCancellationRequested;
            }
            return configuration;
        }
    }

    internal static async Task EmulatorDescriptionUsesModuleLocalization()
    {
        IEmulationConfiguration configuration = new MachineConfigurationScenarios.Configuration(
            "synthetic", Guid.NewGuid(), "machine-a", "emulator-a");
        var definition = new EmulationEmulatorDefinition(
            "emulator-a", "Emulator A", "Emulation.Emulator.emulator-a.Description",
            new HashSet<string>(StringComparer.Ordinal) { "machine-a" });
        var installation = new EmulationEmulatorInstallation(definition, "1.0.0");
        var manager = ControlledDependencies.Simulate<IEmulationEmulatorManager>((method, _) => method.Name switch
        {
            "GetEmulatorInstallationAsync" => ValueTask.FromResult(installation),
            "GetEmulatorInstallationsAsync" =>
                ValueTask.FromResult<IReadOnlyList<EmulationEmulatorInstallation>>([installation]),
            _ => throw new InvalidOperationException(method.Name)
        });
        var controller = new EmulationEmulatorManagementController(manager,
            () => configuration, value => configuration = value, () => false,
            new EmulatorLocalization(definition.DescriptionResourceKey, "Description traduite"));
        try
        {
            var panel = Assert.IsType<EmulationCoreManagementPanel>(controller.CreateView());
            await controller.RefreshAsync();
            Assert.Equal("Emulator A", Assert.IsType<EmulationEmulatorInstallation>(
                panel.Emulators.SelectedItem).DisplayName);
            Assert.Equal("Description traduite", panel.Description.Text);
        }
        finally
        {
            await controller.DisposeAsync();
        }
    }

    internal static async Task ModuleWindowReopensWithANewVisualTree()
    {
        var module = new MachineConfigurationScenarios.Module();
        EmulationModuleOptionsWindow? first = null;
        EmulationModuleOptionsWindow? second = null;
        try
        {
            first = new EmulationModuleOptionsWindow(module.Service);
            var firstSection = Assert.IsType<EmulationModuleSettingsSection>(first.ModuleContent.Content);
            first.Show();
            first.Close();
            await System.Windows.Threading.Dispatcher.Yield(
                System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            Assert.Null(first.ModuleContent.Content);
            Assert.False(first.IsVisible);

            second = new EmulationModuleOptionsWindow(module.Service);
            var secondSection = Assert.IsType<EmulationModuleSettingsSection>(second.ModuleContent.Content);
            Assert.NotSame(firstSection, secondSection);
            second.Show();
            second.Close();
            await System.Windows.Threading.Dispatcher.Yield(
                System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            Assert.Null(second.ModuleContent.Content);
            Assert.False(second.IsVisible);
        }
        finally
        {
            if (first?.IsVisible == true) first.Close();
            if (second?.IsVisible == true) second.Close();
            await System.Windows.Threading.Dispatcher.Yield(
                System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            module.Cleanup();
        }
    }

    internal static async Task AmigaModuleWindowBuildsItsVisualTree()
    {
        var root = Path.Combine(Path.GetTempPath(), $"gwgui-amiga-settings-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        using var httpClient = new HttpClient();
        Window? owner = null;
        EmulationModuleOptionsWindow? window = null;
        try
        {
            var module = new AmigaEmulationModule(
                Path.Combine(root, "Configurations"), root, httpClient, Path.Combine(root, "Core"));
            var configuration = module.ChangeMachine(module.CreateConfiguration("A500"), "A600");
            await module.SaveConfigurationAsync(configuration);
            owner = new Window { Width = 800, Height = 600 };
            owner.Show();
            window = new EmulationModuleOptionsWindow(module) { Owner = owner };
            var renderedVisibleTree = false;
            window.ContentRendered += (_, _) =>
            {
                var section = Assert.IsType<EmulationModuleSettingsSection>(window.ModuleContent.Content);
                var machines = Assert.Single(MachineConfigurationScenarios.Controls<ListBox>(section),
                    control => control.Items.Count == module.Machines.Count && control.IsVisible);
                var tabs = Assert.Single(MachineConfigurationScenarios.Controls<TabControl>(section),
                    control => control.Items.Count == 10 && control.IsVisible);
                renderedVisibleTree = window.IsVisible && section.IsVisible && machines.IsVisible && tabs.IsVisible
                    && section.ActualWidth > 0 && section.ActualHeight > 0
                    && machines.ActualWidth > 0 && tabs.ActualWidth > 0;
                window.Dispatcher.BeginInvoke(window.Close);
            };

            window.ShowDialog();
            await System.Windows.Threading.Dispatcher.Yield(
                System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            Assert.True(renderedVisibleTree);
        }
        finally
        {
            if (window?.IsVisible == true)
            {
                window.Close();
                await System.Windows.Threading.Dispatcher.Yield(
                    System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }
            if (window is not null)
            {
                Assert.False(window.IsVisible);
                Assert.Null(window.ModuleContent.Content);
            }
            if (owner?.IsVisible == true) owner.Close();
            await System.Windows.Threading.Dispatcher.Yield(
                System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            Directory.Delete(root, true);
        }
    }

    internal static void ModuleWindowUsesTheGenericSectionAndDynamicTitle()
    {
        var module = new MachineConfigurationScenarios.Module();
        var window = new EmulationModuleOptionsWindow(module.Service);
        try
        {
            Assert.IsType<EmulationModuleSettingsSection>(window.ModuleContent.Content);
            Assert.False(string.IsNullOrWhiteSpace(window.Title));
        }
        finally
        {
            window.Close();
            module.Cleanup();
        }
    }

    private sealed class EmulatorLocalization(string key, string translation)
        : IEmulationModuleLocalization
    {
        public bool TryGetString(string resourceKey, System.Globalization.CultureInfo culture,
            out string value)
        {
            _ = culture;
            value = string.Equals(resourceKey, key, StringComparison.Ordinal)
                ? translation
                : string.Empty;
            return value.Length > 0;
        }
    }
}
