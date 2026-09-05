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

internal sealed class EmulationVideoProcessingSettingsSection : UserControl
{
    private EmulationVideoProcessingConfiguration _configuration = new();
    private FrameworkElement? _displaySettings;
    private FrameworkElement? _rendererChoice;
    private bool _loading;
    private Slider? _activeSliderEdit;
    private bool _sliderSavePending;
    private bool _shaderLoading;
    private FrameworkElement? _shaderLoadingIndicator;
    private string _selectedSettingsTab = DisplayTab;

    private const string DisplayTab = "Display";
    private const string RenderingTab = "Rendering";
    private const string TechnologyTab = "Technology";
    private const string ImageTab = "Image";
    private const string RestorationTab = "Restoration";
    private const string MotionTab = "Motion";
    private const string SignalTab = "Signal";
    private const string EffectsTab = "Effects";

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
            tabs.Items.Add(Tab(DisplayTab, EmulationResourceKeys.VideoTabDisplay,
                CreateDisplayAndRendering(),
                EmulationVideoSettingsLayoutConstants.DisplayTabContentMaximumWidth,
                compactFields: false, frameContent: false));
            var technology = CreateTechnologyPanel();
            if (technology is not null)
            {
                AutomationProperties.SetAutomationId(technology, "VideoTechnologyParameters");
                tabs.Items.Add(Tab(TechnologyTab,
                    EmulationVideoProcessingCatalog.DisplayTechnologyResourceKeys[
                        _configuration.DisplayTechnology], technology,
                    frameContent: _configuration.DisplayTechnology is not
                        (EmulationVideoDisplayTechnology.Plasma or
                         EmulationVideoDisplayTechnology.FixedPixel or
                         EmulationVideoDisplayTechnology.Vector or
                         EmulationVideoDisplayTechnology.Vfd)));
            }
            tabs.Items.Add(Tab(ImageTab, EmulationResourceKeys.VideoTabImage,
                CreateImageGroups(),
                EmulationVideoSettingsLayoutConstants.DisplayTabContentMaximumWidth,
                compactFields: false, frameContent: false));
            tabs.Items.Add(Tab(EffectsTab, EmulationResourceKeys.VideoTabEffects,
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
        AutomationProperties.SetAutomationId(loading, "VideoShaderLoading");
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

    private static Border FramedGroup(string titleResourceKey, FrameworkElement content)
    {
        var panel = new StackPanel();
        panel.Children.Add(new TextBlock
        {
            Text = LocExtension.Get(titleResourceKey),
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 10)
        });
        panel.Children.Add(content);
        return FramedSection(panel);
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
        for (var row = 0; row < (children.Length - 1 + 1) / 2; row++)
            fields.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        for (var index = 1; index < children.Length; index++)
        {
            var field = children[index];
            var cell = index - 1;
            if (field is FrameworkElement element)
                element.Margin = new Thickness(cell % 2 == 0 ? 0 : 10, 2,
                    cell % 2 == 0 ? 10 : 0, 6);
            Grid.SetRow(field, cell / 2);
            Grid.SetColumn(field, cell % 2);
            fields.Children.Add(field);
        }
        section.Children.Add(fields);
        return section;
    }

    private FrameworkElement CreateCrtPanel()
    {
        var crt = _configuration.Crt;
        var panel = Section(EmulationResourceKeys.VideoTechnologyCrt);
        var color = new StackPanel();
        color.Children.Add(ChoiceField(ParameterKey(EmulationVideoProcessingCatalog.CrtColorMode),
            EmulationVideoProcessingCatalog.CrtColorModeResourceKeys, crt.ColorMode, value =>
            {
                SetCrt(_configuration.Crt with { ColorMode = value });
                RebuildContent();
            }, EmulationVideoProcessingCatalog.CrtColorMode));
        panel.Children.Add(FramedGroup("Emulation.Video.Crt.Group.Color", color));
        var beam = new StackPanel();
        AddIntensity(beam, EmulationVideoProcessingCatalog.CrtBeamWidth, crt.BeamWidth,
            value => SetCrt(_configuration.Crt with { BeamWidth = value }));
        AddIntensity(beam, EmulationVideoProcessingCatalog.CrtBeamIntensity, crt.BeamIntensity,
            value => SetCrt(_configuration.Crt with { BeamIntensity = value }));
        AddIntensity(beam, EmulationVideoProcessingCatalog.CrtBeamDiffusion, crt.BeamDiffusion,
            value => SetCrt(_configuration.Crt with { BeamDiffusion = value }));
        AddIntensity(beam, EmulationVideoProcessingCatalog.CrtHaloIntensity, crt.HaloIntensity,
            value => SetCrt(_configuration.Crt with { HaloIntensity = value }));
        panel.Children.Add(FramedGroup("Emulation.Video.Crt.Group.Beam", beam));
        var mask = new StackPanel();
        mask.Children.Add(ChoiceField(ParameterKey(EmulationVideoProcessingCatalog.CrtMask),
            EmulationVideoProcessingCatalog.CrtMaskResourceKeys, crt.Mask,
            value => { SetCrt(_configuration.Crt with { Mask = value }); RebuildContent(); },
            EmulationVideoProcessingCatalog.CrtMask));
        if (crt.Mask != EmulationCrtMask.None)
        {
        if (crt.ColorMode == EmulationCrtColorMode.Color)
        mask.Children.Add(ChoiceField(ParameterKey(EmulationVideoProcessingCatalog.CrtMaskSubpixels),
            EmulationVideoProcessingCatalog.SubpixelLayoutResourceKeys, crt.MaskSubpixels,
            value => SetCrt(_configuration.Crt with { MaskSubpixels = value }),
            EmulationVideoProcessingCatalog.CrtMaskSubpixels));
        AddIntensity(mask, EmulationVideoProcessingCatalog.CrtMaskIntensity, crt.MaskIntensity,
            value => SetCrt(_configuration.Crt with { MaskIntensity = value }));
        }
        panel.Children.Add(FramedGroup("Emulation.Video.Crt.Group.Mask", mask));
        var geometry = new StackPanel();
        AddSlider(geometry, EmulationVideoProcessingCatalog.CrtHorizontalCurvature,
            crt.HorizontalCurvature, -100, 100,
            value => SetCrt(_configuration.Crt with { HorizontalCurvature = value }));
        AddSlider(geometry, EmulationVideoProcessingCatalog.CrtVerticalCurvature,
            crt.VerticalCurvature, -100, 100,
            value => SetCrt(_configuration.Crt with { VerticalCurvature = value }));
        AddSlider(geometry, EmulationVideoProcessingCatalog.CrtTrapezoid, crt.Trapezoid, -100, 100,
            value => SetCrt(_configuration.Crt with { Trapezoid = value }));
        AddIntensity(geometry, EmulationVideoProcessingCatalog.CrtVignette, crt.Vignette,
            value => SetCrt(_configuration.Crt with { Vignette = value }));
        panel.Children.Add(FramedGroup("Emulation.Video.Crt.Group.Geometry", geometry));
        var scanlines = new StackPanel();
        AddToggle(scanlines, EmulationVideoProcessingCatalog.CrtScanlinesEnabled, crt.ScanlinesEnabled, value =>
        {
            SetCrt(_configuration.Crt with { ScanlinesEnabled = value });
            RebuildContent();
        });
        if (crt.ScanlinesEnabled) AddScanlineFields(scanlines, crt);
        panel.Children.Add(FramedGroup("Emulation.Video.Crt.Group.Scanlines", scanlines));
        var pattern = new StackPanel();
        AddToggle(pattern, EmulationVideoProcessingCatalog.CrtPatternEnabled, crt.PatternEnabled, value =>
        {
            SetCrt(_configuration.Crt with { PatternEnabled = value });
            RebuildContent();
        });
        if (crt.PatternEnabled) AddPatternFields(pattern, crt);
        panel.Children.Add(FramedGroup("Emulation.Video.Crt.Group.Interference", pattern));
        return panel;
    }

    private void AddScanlineFields(Panel panel, EmulationCrtVideoConfiguration crt)
    {
        panel.Children.Add(ChoiceField(ParameterKey(EmulationVideoProcessingCatalog.CrtScanlineOrientation),
            EmulationVideoProcessingCatalog.PatternOrientationResourceKeys, crt.ScanlineOrientation,
            value => SetCrt(_configuration.Crt with { ScanlineOrientation = value }),
            EmulationVideoProcessingCatalog.CrtScanlineOrientation));
        AddIntensity(panel, EmulationVideoProcessingCatalog.CrtScanlineIntensity, crt.ScanlineIntensity,
            value => SetCrt(_configuration.Crt with { ScanlineIntensity = value }));
        AddIntensity(panel, EmulationVideoProcessingCatalog.CrtScanlineThickness, crt.ScanlineThickness,
            value => SetCrt(_configuration.Crt with { ScanlineThickness = value }));
        panel.Children.Add(ChoiceField(ParameterKey(EmulationVideoProcessingCatalog.CrtScanlinePhase),
            EmulationVideoProcessingCatalog.ScanlinePhaseResourceKeys, crt.ScanlinePhase,
            value => SetCrt(_configuration.Crt with { ScanlinePhase = value }),
            EmulationVideoProcessingCatalog.CrtScanlinePhase));
        AddIntensity(panel, EmulationVideoProcessingCatalog.CrtScanlineCompensation, crt.ScanlineCompensation,
            value => SetCrt(_configuration.Crt with { ScanlineCompensation = value }));
    }

    private void AddPatternFields(Panel panel, EmulationCrtVideoConfiguration crt)
    {
        panel.Children.Add(ChoiceField(ParameterKey(EmulationVideoProcessingCatalog.CrtPatternOrientation),
            EmulationVideoProcessingCatalog.PatternOrientationResourceKeys, crt.PatternOrientation,
            value => SetCrt(_configuration.Crt with { PatternOrientation = value }),
            EmulationVideoProcessingCatalog.CrtPatternOrientation));
        AddIntensity(panel, EmulationVideoProcessingCatalog.CrtPatternFrequency, crt.PatternFrequency,
            value => SetCrt(_configuration.Crt with { PatternFrequency = value }));
        AddIntensity(panel, EmulationVideoProcessingCatalog.CrtPatternPhase, crt.PatternPhase,
            value => SetCrt(_configuration.Crt with { PatternPhase = value }));
        AddIntensity(panel, EmulationVideoProcessingCatalog.CrtPatternIntensity, crt.PatternIntensity,
            value => SetCrt(_configuration.Crt with { PatternIntensity = value }));
    }

    private FrameworkElement CreateFixedPixelPanel() =>
        EmulationFixedPixelSettingsBlock.Create(_configuration.FixedPixel,
            update => SetFixedPixel(update(_configuration.FixedPixel)), RebuildContent);

    private FrameworkElement CreatePlasmaPanel() =>
        EmulationPlasmaSettingsBlock.Create(_configuration.Plasma,
            update => SetPlasma(update(_configuration.Plasma)));

    private FrameworkElement CreateVectorPanel() =>
        EmulationVectorSettingsBlock.Create(_configuration.Vector,
            update => SetVector(update(_configuration.Vector)));

    private FrameworkElement CreateVfdPanel() =>
        EmulationVfdSettingsBlock.Create(_configuration.Vfd,
            update => SetVfd(update(_configuration.Vfd)), RebuildContent);

    private FrameworkElement CreateLedMatrixPanel() =>
        EmulationLedMatrixSettingsBlock.Create(_configuration.LedMatrix,
            update => SetLedMatrix(update(_configuration.LedMatrix)));

    private FrameworkElement CreateDotMatrixPanel() =>
        EmulationDotMatrixSettingsBlock.Create(_configuration.DotMatrix,
            update => SetDotMatrix(update(_configuration.DotMatrix)));

    private FrameworkElement CreateSegmentDisplayPanel() =>
        EmulationSegmentDisplaySettingsBlock.Create(_configuration.SegmentDisplay,
            update => SetSegmentDisplay(update(_configuration.SegmentDisplay)));

    private FrameworkElement CreateEPaperPanel() =>
        EmulationEPaperSettingsBlock.Create(_configuration.EPaper,
            update => SetEPaper(update(_configuration.EPaper)), RebuildContent);

    private FrameworkElement CreateProjectionPanel()
    {
        var projection = _configuration.Projection;
        var panel = Section(EmulationResourceKeys.VideoTechnologyProjection);
        AddIntensity(panel, EmulationVideoProcessingCatalog.ProjectionOpticalBlur,
            projection.OpticalBlur,
            value => SetProjection(_configuration.Projection with { OpticalBlur = value }));
        AddIntensity(panel, EmulationVideoProcessingCatalog.ProjectionDiffusion,
            projection.Diffusion, value => SetProjection(_configuration.Projection with { Diffusion = value }));
        AddIntensity(panel, EmulationVideoProcessingCatalog.ProjectionScreenTexture,
            projection.ScreenTexture,
            value => SetProjection(_configuration.Projection with { ScreenTexture = value }));
        AddIntensity(panel, EmulationVideoProcessingCatalog.ProjectionConvergence,
            projection.Convergence,
            value => SetProjection(_configuration.Projection with { Convergence = value }));
        return panel;
    }

    private static StackPanel Section(string resourceKey)
    {
        var section = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
        section.Children.Add(new TextBlock
        {
            Text = LocExtension.Get(resourceKey),
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 8)
        });
        return section;
    }

