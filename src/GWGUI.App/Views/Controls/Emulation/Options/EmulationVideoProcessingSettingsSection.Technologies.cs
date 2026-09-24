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
        panel.Children.Add(FramedGroup(EmulationResourceKeys.VideoCrtGroupColor,
            color, compact: true));
        var beam = new StackPanel();
        AddIntensity(beam, EmulationVideoProcessingCatalog.CrtBeamWidth, crt.BeamWidth,
            value => SetCrt(_configuration.Crt with { BeamWidth = value }));
        AddIntensity(beam, EmulationVideoProcessingCatalog.CrtBeamIntensity, crt.BeamIntensity,
            value => SetCrt(_configuration.Crt with { BeamIntensity = value }));
        AddIntensity(beam, EmulationVideoProcessingCatalog.CrtBeamDiffusion, crt.BeamDiffusion,
            value => SetCrt(_configuration.Crt with { BeamDiffusion = value }));
        AddIntensity(beam, EmulationVideoProcessingCatalog.CrtHaloIntensity, crt.HaloIntensity,
            value => SetCrt(_configuration.Crt with { HaloIntensity = value }));
        panel.Children.Add(FramedGroup(EmulationResourceKeys.VideoCrtGroupBeam,
            beam, compact: true));
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
        panel.Children.Add(FramedGroup(EmulationResourceKeys.VideoCrtGroupMask,
            mask, compact: true));
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
        panel.Children.Add(FramedGroup(EmulationResourceKeys.VideoCrtGroupGeometry,
            geometry, compact: true));
        var scanlines = new StackPanel();
        AddToggle(scanlines, EmulationVideoProcessingCatalog.CrtScanlinesEnabled, crt.ScanlinesEnabled, value =>
        {
            SetCrt(_configuration.Crt with { ScanlinesEnabled = value });
            RebuildContent();
        });
        if (crt.ScanlinesEnabled) AddScanlineFields(scanlines, crt);
        panel.Children.Add(FramedGroup(EmulationResourceKeys.VideoCrtGroupScanlines,
            scanlines, compact: true));
        var pattern = new StackPanel();
        AddToggle(pattern, EmulationVideoProcessingCatalog.CrtPatternEnabled, crt.PatternEnabled, value =>
        {
            SetCrt(_configuration.Crt with { PatternEnabled = value });
            RebuildContent();
        });
        if (crt.PatternEnabled) AddPatternFields(pattern, crt);
        panel.Children.Add(FramedGroup(EmulationResourceKeys.VideoCrtGroupInterference,
            pattern, compact: true));
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

    private FrameworkElement CreateProjectionPanel() =>
        EmulationProjectionSettingsBlock.Create(_configuration.Projection,
            update => SetProjection(update(_configuration.Projection)));

}
