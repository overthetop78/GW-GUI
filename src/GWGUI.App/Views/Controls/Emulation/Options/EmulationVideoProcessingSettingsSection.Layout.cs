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
    private FrameworkElement CreateDisplayAndRendering()
    {
        var selectors = CreateSelectors();
        if (_displaySettings is null)
            return FramedGroup(EmulationResourceKeys.VideoGwGuiProcessing, selectors);
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        var display = FramedGroup(EmulationResourceKeys.VideoDisplaySettings, _displaySettings);
        display.Margin = new Thickness(0, 0, 9, 0);
        grid.Children.Add(display);
        var processing = FramedGroup(EmulationResourceKeys.VideoGwGuiProcessing, selectors);
        processing.Margin = new Thickness(9, 0, 0, 0);
        Grid.SetColumn(processing, 1);
        grid.Children.Add(processing);
        return grid;
    }

    private FrameworkElement CreateImageGroups() => SideBySideGroups(
        FramedSection(EmulationImageParametersSettingsBlock.Create(_configuration.Adjustments, update => SetAdjustments(update(_configuration.Adjustments)))),
        FramedSection(EmulationImageRestorationSettingsBlock.Create(_configuration.Restoration, update => SetRestoration(update(_configuration.Restoration)))));

    private FrameworkElement CreateEffectGroups()
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        var groups = new[]
        {
            FramedSection(EmulationTemporalEffectsSettingsBlock.Create(_configuration.Temporal, update => SetTemporal(update(_configuration.Temporal)))),
            FramedSection(CreateSignalSimulation()),
            FramedSection(CreateStylistic())
        };
        for (var index = 0; index < groups.Length; index++)
        {
            groups[index].Margin = new Thickness(index == 0 ? 0 : 6, 0,
                index == groups.Length - 1 ? 0 : 6, 0);
            Grid.SetColumn(groups[index], index);
            grid.Children.Add(groups[index]);
        }
        return grid;
    }

    private static FrameworkElement SideBySideGroups(FrameworkElement left,
        FrameworkElement right)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        left.Margin = new Thickness(0, 0, 9, 0);
        right.Margin = new Thickness(9, 0, 0, 0);
        grid.Children.Add(left);
        Grid.SetColumn(right, 1);
        grid.Children.Add(right);
        return grid;
    }

    private FrameworkElement CreateSelectors()
    {
        var panel = new StackPanel();
        _shaderLoadingIndicator = CreateShaderLoadingIndicator();
        panel.Children.Add(_shaderLoadingIndicator);
        if (_rendererChoice is not null) panel.Children.Add(_rendererChoice);
        panel.Children.Add(ChoiceField(EmulationResourceKeys.VideoSampling,
            EmulationVideoProcessingCatalog.SamplingResourceKeys, _configuration.Sampling,
            value => Update(configuration => configuration with { Sampling = value })));
        panel.Children.Add(ChoiceField(EmulationResourceKeys.VideoTechnology,
            EmulationVideoProcessingCatalog.DisplayTechnologyResourceKeys,
            _configuration.DisplayTechnology, value =>
            {
                Update(configuration => configuration with { DisplayTechnology = value });
                RebuildContent();
            }));
        return panel;
    }

    private FrameworkElement CreateShaderLoadingIndicator()
    {
        var loading = new StackPanel
        {
            Margin = new Thickness(0, 0, 0, 12),
            Visibility = _shaderLoading ? Visibility.Visible : Visibility.Collapsed
        };
        loading.Children.Add(new TextBlock
        {
            Text = LocExtension.Get(EmulationResourceKeys.VideoShaderLoading),
            FontWeight = FontWeights.SemiBold
        });
        loading.Children.Add(new ProgressBar
        {
            IsIndeterminate = true,
            Height = 4,
            Margin = new Thickness(0, 7, 0, 0)
        });
        AutomationProperties.SetAutomationId(loading,
            EmulationVideoSettingsLayoutConstants.VideoShaderLoadingAutomationId);
        return loading;
    }

    private FrameworkElement CreateSignalSimulation()
    {
        var panel = Section(EmulationResourceKeys.VideoSignalSimulationSettings);
        panel.Children.Add(ChoiceField(ParameterKey(EmulationVideoProcessingCatalog.SignalConnection),
            EmulationVideoProcessingCatalog.SignalConnectionResourceKeys,
            _configuration.SignalSimulation.Connection, value =>
            {
                SetSignalSimulation(_configuration.SignalSimulation with { Connection = value });
                RebuildContent();
            }, EmulationVideoProcessingCatalog.SignalConnection));
        if (_configuration.SignalSimulation.Connection != EmulationSignalConnection.None)
        {
            AddIntensity(panel, EmulationVideoProcessingCatalog.SignalConnectionIntensity,
                _configuration.SignalSimulation.ConnectionIntensity,
                value => SetSignalSimulation(_configuration.SignalSimulation with
                { ConnectionIntensity = value }));
            panel.Children.Add(ChoiceField(ParameterKey(EmulationVideoProcessingCatalog.SignalStandard),
                EmulationVideoProcessingCatalog.SignalStandardResourceKeys,
                _configuration.SignalSimulation.Standard, value =>
                {
                    SetSignalSimulation(_configuration.SignalSimulation with { Standard = value });
                    RebuildContent();
                }, EmulationVideoProcessingCatalog.SignalStandard));
            AddIntensity(panel, EmulationVideoProcessingCatalog.SignalStandardIntensity,
                _configuration.SignalSimulation.StandardIntensity,
                value => SetSignalSimulation(_configuration.SignalSimulation with
                { StandardIntensity = value }));
        }
        return panel;
    }

    private FrameworkElement CreateStylistic()
    {
        var panel = Section(EmulationResourceKeys.VideoStylisticSettings);
        AddIntensity(panel, EmulationVideoProcessingCatalog.Grain, _configuration.Stylistic.Grain,
            value => SetStylistic(_configuration.Stylistic with { Grain = value }));
        AddIntensity(panel, EmulationVideoProcessingCatalog.Vhs, _configuration.Stylistic.Vhs,
            value => SetStylistic(_configuration.Stylistic with { Vhs = value }));
        AddIntensity(panel, EmulationVideoProcessingCatalog.ChromaticAberration,
            _configuration.Stylistic.ChromaticAberration,
            value => SetStylistic(_configuration.Stylistic with { ChromaticAberration = value }));
        AddIntensity(panel, EmulationVideoProcessingCatalog.Bloom, _configuration.Stylistic.Bloom,
            value => SetStylistic(_configuration.Stylistic with { Bloom = value }));
        AddToggle(panel, EmulationVideoProcessingCatalog.Sepia, _configuration.Stylistic.Sepia,
            value => SetStylistic(_configuration.Stylistic with { Sepia = value }));
        return panel;
    }

    private FrameworkElement? CreateTechnologyPanel() => _configuration.DisplayTechnology switch
    {
        EmulationVideoDisplayTechnology.Crt => CreateCrtPanel(),
        EmulationVideoDisplayTechnology.FixedPixel => CreateFixedPixelPanel(),
        EmulationVideoDisplayTechnology.Plasma => CreatePlasmaPanel(),
        EmulationVideoDisplayTechnology.Vector => CreateVectorPanel(),
        EmulationVideoDisplayTechnology.Vfd => CreateVfdPanel(),
        EmulationVideoDisplayTechnology.LedMatrix => CreateLedMatrixPanel(),
        EmulationVideoDisplayTechnology.DotMatrix => CreateDotMatrixPanel(),
        EmulationVideoDisplayTechnology.SegmentDisplay => CreateSegmentDisplayPanel(),
        EmulationVideoDisplayTechnology.EPaper => CreateEPaperPanel(),
        EmulationVideoDisplayTechnology.Projection => CreateProjectionPanel(),
        _ => null
    };

    private static TabItem Tab(string id, string headerResourceKey, FrameworkElement content,
        double maximumWidth = EmulationVideoSettingsLayoutConstants.TabContentMaximumWidth,
        bool compactFields = true, bool frameContent = true)
    {
        if (compactFields) content = CompactSection(content);
        var container = new Border
        {
            Child = content,
            MaxWidth = maximumWidth,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(12)
        };
        if (frameContent)
            container.SetResourceReference(StyleProperty,
                ControlVisualConstants.CardStyleResource);
        var scroller = new ScrollViewer
        {
            Content = container,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            HorizontalContentAlignment = HorizontalAlignment.Stretch
        };
        return new TabItem
        {
            Header = LocExtension.Get(headerResourceKey),
            Tag = id,
            Content = scroller
        };
    }

    private static Border FramedSection(FrameworkElement content)
    {
        var frame = new Border { Child = content };
        frame.SetResourceReference(StyleProperty, ControlVisualConstants.CardStyleResource);
        return frame;
    }

    private static Border FramedGroup(string titleResourceKey, FrameworkElement content,
        bool compact = false)
    {
        var panel = new StackPanel();
        panel.Children.Add(new TextBlock
        {
            Text = LocExtension.Get(titleResourceKey),
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 10)
        });
        panel.Children.Add(compact ? CompactFields(content) : content);
        return FramedSection(panel);
    }

    private static FrameworkElement CompactFields(FrameworkElement content)
    {
        if (content is not StackPanel panel || panel.Children.Count < 2) return content;
        var children = panel.Children.Cast<UIElement>().ToArray();
        panel.Children.Clear();
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        for (var index = 0; index < children.Length; index++)
        {
            var child = children[index];
            if (child is FrameworkElement element)
                element.Margin = new Thickness(index % 2 == 0 ? 0 : 8, 0,
                    index % 2 == 0 ? 8 : 0, 0);
            Grid.SetColumn(child, index % 2);
            Grid.SetRow(child, index / 2);
            grid.Children.Add(child);
        }
        for (var row = 0; row < (children.Length + 1) / 2; row++)
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        return grid;
    }

    private static FrameworkElement CompactSection(FrameworkElement content)
    {
        if (content is not StackPanel section || section.Children.Count < 2) return content;
        var children = section.Children.Cast<UIElement>().ToArray();
        section.Children.Clear();
        section.Children.Add(children[0]);
        var fields = new Grid();
        fields.ColumnDefinitions.Add(new ColumnDefinition());
        fields.ColumnDefinitions.Add(new ColumnDefinition());
        var columns = new[] { new StackPanel(), new StackPanel() };
        for (var index = 1; index < children.Length; index++)
        {
            var field = children[index];
            var cell = index - 1;
            if (field is FrameworkElement element)
                element.Margin = new Thickness(cell % 2 == 0 ? 0 : 10, 2,
                    cell % 2 == 0 ? 10 : 0, 6);
            columns[cell % 2].Children.Add(field);
        }
        fields.Children.Add(columns[0]);
        Grid.SetColumn(columns[1], 1);
        fields.Children.Add(columns[1]);
        section.Children.Add(fields);
        return section;
    }

}