    private FrameworkElement ChoiceField<T>(string labelResourceKey,
        IReadOnlyDictionary<T, string> resourceKeys, T selected, Action<T> changed,
        string? automationId = null) where T : struct, Enum
    {
        var choices = resourceKeys.Select(choice => new Choice<T>(choice.Key,
            LocExtension.Get(choice.Value))).ToArray();
        var selector = new ComboBox
        {
            ItemsSource = choices,
            DisplayMemberPath = nameof(Choice<T>.DisplayName),
            SelectedItem = choices.First(choice => EqualityComparer<T>.Default.Equals(choice.Value, selected)),
            MinWidth = 180
        };
        AutomationProperties.SetAutomationId(selector, automationId ?? typeof(T).Name);
        selector.SelectionChanged += (_, _) =>
        {
            if (!_loading && selector.SelectedItem is Choice<T> choice) changed(choice.Value);
        };
        return Field(labelResourceKey, selector);
    }

    private void AddIntensity(Panel panel, string id, int value, Action<int> changed) =>
        AddSlider(panel, id, value, EmulationVideoProcessingLimits.IntensityMinimum,
            EmulationVideoProcessingLimits.IntensityMaximum, changed);

    private void AddOptionalIntensity(Panel panel, string id, int? value, Action<int?> changed)
    {
        var enabled = value.HasValue;
        var row = new StackPanel();
        var toggle = new CheckBox { IsChecked = enabled };
        AutomationProperties.SetAutomationId(toggle, id + "Enabled");
        toggle.Checked += (_, _) =>
        {
            if (!_loading) changed(value ?? EmulationVideoProcessingLimits.IntensityMinimum);
            RebuildContent();
        };
        toggle.Unchecked += (_, _) =>
        {
            if (!_loading) changed(null);
            RebuildContent();
        };
        row.Children.Add(Field(ParameterKey(id), toggle));
        if (enabled)
            AddSlider(row, id, value!.Value, EmulationVideoProcessingLimits.IntensityMinimum,
                EmulationVideoProcessingLimits.IntensityMaximum, number => changed(number));
        panel.Children.Add(row);
    }

