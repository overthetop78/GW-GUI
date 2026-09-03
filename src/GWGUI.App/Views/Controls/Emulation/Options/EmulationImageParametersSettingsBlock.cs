using GWGUI.App.Constants.Localization;
using GWGUI.App.Localization.Extensions;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Dictionaries;
using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace GWGUI.App.Views.Controls.Emulation.Options;

internal static class EmulationImageParametersSettingsBlock
{
    internal static FrameworkElement Create(EmulationImageAdjustments adjustments,
        Action<Func<EmulationImageAdjustments, EmulationImageAdjustments>> changed)
    {
        var block = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
        block.Children.Add(new TextBlock
        {
            Text = LocExtension.Get(EmulationResourceKeys.VideoGeneralSettings),
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 12)
        });
        var sliders = new UniformGrid { Columns = 5, HorizontalAlignment = HorizontalAlignment.Stretch };
        sliders.Children.Add(VerticalSlider(EmulationVideoProcessingCatalog.Brightness,
            adjustments.Brightness, value => changed(current => current with { Brightness = value })));
        sliders.Children.Add(VerticalSlider(EmulationVideoProcessingCatalog.Contrast,
            adjustments.Contrast, value => changed(current => current with { Contrast = value })));
        sliders.Children.Add(VerticalSlider(EmulationVideoProcessingCatalog.Gamma,
            adjustments.Gamma, value => changed(current => current with { Gamma = value })));
        sliders.Children.Add(VerticalSlider(EmulationVideoProcessingCatalog.Saturation,
            adjustments.Saturation, value => changed(current => current with { Saturation = value })));
        sliders.Children.Add(VerticalSlider(EmulationVideoProcessingCatalog.Sharpness,
            adjustments.Sharpness, value => changed(current => current with { Sharpness = value })));
        block.Children.Add(sliders);
        return block;

    }

    private static FrameworkElement VerticalSlider(string id, int value, Action<int> changed)
    {
        var column = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(8, 0, 8, 0)
        };
        column.Children.Add(new TextBlock
        {
            Text = LocExtension.Get(EmulationVideoProcessingCatalog.ParameterResourceKeys[id]),
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 8)
        });
        var slider = new Slider
        {
            Orientation = Orientation.Vertical,
            Minimum = EmulationVideoProcessingLimits.AdjustmentMinimum,
            Maximum = EmulationVideoProcessingLimits.AdjustmentMaximum,
            Value = value,
            TickFrequency = 1,
            IsSnapToTickEnabled = true,
            Height = 220,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        AutomationProperties.SetAutomationId(slider, id);
        var displayedValue = new TextBlock
        {
            Text = value.ToString(CultureInfo.CurrentCulture),
            MinWidth = 36,
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 8, 0, 0)
        };
        slider.ValueChanged += (_, _) =>
        {
            var number = (int)slider.Value;
            displayedValue.Text = number.ToString(CultureInfo.CurrentCulture);
            changed(number);
        };
        column.Children.Add(slider);
        column.Children.Add(displayedValue);
        return column;
    }
}