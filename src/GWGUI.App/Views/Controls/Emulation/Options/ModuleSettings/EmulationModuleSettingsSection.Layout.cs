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

    private Button BuildCreateButton()
    {
        var save = new Button
        {
            Content = LocExtension.Get("Common.Create"),
            MinWidth = 110,
            HorizontalAlignment = HorizontalAlignment.Right,
            Visibility = _saved.Any(configuration => configuration.MachineId == _configuration.MachineId) ? Visibility.Collapsed : Visibility.Visible
        };
        save.Click += async (_, _) => await ExecuteAsync(SaveAsync);
        return save;
    }

    private UIElement BuildMachineTabs()
    {
        _fieldControls.Clear();
        _userChangeHandlers.Clear();
        var settings = _module.Describe(_configuration.MachineId, _configuration);
        var tabs = EmulationMachineTabs.Create(tab => settings.Visibility.Tabs.GetValueOrDefault(tab) ? BuildTab(settings, tab) : null, $"{_module.Id}:{_configuration.MachineId}", TabActivatedAsync, _selectedTab);
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
        if (tab == EmulationMachineTab.General)
        {
            var createButton = BuildCreateButton();
            if (_emulatorManagement is not null)
                panel.Children.Add(_emulatorManagement.CreateView(createButton));
            else
            {
                var heading = new Grid { Margin = new Thickness(0, 0, 0, 12) };
                heading.Children.Add(createButton);
                panel.Children.Add(heading);
            }
        }
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
        var fields = settings.Blocks.Where(block => block.Tab == EmulationMachineTab.Video && block.IsVisible).SelectMany(block => block.Fields).Where(field => field.IsVisible).ToArray();
        var blocks = settings.Blocks.Where(block => block.Tab == EmulationMachineTab.Video && block.IsVisible).ToArray();
        FrameworkElement? nativeSettings = null;
        FrameworkElement display;
        if (blocks.Any(block => block.IsCompact))
        {
            display = EmulationSettingsLayout.VideoSettingsFields(blocks.Where(block => !block.IsCollapsible).SelectMany(block => block.Fields).Where(field => field.IsVisible && field.LabelResourceKey != rendererResourceKey).Select(CreateVideoSettingsField).ToArray());
            var groups = new StackPanel();
            foreach (var block in blocks.Where(block => block.IsCollapsible))
            {
                groups.Children.Add(new Expander
                {
                    Header = LocExtension.GetForModule(_module, block.TitleResourceKey),
                    IsExpanded = groups.Children.Count == 0,
                    Content = EmulationSettingsLayout.VideoSettingsFields(block.Fields.Where(field => field.IsVisible && field.LabelResourceKey != rendererResourceKey).Select(CreateVideoSettingsField).ToArray()),
                    Margin = new Thickness(0, 0, 0, 12)
                });
            }
            if (groups.Children.Count > 0) nativeSettings = groups;
        }
        else if (!blocks.Any(block => block.IsCollapsible))
            display = EmulationSettingsLayout.VideoSettingsFields(fields.Where(field => field.LabelResourceKey != rendererResourceKey).Select(CreateVideoSettingsField).ToArray());
        else
        {
            var panel = new StackPanel();
            foreach (var block in blocks)
            {
                var blockFields = block.Fields.Where(field => field.IsVisible && field.LabelResourceKey != rendererResourceKey).Select(CreateVideoSettingsField).ToArray();
                if (blockFields.Length == 0) continue;
                var content = EmulationSettingsLayout.VideoSettingsFields(blockFields);
                panel.Children.Add(block.IsCollapsible ? new Expander
                {
                    Header = LocExtension.GetForModule(_module, block.TitleResourceKey),
                    Content = content,
                    Margin = new Thickness(0, 0, 0, 12)
                } : content);
            }
            display = panel;
        }
        var profile = _profiles.Get(_module.Id, _configuration.Id);
        var renderer = new ComboBox
        {
            ItemsSource = EmulationVideoProcessingCatalog.RendererResourceKeys.Select(pair => new KeyValuePair<EmulationVideoRenderer, string>(pair.Key, LocExtension.Get(pair.Value))).ToArray(),
            DisplayMemberPath = nameof(KeyValuePair<EmulationVideoRenderer, string>.Value),
            SelectedValuePath = nameof(KeyValuePair<EmulationVideoRenderer, string>.Key),
            SelectedValue = profile.Renderer
        };
        System.Windows.Automation.AutomationProperties.SetAutomationId(renderer, nameof(EmulationVideoPresentationProfile.Renderer));
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
            new EmulationVideoSettingsField(LocExtension.Get(rendererResourceKey), renderer, Explanation: LocExtension.Get("Emulation.Help.Video.Rendering.Short"), DetailedExplanation: LocExtension.Get("Emulation.Help.Video.Rendering.Detailed")));
        _videoProcessing.SetConfiguration(profile.Processing, display, rendererChoice, nativeSettings: nativeSettings);
        _videoProcessing.SetShaderLoading(EmulationVideoShaderLoadingStatus.IsLoading(_module.Id, _configuration.Id));
        ApplySettingsRules(settings);
        return EmulationSettingsLayout.VideoSettingsPage(_videoProcessing);
    }

    private EmulationVideoSettingsField CreateVideoSettingsField(EmulationSettingsField field) =>
        new(LocExtension.GetForModule(_module, field.LabelResourceKey), CreateField(field),
            IsTrailingCheckBox: field.Editor == EmulationSettingsEditor.Toggle,
            Explanation: field.ExplanationResourceKey is null ? null : LocExtension.GetForModule(_module, field.ExplanationResourceKey),
            DetailedExplanation: field.DetailedExplanationResourceKey is null ? null : LocExtension.GetForModule(_module, field.DetailedExplanationResourceKey));

    private void AddBlocks(Panel panel, EmulationMachineSettings settings, EmulationMachineTab tab)
    {
        bool IsDisplayed(EmulationSettingsField field) => field.IsVisible && !(tab == EmulationMachineTab.General && field.Editor == EmulationSettingsEditor.DirectoryPath && field.DefaultFolderCategory is not null);
        if (tab == EmulationMachineTab.Audio)
        {
            var blocks = settings.Blocks.Where(block => block.Tab == tab && block.IsVisible && block.Fields.Any(IsDisplayed))
                .OrderBy(block => block.Fields.Any(field => field.ChoiceSource == EmulationSettingsChoiceSource.AudioOutputDevices) ? 1 : 0).ToArray();
            var cards = blocks.Select(block => (FrameworkElement)EmulationSettingsLayout.IconCard(
                EmulationSettingsLayout.BoundedForm(1, block.Fields.Where(IsDisplayed).Select(CreateControlField).ToArray()),
                LocExtension.GetForModule(_module, block.TitleResourceKey), block.Icon ?? IconGlyphs.Audio)).ToList();
            var common = blocks.Select((block, index) => (block, card: cards[index]))
                .Where(item => item.block.Fields.Any(field => field.ChoiceSource == EmulationSettingsChoiceSource.AudioOutputDevices))
                .Select(item => item.card).ToHashSet();
            panel.Children.Add(EmulationSettingsLayout.CompactCards(cards, card => common.Contains(card) ? 1 : 0));
            return;
        }
        if (settings.Blocks.Any(block => block.Tab == tab && block.IsCompact))
        {
            panel.Children.Add(EmulationSettingsLayout.CompactCards(settings.Blocks.Where(block => block.Tab == tab && block.IsVisible && block.Fields.Any(IsDisplayed))
                .OrderBy(block => tab == EmulationMachineTab.Audio && !block.IsCompact ? 1 : 0)
                .Select(block => EmulationSettingsLayout.IconCard(
                    tab == EmulationMachineTab.Audio && !block.IsCompact ? EmulationSettingsLayout.CompactForm(1, block.Fields.Where(IsDisplayed).Select(CreateControlField).ToArray()) : EmulationSettingsLayout.BoundedForm(1, block.Fields.Where(IsDisplayed).Select(CreateControlField).ToArray()),
                    LocExtension.GetForModule(_module, block.TitleResourceKey),
                    block.Icon ?? (tab == EmulationMachineTab.Audio ? IconGlyphs.Audio : IconGlyphs.Settings)))));
            return;
        }
        foreach (var block in settings.Blocks.Where(block => block.Tab == tab && block.IsVisible))
        {
            var fields = block.Fields.Where(IsDisplayed).Select(CreateControlField).ToArray();
            if (fields.Length == 0) continue;
            var columns = tab == EmulationMachineTab.Rom ? 1 : Math.Max(1, block.Columns);
            var form = EmulationSettingsLayout.CompactForm(columns, fields);
            panel.Children.Add(EmulationSettingsLayout.IconCard(form, LocExtension.GetForModule(_module, block.TitleResourceKey), block.Icon ?? (tab == EmulationMachineTab.Audio ? IconGlyphs.Audio : IconGlyphs.Settings)));
        }
    }
}