    private void AddSlider(Panel panel, string id, int value, int minimum, int maximum,
        Action<int> changed)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var slider = new Slider
        {
            Minimum = minimum,
            Maximum = maximum,
            Value = value,
            TickFrequency = 1,
            IsSnapToTickEnabled = true
        };
        AutomationProperties.SetAutomationId(slider, id);
        var displayedValue = new TextBlock
        {
            Text = value.ToString(CultureInfo.CurrentCulture),
            MinWidth = 36,
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 0, 0)
        };
        slider.ValueChanged += (_, _) =>
        {
            var number = (int)slider.Value;
            displayedValue.Text = number.ToString(CultureInfo.CurrentCulture);
            if (!_loading) changed(number);
        };
        grid.Children.Add(slider);
        Grid.SetColumn(displayedValue, 1);
        grid.Children.Add(displayedValue);
        panel.Children.Add(Field(ParameterKey(id), grid));
    }

    private void AddToggle(Panel panel, string id, bool value, Action<bool> changed)
    {
        var toggle = new CheckBox { IsChecked = value };
        AutomationProperties.SetAutomationId(toggle, id);
        toggle.Checked += (_, _) => { if (!_loading) changed(true); };
        toggle.Unchecked += (_, _) => { if (!_loading) changed(false); };
        panel.Children.Add(Field(ParameterKey(id), toggle));
    }

    private void AddArgb(Panel panel, string id, uint? value, Action<uint?> changed)
    {
        var input = new TextBox { Text = value?.ToString("X8", CultureInfo.InvariantCulture) ?? string.Empty };
        AutomationProperties.SetAutomationId(input, id);
        input.LostKeyboardFocus += (_, _) =>
        {
            if (uint.TryParse(input.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var color))
                changed(color);
            else if (string.IsNullOrWhiteSpace(input.Text))
                changed(null);
            else
                input.Text = value?.ToString("X8", CultureInfo.InvariantCulture) ?? string.Empty;
        };
        panel.Children.Add(Field(ParameterKey(id), input));
    }

    private static FrameworkElement Field(string labelResourceKey, FrameworkElement control)
    {
        var field = new StackPanel { Margin = new Thickness(0, 3, 0, 9) };
        field.Children.Add(new TextBlock
        {
            Text = LocExtension.Get(labelResourceKey),
            Margin = new Thickness(0, 0, 0, 4),
            TextWrapping = TextWrapping.Wrap
        });
        field.Children.Add(control);
        return field;
    }

    private static string ParameterKey(string id) =>
        EmulationVideoProcessingCatalog.ParameterResourceKeys[id];

    private void SetAdjustments(EmulationImageAdjustments value) =>
        Update(configuration => configuration with { Adjustments = value });

    private void SetRestoration(EmulationImageRestorationConfiguration value) =>
        Update(configuration => configuration with { Restoration = value });

    private void SetTemporal(EmulationTemporalVideoConfiguration value) =>
        Update(configuration => configuration with { Temporal = value });

    private void SetSignalSimulation(EmulationSignalSimulationConfiguration value) =>
        Update(configuration => configuration with { SignalSimulation = value });

    private void SetStylistic(EmulationStylisticVideoConfiguration value) =>
        Update(configuration => configuration with { Stylistic = value });

    private void SetCrt(EmulationCrtVideoConfiguration value) =>
        Update(configuration => configuration with { Crt = value });

    private void SetFixedPixel(EmulationFixedPixelVideoConfiguration value) =>
        Update(configuration => configuration with { FixedPixel = value });

    private void SetPlasma(EmulationPlasmaVideoConfiguration value) =>
        Update(configuration => configuration with { Plasma = value });

    private void SetVector(EmulationVectorVideoConfiguration value) =>
        Update(configuration => configuration with { Vector = value });

    private void SetVfd(EmulationVfdVideoConfiguration value) =>
        Update(configuration => configuration with { Vfd = value });

    private void SetLedMatrix(EmulationLedMatrixVideoConfiguration value) =>
        Update(configuration => configuration with { LedMatrix = value });

    private void SetDotMatrix(EmulationDotMatrixVideoConfiguration value) =>
        Update(configuration => configuration with { DotMatrix = value });

    private void SetSegmentDisplay(EmulationSegmentDisplayVideoConfiguration value) =>
        Update(configuration => configuration with { SegmentDisplay = value });

    private void SetEPaper(EmulationEPaperVideoConfiguration value) =>
        Update(configuration => configuration with { EPaper = value });

    private void SetProjection(EmulationProjectionVideoConfiguration value) =>
        Update(configuration => configuration with { Projection = value });

    private void Update(Func<EmulationVideoProcessingConfiguration,
        EmulationVideoProcessingConfiguration> update)
    {
        if (_loading) return;
        _configuration = EmulationVideoProcessingConfigurationFunctions.Normalize(update(_configuration));
        ConfigurationChanged?.Invoke(this, EventArgs.Empty);
        if (_activeSliderEdit is null)
            ConfigurationSaveRequested?.Invoke(this, EventArgs.Empty);
        else
            _sliderSavePending = true;
    }

    private void TrackSliderEdits(DependencyObject root)
    {
        foreach (var slider in Descendants(root).OfType<Slider>())
        {
            slider.PreviewMouseLeftButtonDown += (_, _) =>
            {
                _activeSliderEdit = slider;
                _sliderSavePending = false;
            };
            slider.PreviewMouseLeftButtonUp += (_, _) => CompleteSliderEdit(slider);
            slider.LostMouseCapture += (_, _) => CompleteSliderEdit(slider);
        }
    }

    private void CompleteSliderEdit(Slider slider)
    {
        if (!ReferenceEquals(_activeSliderEdit, slider)) return;
        _activeSliderEdit = null;
        if (!_sliderSavePending) return;
        _sliderSavePending = false;
        ConfigurationSaveRequested?.Invoke(this, EventArgs.Empty);
    }

    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
        {
            yield return child;
            foreach (var descendant in Descendants(child))
                yield return descendant;
        }
    }

    private sealed record Choice<T>(T Value, string DisplayName) where T : struct, Enum;
}
