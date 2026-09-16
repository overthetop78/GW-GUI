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
