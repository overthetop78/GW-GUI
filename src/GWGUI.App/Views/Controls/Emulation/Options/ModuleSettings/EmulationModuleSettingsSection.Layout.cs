using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Constants.Emulation.Errors;
using GWGUI.App.Constants.Localization;
using GWGUI.App.Contracts.Emulation.Settings;
using GWGUI.App.Contracts.Emulation.Configurations;
using GWGUI.App.Contracts.Views.Emulation.Settings;
using GWGUI.App.Functions.Views.Emulation.Machine;
using GWGUI.App.Functions.Views.Emulation.Settings;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Services.Emulation;
using GWGUI.Emulation;
using System.Windows;
using System.Windows.Controls;

namespace GWGUI.App.Views.Controls.Emulation.Options;

internal sealed partial class EmulationModuleSettingsSection
{
    private UIElement BuildEditor()
    {
        var layout = new Grid();
        layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(250) });
        layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });
        layout.ColumnDefinitions.Add(new ColumnDefinition());
        var navigation = new Border { Padding = new Thickness(8), Child = _machines };
        navigation.SetResourceReference(FrameworkElement.StyleProperty, ControlVisualConstants.CardStyleResource);
        layout.Children.Add(navigation);
        var tabs = BuildMachineTabs();
        Grid.SetColumn(tabs, 2);
        layout.Children.Add(tabs);
        return layout;
    }

    private UIElement BuildGeneralHeader()
    {
        var heading = new Grid { Margin = new Thickness(0, 0, 0, 12) };
        heading.ColumnDefinitions.Add(new ColumnDefinition());
        var save = new Button
        {
            Content = LocExtension.Get("Common.Create"),
            MinWidth = 110,
            HorizontalAlignment = HorizontalAlignment.Right,
            Visibility = _saved.Any(configuration => configuration.MachineId == _configuration.MachineId)
                ? Visibility.Collapsed : Visibility.Visible
        };
        save.Click += async (_, _) => await ExecuteAsync(SaveAsync);
        heading.Children.Add(save);
        return heading;
    }

    private UIElement BuildMachineTabs()
    {
        _fieldControls.Clear();
        _userChangeHandlers.Clear();
        var settings = _module.Describe(_configuration.MachineId, _configuration);
        var tabs = EmulationMachineTabs.Create(tab => settings.Visibility.Tabs.GetValueOrDefault(tab)
            ? BuildTab(settings, tab)
            : null, $"{_module.Id}:{_configuration.MachineId}", TabActivatedAsync, _selectedTab);
        AttachUserChangeHandlers();
        return tabs;
    }

    private UIElement BuildTab(EmulationMachineSettings settings, EmulationMachineTab tab)
    {
        if (tab == EmulationMachineTab.Cpu) return BuildCpuSettingsTab(settings);
        if (tab == EmulationMachineTab.Ram) return BuildMemorySettingsTab(settings);
        if (tab == EmulationMachineTab.Video) return BuildVideoSettingsTab(settings);
        if (tab is (EmulationMachineTab.Keyboard or EmulationMachineTab.Mouse
            or EmulationMachineTab.Controllers) && _inputSettings is not null)
            return BuildInputSettingsTab(settings, tab);
        var panel = new StackPanel { Margin = new Thickness(12) };
        if (tab == EmulationMachineTab.General) panel.Children.Add(BuildGeneralHeader());
        if (tab == EmulationMachineTab.General && _emulatorManagement is not null)
            panel.Children.Add(_emulatorManagement.CreateView());
        AddBlocks(panel, settings, tab);
        ApplySettingsRules(settings);
        if (_storageSettings is not null && tab == EmulationMachineTab.Storage)
            panel.Children.Insert(0, _storageSettings.CreateContent(_configuration));
        if (tab == EmulationMachineTab.Rom && _firmwareManagement is not null)
            return _firmwareManagement.CreateView(panel);
        return EmulationSettingsLayout.ScrollPage(panel);
    }

    private UIElement BuildVideoSettingsTab(EmulationMachineSettings settings)
    {
        const string rendererResourceKey = EmulationResourceKeys.VideoRenderingSettings;
        var fields = settings.Blocks
            .Where(block => block.Tab == EmulationMachineTab.Video && block.IsVisible)
            .SelectMany(block => block.Fields)
            .Where(field => field.IsVisible)
            .ToArray();
        var display = fields.Where(field => field.LabelResourceKey != rendererResourceKey)
            .Select(CreateVideoSettingsField).ToArray();
        var profile = _profiles.Get(_module.Id, _configuration.Id);
        var renderer = new ComboBox
        {
            ItemsSource = EmulationVideoProcessingCatalog.RendererResourceKeys.Select(pair =>
                new KeyValuePair<EmulationVideoRenderer, string>(pair.Key, LocExtension.Get(pair.Value))).ToArray(),
            DisplayMemberPath = nameof(KeyValuePair<EmulationVideoRenderer, string>.Value),
            SelectedValuePath = nameof(KeyValuePair<EmulationVideoRenderer, string>.Key),
            SelectedValue = profile.Renderer
        };
        System.Windows.Automation.AutomationProperties.SetAutomationId(renderer,
            nameof(EmulationVideoPresentationProfile.Renderer));
        renderer.SelectionChanged += async (_, _) =>
        {
            if (renderer.SelectedValue is not EmulationVideoRenderer choice) return;
            var current = _profiles.Get(_module.Id, _configuration.Id);
            _profiles.Set(_module.Id, _configuration.Id, current with { Renderer = choice });
            if (!_saved.Any(item => item.Id == _configuration.Id))
                EmulationConfigurationDraftStore.Set(_module.Id, _configuration);
            VideoConfigurationChanged?.Invoke(this, new EmulationConfigurationSavedEventArgs(_configuration));
            await ExecuteAsync(SaveVideoProfileAsync);
        };
        var rendererChoice = EmulationSettingsLayout.VideoSettingsChoice(
            new EmulationVideoSettingsField(LocExtension.Get(rendererResourceKey), renderer));
        _videoProcessing.SetConfiguration(profile.Processing,
            EmulationSettingsLayout.VideoSettingsFields(display), rendererChoice);
        _videoProcessing.SetShaderLoading(EmulationVideoShaderLoadingStatus.IsLoading(
            _module.Id, _configuration.Id));
        ApplySettingsRules(settings);
        return EmulationSettingsLayout.VideoSettingsPage(_videoProcessing);
    }

    private EmulationVideoSettingsField CreateVideoSettingsField(EmulationSettingsField field) =>
        new(LocExtension.GetForModule(_module, field.LabelResourceKey), CreateField(field),
            IsTrailingCheckBox: field.Editor == EmulationSettingsEditor.Toggle);

    private void AddBlocks(Panel panel, EmulationMachineSettings settings, EmulationMachineTab tab)
    {
        foreach (var block in settings.Blocks.Where(block => block.Tab == tab && block.IsVisible))
        {
            var fields = block.Fields.Where(field => field.IsVisible).Select(CreateControlField).ToArray();
            if (fields.Length == 0) continue;
            var columns = tab == EmulationMachineTab.Rom ? 1 : Math.Max(1, block.Columns);
            var form = EmulationSettingsLayout.CompactForm(columns, fields);
            panel.Children.Add(EmulationSettingsLayout.IconCard(form,
                LocExtension.GetForModule(_module, block.TitleResourceKey), block.Icon ?? IconGlyphs.Settings));
        }
    }
}
