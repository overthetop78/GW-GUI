using System.IO;
using System.Net.Http;

namespace GWGUI.Tests.Interface.EmulationViews;
[Collection("WPF")]
public class EmulationViewsTests(GWGUI.Tests.Application.TestInfrastructure.StaExecutionScenarios sta)
{
    [Fact] public Task DraftReloadDiscardAndSavedConfigurationAreIsolated() => sta.RunAsync(MachineConfigurationScenarios.DraftLifecycle);
    [Fact] public Task MachineSwitchKeepsSavedAndDraftChanges() => sta.RunAsync(MachineConfigurationScenarios.MachineSwitchKeepsSavedAndDraftChanges);
    [Fact] public Task VideoProfileChangesArePublishedAndPersisted() => sta.RunAsync(MachineConfigurationScenarios.VideoProfileChangesArePublishedAndPersisted);
    [Fact] public Task TabsSelectAndCloseOnlyTargetSession() => sta.RunAsync(MachineTabsScenarios.Tabs);
    [Fact] public Task CollapsedSelectorShowsLocalizedLabel() => sta.Run(MachineTabsScenarios.CollapsedSelectorShowsLocalizedLabel);
    [Theory] [InlineData(false)] [InlineData(true)] public Task ConfigurationDraftSaveAndRetry(bool failure) => sta.RunAsync(() => MachineConfigurationScenarios.ConfigurationEditing(failure));
    [Theory] [InlineData(false)] [InlineData(true)] public Task CommandsRespectPowerStateAndReportErrors(bool failure) => sta.Run(() => EmulationInteractionScenarios.Commands(failure));
    [Fact] public Task EmulationErrorsAppearOnceInConsoleAndDetachCleanly() => sta.Run(EmulationInteractionScenarios.ErrorLogAppearsOnceInConsoleAndDetaches);
    [Fact] public Task ConsoleLayoutIsIndependentForEachMainTab() => sta.Run(EmulationInteractionScenarios.ConsoleLayoutIsIndependentForEachMainTab);
    [Fact] public void CassetteCommandsReflectCapabilitiesAndTransportState() => CassetteTransportPresentationScenarios.CommandsReflectCapabilitiesAndTransportState();
    [Fact] public Task CassettePanelShowsEveryCommandBelowTheDevice() => sta.Run(CassetteTransportPresentationScenarios.PanelContainsASeparateRowWithEveryCommand);
    [Fact] public Task EmptyDeviceKeepsItsSizeAndCentersItsLed() => sta.Run(CassetteTransportPresentationScenarios.DeviceHeaderKeepsEjectSpaceAndCentersLedWhenEmpty);

    [Fact]
    public Task AcornJoystickVisualAndDisconnectedPortRemainEditable() => sta.Run(() =>
    {
        using var http = new HttpClient();
        var module = new GWGUI.Emulation.Acorn.Modules.AcornEmulationModule(Path.GetTempPath(), Path.GetTempPath(), http, Path.GetTempPath());
        var configuration = module.CreateConfiguration("BbcMicroModelB");
        var controller = new GWGUI.App.Controllers.Emulation.Input.EmulationInputSettingsController(module);
        var root = controller.CreateContent(GWGUI.Emulation.Enums.EmulationMachineTab.Controllers, configuration, []);
        var window = new System.Windows.Window { Content = root, Width = 1100, Height = 800, ShowInTaskbar = false };
        try
        {
            window.Show();
            var selectors = Descendants(root).OfType<System.Windows.Controls.ComboBox>()
                .Where(combo => combo.Items.OfType<GWGUI.App.Contracts.Emulation.Controllers.EmulationControllerCategoryChoiceView>().Any()).ToArray();
            Assert.Equal(2, selectors.Length);
            foreach (var selector in selectors)
            {
                var disconnected = selector.Items.OfType<GWGUI.App.Contracts.Emulation.Controllers.EmulationControllerCategoryChoiceView>()
                    .Single(choice => choice.Category == GWGUI.Emulation.Enums.EmulationControllerCategory.None);
                var joystick = selector.Items.OfType<GWGUI.App.Contracts.Emulation.Controllers.EmulationControllerCategoryChoiceView>()
                    .Single(choice => choice.Category == GWGUI.Emulation.Enums.EmulationControllerCategory.Joystick);
                selector.SelectedItem = disconnected;
                var without = module.DescribeInputSettings(controller.Apply(configuration));
                Assert.Contains(without.ControllerPorts, port => port.SelectedControllerId == "None");
                selector.SelectedItem = joystick;
            }
            var restored = module.DescribeInputSettings(controller.Apply(configuration));
            Assert.All(restored.ControllerPorts, port =>
            {
                Assert.Equal(GWGUI.Emulation.Constants.EmulationControllerVisualIds.CompetitionPro5000, port.VisualId);
                Assert.Equal(5, port.Bindings.Definitions.Count);
                Assert.All(port.Bindings.Values.Values, Assert.Empty);
            });
        }
        finally
        {
            GWGUI.Tests.Application.TestInfrastructure.TestResourceCleanup.ReleaseWindows(System.Windows.Application.Current);
        }
    });

