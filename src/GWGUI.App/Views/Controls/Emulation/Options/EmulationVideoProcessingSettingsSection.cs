using GWGUI.App.Constants.Localization;
using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Constants.Views.Emulation;
using GWGUI.App.Functions.Views.Emulation.Settings;
using GWGUI.App.Localization.Extensions;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Dictionaries;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;
using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace GWGUI.App.Views.Controls.Emulation.Options;

internal sealed partial class EmulationVideoProcessingSettingsSection : UserControl
{
    private EmulationVideoProcessingConfiguration _configuration = new();
    private FrameworkElement? _displaySettings;
    private FrameworkElement? _rendererChoice;
    private bool _loading;
    private Slider? _activeSliderEdit;
    private bool _sliderSavePending;
    private bool _shaderLoading;
    private FrameworkElement? _shaderLoadingIndicator;
    private string _selectedSettingsTab = EmulationVideoSettingsLayoutConstants.DisplayTabId;

    internal EmulationVideoProcessingSettingsSection()
    {
        RebuildContent();
    }

    internal event EventHandler? ConfigurationChanged;
    internal event EventHandler? ConfigurationSaveRequested;

    internal EmulationVideoProcessingConfiguration Configuration => _configuration;

    internal void SetShaderLoading(bool isLoading)
    {
        _shaderLoading = isLoading;
        if (_shaderLoadingIndicator is not null)
            _shaderLoadingIndicator.Visibility = isLoading
                ? Visibility.Visible : Visibility.Collapsed;
    }

    internal void SetConfiguration(EmulationVideoProcessingConfiguration? configuration)
    {
        _configuration = EmulationVideoProcessingConfigurationFunctions.Normalize(configuration);
        RebuildContent();
    }

    internal void SetConfiguration(EmulationVideoProcessingConfiguration? configuration,
        FrameworkElement? displaySettings, FrameworkElement? rendererChoice)
    {
        _configuration = EmulationVideoProcessingConfigurationFunctions.Normalize(configuration);
        _displaySettings = displaySettings;
        _rendererChoice = rendererChoice;
        RebuildContent();
    }

    internal void RefreshLocalizedContent() => RebuildContent();

    private void RebuildContent()
    {
        var offsets = (Content as TabControl)?.Items.Cast<TabItem>()
            .Where(item => item.Content is ScrollViewer)
            .ToDictionary(item => (string)item.Tag,
                item => ((ScrollViewer)item.Content).VerticalOffset, StringComparer.Ordinal)
            ?? new Dictionary<string, double>(StringComparer.Ordinal);
        _loading = true;
        try
        {
            if (_displaySettings is not null)
                EmulationSettingsLayout.DetachReusableElement(_displaySettings);
            if (_rendererChoice is not null)
                EmulationSettingsLayout.DetachReusableElement(_rendererChoice);
            var tabs = new TabControl { Margin = new Thickness(12) };
            tabs.Items.Add(Tab(EmulationVideoSettingsLayoutConstants.DisplayTabId,
                EmulationResourceKeys.VideoTabDisplay,
                CreateDisplayAndRendering(),
                EmulationVideoSettingsLayoutConstants.DisplayTabContentMaximumWidth,
                compactFields: false, frameContent: false));
            var technology = CreateTechnologyPanel();
            if (technology is not null)
            {
                AutomationProperties.SetAutomationId(technology,
                    EmulationVideoSettingsLayoutConstants.VideoTechnologyParametersAutomationId);
                tabs.Items.Add(Tab(EmulationVideoSettingsLayoutConstants.TechnologyTabId,
                    EmulationVideoProcessingCatalog.DisplayTechnologyResourceKeys[
                        _configuration.DisplayTechnology], technology,
                    frameContent: _configuration.DisplayTechnology is not
                        (EmulationVideoDisplayTechnology.Plasma or
                         EmulationVideoDisplayTechnology.FixedPixel or
                         EmulationVideoDisplayTechnology.Vector or
                         EmulationVideoDisplayTechnology.Vfd)));
            }
            tabs.Items.Add(Tab(EmulationVideoSettingsLayoutConstants.ImageTabId,
                EmulationResourceKeys.VideoTabImage,
                CreateImageGroups(),
                EmulationVideoSettingsLayoutConstants.DisplayTabContentMaximumWidth,
                compactFields: false, frameContent: false));
            tabs.Items.Add(Tab(EmulationVideoSettingsLayoutConstants.EffectsTabId,
                EmulationResourceKeys.VideoTabEffects,
                CreateEffectGroups(),
                EmulationVideoSettingsLayoutConstants.DisplayTabContentMaximumWidth,
                compactFields: false, frameContent: false));
            tabs.SelectedItem = tabs.Items.Cast<TabItem>().FirstOrDefault(item =>
                string.Equals((string)item.Tag, _selectedSettingsTab, StringComparison.Ordinal))
                ?? tabs.Items[0];
            tabs.SelectionChanged += (_, args) =>
            {
                if (!ReferenceEquals(args.Source, tabs) || tabs.SelectedItem is not TabItem selected)
                    return;
                _selectedSettingsTab = (string)selected.Tag;
            };
            Content = tabs;
            TrackSliderEdits(tabs);
            tabs.Loaded += (_, _) =>
            {
                foreach (var item in tabs.Items.Cast<TabItem>())
                    if (item.Content is ScrollViewer scroller
                        && offsets.TryGetValue((string)item.Tag, out var offset))
                        scroller.ScrollToVerticalOffset(offset);
            };
        }
        finally
        {
            _loading = false;
        }
    }

}