    [Fact]
    public Task KeyboardCaptureWaitsForEveryReleaseAndAcceptsModifiersAlone() => sta.Run(() =>
    {
        var editor = new GWGUI.App.Views.Controls.Emulation.Input.InputBindingEditor();
        editor.SetRows([new GWGUI.Emulation.Contracts.InputBindingDefinition("Copy", "Emulation.Input.Actions", "")], null);
        var notifications = 0;
        editor.BindingsChanged += (_, _) => notifications++;
        try
        {
            Assert.True(editor.SelectAndStartCapture("Copy"));
            editor.CaptureKeyboardKeyDown(System.Windows.Input.Key.LeftCtrl, System.Windows.Input.ModifierKeys.Control);
            editor.CaptureKeyboardKeyDown(System.Windows.Input.Key.LeftShift, System.Windows.Input.ModifierKeys.Control | System.Windows.Input.ModifierKeys.Shift);
            editor.CaptureKeyboardKeyDown(System.Windows.Input.Key.A, System.Windows.Input.ModifierKeys.Control | System.Windows.Input.ModifierKeys.Shift);
            editor.CaptureKeyboardKeyUp(System.Windows.Input.Key.A);
            Assert.Equal(0, notifications);
            editor.CaptureKeyboardKeyUp(System.Windows.Input.Key.LeftCtrl);
            Assert.Equal(0, notifications);
            editor.CaptureKeyboardKeyUp(System.Windows.Input.Key.LeftShift);
            Assert.Equal("LeftCtrl+LeftShift+A", editor.Rows[0].Binding);
            Assert.Equal(1, notifications);
            Assert.True(editor.SelectAndStartCapture("Copy"));
            editor.CaptureKeyboardKeyDown(System.Windows.Input.Key.RightAlt, System.Windows.Input.ModifierKeys.Alt);
            Assert.Equal(1, notifications);
            editor.CaptureKeyboardKeyUp(System.Windows.Input.Key.RightAlt);
            Assert.Equal("RightAlt", editor.Rows[0].Binding);
            Assert.False(editor.HasErrors);
            Assert.Equal(2, notifications);
            Assert.True(editor.SelectAndStartCapture("Copy"));
            editor.CaptureKeyboardKeyDown(System.Windows.Input.Key.RightShift, System.Windows.Input.ModifierKeys.Shift);
            editor.CaptureKeyboardKeyDown(System.Windows.Input.Key.LeftAlt, System.Windows.Input.ModifierKeys.Shift | System.Windows.Input.ModifierKeys.Alt);
            editor.CaptureKeyboardKeyDown(System.Windows.Input.Key.RightCtrl, System.Windows.Input.ModifierKeys.Shift | System.Windows.Input.ModifierKeys.Alt | System.Windows.Input.ModifierKeys.Control);
            editor.CaptureKeyboardKeyDown(System.Windows.Input.Key.F8, System.Windows.Input.ModifierKeys.Shift | System.Windows.Input.ModifierKeys.Alt | System.Windows.Input.ModifierKeys.Control);
            editor.CaptureKeyboardKeyDown(System.Windows.Input.Key.F6, System.Windows.Input.ModifierKeys.Shift | System.Windows.Input.ModifierKeys.Alt | System.Windows.Input.ModifierKeys.Control);
            editor.CaptureKeyboardKeyUp(System.Windows.Input.Key.F6);
            editor.CaptureKeyboardKeyUp(System.Windows.Input.Key.RightCtrl);
            editor.CaptureKeyboardKeyUp(System.Windows.Input.Key.LeftAlt);
            editor.CaptureKeyboardKeyUp(System.Windows.Input.Key.F8);
            Assert.Equal(2, notifications);
            editor.CaptureKeyboardKeyUp(System.Windows.Input.Key.RightShift);
            Assert.Equal("RightCtrl+LeftAlt+RightShift+F6+F8", editor.Rows[0].Binding);
            Assert.Equal(3, notifications);
            Assert.True(editor.SelectAndStartCapture("Copy"));
            editor.CaptureKeyboardKeyDown(System.Windows.Input.Key.LeftAlt, System.Windows.Input.ModifierKeys.Alt);
            Assert.False(editor.CaptureKeyboardKeyDown(System.Windows.Input.Key.Tab, System.Windows.Input.ModifierKeys.Alt));
            Assert.Equal(3, notifications);
        }
        finally
        {
            editor.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.FrameworkElement.UnloadedEvent));
        }
    });

    [Fact]
    public Task ConflictingMouseBindingsStayEditableAndKeyboardChordsPersist() => sta.Run(() =>
    {
        using var http = new HttpClient();
        var module = new GWGUI.Emulation.Amstrad.Modules.AmstradEmulationModule(Path.GetTempPath(), Path.GetTempPath(), http, Path.GetTempPath());
        var configuration = module.CreateConfiguration("cpc-6128");
        var controller = new GWGUI.App.Controllers.Emulation.Input.EmulationInputSettingsController(module);
        var root = controller.CreateContent(GWGUI.Emulation.Enums.EmulationMachineTab.Mouse, configuration, []);
        var editor = Descendants(root).OfType<GWGUI.App.Views.Controls.Emulation.Input.InputBindingEditor>().Single();
        var original = module.DescribeInputSettings(configuration).Mouse!;
        editor.SetRows(original.Definitions, original.Definitions.ToDictionary(definition => definition.Id, _ => "Mouse:Right"));
        Assert.True(controller.HasErrors);
        Assert.Same(configuration, controller.Apply(configuration));
        editor.SetRows(original.Definitions, original.Values);
        Assert.False(controller.HasErrors);
        controller.Apply(configuration);
        root = controller.CreateContent(GWGUI.Emulation.Enums.EmulationMachineTab.Keyboard, configuration, []);
        editor = Descendants(root).OfType<GWGUI.App.Views.Controls.Emulation.Input.InputBindingEditor>().Single();
        var definitions = module.DescribeInputSettings(configuration).Keyboard!.Definitions;
        editor.SetRows(definitions, new Dictionary<string, string> { [definitions[0].Id] = "Ctrl+Shift+A" });
        var captured = controller.Apply(configuration);
        Assert.Equal("Ctrl+Shift+A", module.DescribeInputSettings(captured).Keyboard!.Values[definitions[0].Id]);
    });

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public Task AudioCardsKeepHostOnTheRightAndStackWhenNarrow(int nativeCount) => sta.RunAsync(async () =>
    {
        var host = new System.Windows.Controls.Border();
        var cards = Enumerable.Range(0, nativeCount).Select(_ => (System.Windows.FrameworkElement)new System.Windows.Controls.Border()).Append(host).ToArray();
        var page = GWGUI.App.Functions.Views.Emulation.Settings.EmulationSettingsLayout.CompactCards(cards, card => ReferenceEquals(card, host) ? 1 : 0);
        var window = new System.Windows.Window { Content = page, Width = 1200, Height = 400, ShowInTaskbar = false };
        try
        {
            window.Show();
            await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            var columns = page.Children.OfType<System.Windows.Controls.StackPanel>().ToArray();
            Assert.Equal(nativeCount, columns[0].Children.Count);
            Assert.Same(host, Assert.Single(columns[1].Children.Cast<System.Windows.FrameworkElement>()));
            window.Width = 600;
            await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            Assert.Equal(nativeCount + 1, columns[0].Children.Count);
            Assert.Empty(columns[1].Children);
        }
        finally { GWGUI.Tests.Application.TestInfrastructure.TestResourceCleanup.ReleaseWindows(System.Windows.Application.Current); }
    });

    [Theory]
    [InlineData(GWGUI.Emulation.Enums.EmulationMachineTab.Keyboard)]
    [InlineData(GWGUI.Emulation.Enums.EmulationMachineTab.Mouse)]
    [InlineData(GWGUI.Emulation.Enums.EmulationMachineTab.Controllers)]
    public Task EveryInputEditorRejectsAppShortcutsAndSystemExceptions(GWGUI.Emulation.Enums.EmulationMachineTab tab) => sta.Run(() =>
    {
        using var http = new HttpClient();
        var module = new GWGUI.Emulation.Amstrad.Modules.AmstradEmulationModule(Path.GetTempPath(), Path.GetTempPath(), http, Path.GetTempPath());
        var configuration = module.CreateConfiguration("cpc-6128");
        var controller = new GWGUI.App.Controllers.Emulation.Input.EmulationInputSettingsController(module, ["Alt+Enter"]);
        var root = controller.CreateContent(tab, configuration, []);
        var editors = Descendants(root).OfType<GWGUI.App.Views.Controls.Emulation.Input.InputBindingEditor>().ToArray();
        Assert.NotEmpty(editors);
        foreach (var editor in editors)
        {
            var rows = editor.Rows;
            if (rows.Count == 0) continue;
            var definitions = rows.Select(row => new GWGUI.Emulation.Contracts.InputBindingDefinition(row.Id, "Emulation.Input.Actions", "")).ToArray();
            foreach (var reserved in new[] { "Alt+Return", "Alt+Tab", "Ctrl+Shift+Escape", "Ctrl+Alt+Delete", "Win+A", "Alt+F4" })
            {
                editor.SetRows(definitions, new Dictionary<string, string> { [rows[0].Id] = tab == GWGUI.Emulation.Enums.EmulationMachineTab.Keyboard ? reserved : "Keyboard:" + reserved });
                Assert.True(editor.HasErrors);
                Assert.Same(configuration, controller.Apply(configuration));
            }
        }
    });

    [Fact]
    public Task AtariKeyboardChordSurvivesSettingsAndConfigurationSerialization() => sta.Run(() =>
    {
        using var http = new HttpClient();
        var module = new GWGUI.Emulation.Atari.Modules.AtariEmulationModule(Path.GetTempPath(), Path.GetTempPath(), http, Path.GetTempPath());
        var configuration = module.Machines.Select(machine => module.CreateConfiguration(machine.Id)).First(candidate => module.DescribeInputSettings(candidate).Keyboard?.Definitions.Count > 0);
        var controller = new GWGUI.App.Controllers.Emulation.Input.EmulationInputSettingsController(module);
        var root = controller.CreateContent(GWGUI.Emulation.Enums.EmulationMachineTab.Keyboard, configuration, []);
        var editor = Descendants(root).OfType<GWGUI.App.Views.Controls.Emulation.Input.InputBindingEditor>().Single();
        var definitions = module.DescribeInputSettings(configuration).Keyboard!.Definitions;
        editor.SetRows(definitions, new Dictionary<string, string> { [definitions[0].Id] = "Ctrl+Shift+A" });
        var captured = controller.Apply(configuration);
        var document = GWGUI.Emulation.Atari.Common.Machines.Common.Functions.ConfigurationStoreFunctions.ToDocument((GWGUI.Emulation.Atari.Common.Machines.Common.Contracts.MachineConfiguration)captured, Path.GetTempPath());
        var serialized = System.Text.Json.JsonSerializer.Serialize(document);
        var parsed = System.Text.Json.JsonSerializer.Deserialize<GWGUI.Emulation.Atari.Common.Machines.Common.Contracts.ConfigurationDocument>(serialized)!;
        var restored = GWGUI.Emulation.Atari.Common.Machines.Common.Functions.ConfigurationStoreFunctions.FromDocument(parsed, Path.GetTempPath());
        Assert.Equal("Ctrl+Shift+A", module.DescribeInputSettings(restored).Keyboard!.Values[definitions[0].Id]);
    });

    [Fact]
    public Task AcornFloppyDialogAppliesControllerAndBootToItsDrive() => sta.Run(() =>
    {
        using var http = new HttpClient();
        var module = new GWGUI.Emulation.Acorn.Modules.AcornEmulationModule(Path.GetTempPath(), Path.GetTempPath(), http, Path.GetTempPath());
        var configuration = module.CreateConfiguration("BbcMicroModelB");
        var storage = module.DescribeStorageSettings(configuration);
        var device = Assert.Single(storage.AvailableDevices);
        var drive = Assert.Single(storage.DeviceSettings!);
        var dialog = new GWGUI.App.Views.Dialogs.Emulation.Storage.FloppyDriveConfigurationDialog("D0:", "BBC Micro Model B", drive.Floppy!, device.FloppyOptions!, Guid.NewGuid(), module.Id, device.ConfigurationFields);
        try
        {
            dialog.Show();
            var selector = Descendants(dialog).OfType<System.Windows.Controls.ComboBox>().Single(combo => combo.Items.OfType<GWGUI.App.Contracts.Emulation.Settings.EmulationSettingsChoiceView>().Any());
            selector.SelectedItem = selector.Items.OfType<GWGUI.App.Contracts.Emulation.Settings.EmulationSettingsChoiceView>().Single(choice => choice.Choice.Id == GWGUI.Emulation.Acorn.Emulators.B2.Constants.OptionConstants.ChallengerController);
            var boot = Descendants(dialog).OfType<System.Windows.Controls.CheckBox>().Single();
            boot.IsChecked = false;
            var result = module.ApplyStorageSettings(configuration, storage with { DeviceSettings = [drive with { Floppy = dialog.Settings }] });
            var fields = Assert.Single(module.DescribeStorageSettings(result).AvailableDevices).ConfigurationFields!;
            Assert.Equal(GWGUI.Emulation.Acorn.Emulators.B2.Constants.OptionConstants.ChallengerController, fields.Single(field => field.Id == GWGUI.Emulation.Acorn.Emulators.B2.Constants.OptionConstants.DiscController).Value);
            Assert.Equal("false", fields.Single(field => field.Id == GWGUI.Emulation.Acorn.Emulators.B2.Constants.OptionConstants.AutoBoot).Value);
        }
        finally { GWGUI.Tests.Application.TestInfrastructure.TestResourceCleanup.ReleaseWindows(System.Windows.Application.Current); }
    });

    [Fact]
    public Task MixedCaptureAccumulatesKeysMouseAndControllerUntilAllAreReleased() => sta.Run(() =>
    {
        var editor = new GWGUI.App.Views.Controls.Emulation.Input.InputBindingEditor();
        editor.ConfigureCaptureSources(GWGUI.App.Enums.Input.InputCaptureSources.Keyboard | GWGUI.App.Enums.Input.InputCaptureSources.Mouse | GWGUI.App.Enums.Input.InputCaptureSources.Controller, true);
        editor.SetRows([new GWGUI.Emulation.Contracts.InputBindingDefinition("action", "Emulation.Input.Actions", "")], null);
        var changes = 0;
        editor.BindingsChanged += (_, _) => changes++;
        try
        {
            editor.SelectAndStartCapture("action");
            editor.CaptureKeyboardKeyDown(System.Windows.Input.Key.LeftCtrl, System.Windows.Input.ModifierKeys.Control);
            editor.CaptureKeyboardKeyDown(System.Windows.Input.Key.A, System.Windows.Input.ModifierKeys.Control);
            editor.CapturePhysicalSourceDown("Mouse:Left");
            editor.CapturePhysicalSourceDown("Mouse:Middle");
            editor.CapturePhysicalSourceDown("Controller:test:ButtonA");
            editor.CapturePhysicalSourceDown("Controller:test:ButtonX");
            editor.CaptureKeyboardKeyUp(System.Windows.Input.Key.A);
            editor.CaptureKeyboardKeyUp(System.Windows.Input.Key.LeftCtrl);
            editor.CapturePhysicalSourceUp("Mouse:Left");
            editor.CapturePhysicalSourceDown("Mouse:Left");
            editor.CapturePhysicalSourceUp("Controller:test:ButtonA");
            editor.CapturePhysicalSourceUp("Mouse:Middle");
            Assert.Equal(0, changes);
            editor.CapturePhysicalSourceUp("Controller:test:ButtonX");
            Assert.Equal(0, changes);
            editor.CapturePhysicalSourceUp("Mouse:Left");
            Assert.Equal(1, changes);
            Assert.Equal("Keyboard:LeftCtrl+A+Mouse:Left+Mouse:Middle+Controller:test:ButtonA+Controller:test:ButtonX", editor.Rows[0].Binding);
            Assert.False(editor.HasErrors);
        }
        finally { editor.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.FrameworkElement.UnloadedEvent)); }
    });

    [Fact]
    public Task ColecoCategoriesSelectNativeDevicesAndPreserveStoredIdentifiers() => sta.Run(() =>
    {
        using var http = new HttpClient();
        var module = new GWGUI.Emulation.Coleco.Modules.ColecoEmulationModule(Path.GetTempPath(), Path.GetTempPath(), http, Path.GetTempPath());
        var configuration = module.CreateConfiguration("ColecoVision");
        var controller = new GWGUI.App.Controllers.Emulation.Input.EmulationInputSettingsController(module);
        var root = controller.CreateContent(GWGUI.Emulation.Enums.EmulationMachineTab.Controllers, configuration, []);
        try
        {
            var selectors = Descendants(root).OfType<System.Windows.Controls.ComboBox>()
                .Where(combo => combo.Items.OfType<GWGUI.App.Contracts.Emulation.Controllers.EmulationControllerCategoryChoiceView>().Any()).ToArray();
            Assert.Equal(2, selectors.Length);
            foreach (var selector in selectors)
            {
                var joystick = selector.Items.OfType<GWGUI.App.Contracts.Emulation.Controllers.EmulationControllerCategoryChoiceView>()
                    .Single(choice => choice.Category == GWGUI.Emulation.Enums.EmulationControllerCategory.Joystick);
                selector.SelectedItem = joystick;
            }
            var visuals = Descendants(root).OfType<System.Windows.Controls.ComboBox>()
                .Where(combo => combo.Items.OfType<GWGUI.App.Contracts.Emulation.Controllers.EmulationControllerVisualChoiceView>()
                    .Any(choice => choice.ControllerChoice?.Id == "SuperAction")).ToArray();
            Assert.Equal(2, visuals.Length);
            foreach (var visual in visuals)
                visual.SelectedItem = visual.Items.OfType<GWGUI.App.Contracts.Emulation.Controllers.EmulationControllerVisualChoiceView>()
                    .Single(choice => choice.ControllerChoice?.Id == "SuperAction");
            var restored = module.DescribeInputSettings(controller.Apply(configuration));
            Assert.All(restored.ControllerPorts, port =>
            {
                Assert.Equal("SuperAction", port.SelectedControllerId);
                Assert.Equal(GWGUI.Emulation.Constants.EmulationControllerVisualIds.ColecoSuperActionController, port.VisualId);
                Assert.NotEmpty(port.Bindings.Definitions);
                Assert.All(port.Bindings.Values.Values, Assert.Empty);
                Assert.Contains("Purple", port.Bindings.Values.Keys);
                Assert.Contains("Blue", port.Bindings.Values.Keys);
                var choice = port.ControllerChoices.Single(item => item.Id == "SuperAction");
                if (port.Number == 1)
                    Assert.Equal("SpinnerLeft", choice.VisualCommandIds![GWGUI.Emulation.Enums.EmulationControllerVisualControl.StickLeft]);
                else
                    Assert.DoesNotContain(GWGUI.Emulation.Enums.EmulationControllerVisualControl.StickLeft, choice.VisualCommandIds!.Keys);
            });
            foreach (var selector in selectors)
                selector.SelectedItem = selector.Items.OfType<GWGUI.App.Contracts.Emulation.Controllers.EmulationControllerCategoryChoiceView>()
                    .Single(choice => choice.Category == GWGUI.Emulation.Enums.EmulationControllerCategory.Gamepad);
            var standard = module.DescribeInputSettings(controller.Apply(configuration));
            Assert.All(standard.ControllerPorts, port =>
            {
                Assert.Equal("Standard", port.SelectedControllerId);
                Assert.Equal(GWGUI.Emulation.Constants.EmulationControllerVisualIds.ColecoController, port.VisualId);
            });
            foreach (var selector in selectors)
                selector.SelectedItem = selector.Items.OfType<GWGUI.App.Contracts.Emulation.Controllers.EmulationControllerCategoryChoiceView>()
                    .Single(choice => choice.Category == GWGUI.Emulation.Enums.EmulationControllerCategory.RacingSimulation);
            var driving = module.DescribeInputSettings(controller.Apply(configuration));
            Assert.All(driving.ControllerPorts, port =>
            {
                Assert.Equal("Driving", port.SelectedControllerId);
                Assert.Equal(GWGUI.Emulation.Constants.EmulationControllerVisualIds.ColecoDrivingController, port.VisualId);
                Assert.All(port.Bindings.Values.Values, Assert.Empty);
                Assert.Contains("FireLeft", port.Bindings.Values.Keys);
                Assert.DoesNotContain("Keypad1", port.Bindings.Values.Keys);
                if (port.Number == 1)
                {
                    Assert.Contains("SpinnerLeft", port.Bindings.Values.Keys);
                    Assert.Contains("SpinnerRight", port.Bindings.Values.Keys);
                    Assert.Equal(3, port.Bindings.Values.Count);
                }
                else
                {
                    Assert.DoesNotContain("SpinnerLeft", port.Bindings.Values.Keys);
                    Assert.DoesNotContain("SpinnerRight", port.Bindings.Values.Keys);
                    Assert.Single(port.Bindings.Values);
                }
            });
        }
        finally
        {
            foreach (var editor in Descendants(root).OfType<GWGUI.App.Views.Controls.Emulation.Input.InputBindingEditor>())
                editor.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.FrameworkElement.UnloadedEvent));
        }
    });

    [Theory]
    [InlineData("Acorn")] [InlineData("Apple")] [InlineData("Bandai")] [InlineData("Epoch")]
    [InlineData("Fairchild")] [InlineData("Mattel")] [InlineData("Philips")] [InlineData("Sharp")]
    [InlineData("Sinclair")] [InlineData("Snk")] [InlineData("Thomson")] [InlineData("Watara")] [InlineData("Panasonic")]
    [InlineData("Coleco")] [InlineData("Wenquxing")]
    public void AddedFamiliesDescribeCategorizedNativeControllerChoices(string family)
    {
        using var http = new HttpClient();
        var context = new GWGUI.Emulation.Contracts.EmulationModuleContext(Path.GetTempPath(), Path.GetTempPath(), http);
        GWGUI.Emulation.Interfaces.IEmulationModuleFactory factory = family switch
        {
            "Acorn" => new GWGUI.Emulation.Acorn.Modules.AcornEmulationModuleFactory(),
            "Apple" => new GWGUI.Emulation.Apple.Modules.AppleEmulationModuleFactory(),
            "Bandai" => new GWGUI.Emulation.Bandai.Modules.BandaiEmulationModuleFactory(),
            "Epoch" => new GWGUI.Emulation.Epoch.Modules.EpochEmulationModuleFactory(),
            "Fairchild" => new GWGUI.Emulation.Fairchild.Modules.FairchildEmulationModuleFactory(),
            "Mattel" => new GWGUI.Emulation.Mattel.Modules.MattelEmulationModuleFactory(),
            "Philips" => new GWGUI.Emulation.Philips.Modules.PhilipsEmulationModuleFactory(),
            "Sharp" => new GWGUI.Emulation.Sharp.Modules.SharpEmulationModuleFactory(),
            "Sinclair" => new GWGUI.Emulation.Sinclair.Modules.SinclairEmulationModuleFactory(),
            "Snk" => new GWGUI.Emulation.Snk.Modules.SnkEmulationModuleFactory(),
            "Thomson" => new GWGUI.Emulation.Thomson.Modules.ThomsonEmulationModuleFactory(),
            "Watara" => new GWGUI.Emulation.Watara.Modules.WataraEmulationModuleFactory(),
            "Panasonic" => new GWGUI.Emulation.Panasonic.Modules.PanasonicEmulationModuleFactory(),
            "Coleco" => new GWGUI.Emulation.Coleco.Modules.ColecoEmulationModuleFactory(),
            "Wenquxing" => new GWGUI.Emulation.Wenquxing.Modules.WenquxingEmulationModuleFactory(),
            _ => throw new ArgumentOutOfRangeException(nameof(family))
        };
        var module = factory.Create(context);
        AssertCategorizedChoices(module);
    }

    [Theory]
    [InlineData("Atari")] [InlineData("Amstrad")] [InlineData("Commodore")] [InlineData("Microsoft")]
    [InlineData("Nec")] [InlineData("Nintendo")] [InlineData("Sega")] [InlineData("Sony")]
    public void ExistingFamiliesDescribeCategorizedNativeControllerChoices(string family)
    {
        using var http = new HttpClient();
        var root = Path.GetTempPath();
        GWGUI.Emulation.Interfaces.IEmulationModule module = family switch
        {
            "Atari" => new GWGUI.Emulation.Atari.Modules.AtariEmulationModule(root, root, http, root),
            "Amstrad" => new GWGUI.Emulation.Amstrad.Modules.AmstradEmulationModule(root, root, http, root),
            "Commodore" => new GWGUI.Emulation.Commodore.Modules.CommodoreEmulationModule(root, root, http, root),
            "Microsoft" => new GWGUI.Emulation.Microsoft.Modules.MicrosoftEmulationModule(root, root, http, root),
            "Nec" => new GWGUI.Emulation.Nec.Modules.NecEmulationModule(root, root, http, root),
            "Nintendo" => new GWGUI.Emulation.Nintendo.Modules.NintendoEmulationModule(root, root, http, root),
            "Sega" => new GWGUI.Emulation.Sega.Modules.SegaEmulationModule(root, root, http, root),
            "Sony" => new GWGUI.Emulation.Sony.Modules.SonyEmulationModule(root, root, http, root),
            _ => throw new ArgumentOutOfRangeException(nameof(family))
        };
        AssertCategorizedChoices(module);
    }

    private static void AssertCategorizedChoices(GWGUI.Emulation.Interfaces.IEmulationModule module)
    {
        var manager = Assert.IsAssignableFrom<GWGUI.Emulation.Interfaces.IEmulationInputSettingsManager>(module);
        foreach (var machine in module.Machines)
        {
            var settings = manager.DescribeInputSettings(module.CreateConfiguration(machine.Id));
            foreach (var port in settings.ControllerPorts)
            {
                Assert.Contains(port.ControllerChoices, choice => choice.Id == port.SelectedControllerId);
                Assert.All(port.ControllerChoices, choice =>
                {
                    Assert.NotEqual(GWGUI.Emulation.Enums.EmulationControllerCategory.Unspecified, choice.Category);
                    Assert.NotNull(choice.BindingDefinitions);
                    Assert.Equal(choice.Id == "None", choice.Category == GWGUI.Emulation.Enums.EmulationControllerCategory.None);
                });
            }
        }
    }

    private static IEnumerable<System.Windows.DependencyObject> Descendants(System.Windows.DependencyObject root)
    {
        yield return root;
        foreach (var child in System.Windows.LogicalTreeHelper.GetChildren(root).OfType<System.Windows.DependencyObject>())
            foreach (var descendant in Descendants(child)) yield return descendant;
    }
}
