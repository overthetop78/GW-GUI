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

internal static class EmulationTemporalEffectsSettingsBlock
{
    internal static FrameworkElement Create(EmulationTemporalVideoConfiguration temporal,
        Action<EmulationTemporalVideoConfiguration> changed)
    {
        var current = temporal;
        var block = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
        block.Children.Add(new TextBlock
        {
            Text = LocExtension.Get(EmulationResourceKeys.VideoTemporalSettings),
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 12)
        });

        var continuous = new StackPanel();
        continuous.Children.Add(HorizontalIntensity(
            EmulationVideoProcessingCatalog.GeneralPersistence,
            temporal.GeneralPersistence,
            value => Publish(current with { GeneralPersistence = value })));
        continuous.Children.Add(HorizontalIntensity(
            EmulationVideoProcessingCatalog.MotionBlur,
            temporal.MotionBlur,
            value => Publish(current with { MotionBlur = value })));
        continuous.Children.Add(HorizontalIntensity(
            EmulationVideoProcessingCatalog.Flicker,
            temporal.Flicker,
            value => Publish(current with { Flicker = value })));
        block.Children.Add(continuous);

        var choices = new StackPanel { Margin = new Thickness(0, 12, 0, 0) };

        var interlacing = new StackPanel();
        var interlacingToggle = Toggle(EmulationVideoProcessingCatalog.Interlacing,
            temporal.Interlacing > 0);
        var visibility = HorizontalIntensity(
            EmulationVideoProcessingCatalog.InterlacingVisibility,
            temporal.InterlacingVisibility,
            value => Publish(current with { InterlacingVisibility = value }));
        visibility.IsEnabled = interlacingToggle.IsChecked == true;
        interlacingToggle.Checked += (_, _) =>
        {
            visibility.IsEnabled = true;
            Publish(current with { Interlacing = 100 });
        };
        interlacingToggle.Unchecked += (_, _) =>
        {
            visibility.IsEnabled = false;
            Publish(current with { Interlacing = 0 });
        };
        interlacing.Children.Add(interlacingToggle);
        interlacing.Children.Add(visibility);
        choices.Children.Add(interlacing);

        var blackFrames = new StackPanel { Margin = new Thickness(0, 12, 0, 0) };
        var blackFrameToggle = Toggle(EmulationVideoProcessingCatalog.BlackFrameInsertion,
            temporal.BlackFrameInsertion);
        blackFrameToggle.Checked += (_, _) =>
            Publish(current with { BlackFrameInsertion = true });
        blackFrameToggle.Unchecked += (_, _) =>
            Publish(current with { BlackFrameInsertion = false });
        blackFrames.Children.Add(blackFrameToggle);
        choices.Children.Add(blackFrames);
        block.Children.Add(choices);
        return block;

        void Publish(EmulationTemporalVideoConfiguration value)
        {
            current = value;
            changed(value);
        }
    }

    private static FrameworkElement HorizontalIntensity(string id, int value, Action<int> changed)
    {
        var row = new Grid { Margin = new Thickness(0, 4, 0, 4) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        row.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var label = Label(id, TextAlignment.Left);
        label.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetColumnSpan(label, 2);
        row.Children.Add(label);
        var slider = IntensitySlider(id, value, changed);
        slider.Margin = new Thickness(0, 2, 10, 0);
        Grid.SetRow(slider, 1);
        row.Children.Add(slider);
        var displayedValue = Value(value, slider);
        displayedValue.VerticalAlignment = VerticalAlignment.Center;
        displayedValue.Margin = new Thickness(0);
        Grid.SetRow(displayedValue, 1);
        Grid.SetColumn(displayedValue, 1);
        row.Children.Add(displayedValue);
        return row;
    }

    private static Slider IntensitySlider(string id, int value, Action<int> changed)
    {
        var slider = new Slider
        {
            Minimum = EmulationVideoProcessingLimits.IntensityMinimum,
            Maximum = EmulationVideoProcessingLimits.IntensityMaximum,
            Value = value,
            TickFrequency = 1,
            IsSnapToTickEnabled = true
        };
        AutomationProperties.SetAutomationId(slider, id);
        slider.ValueChanged += (_, _) => changed((int)slider.Value);
        return slider;
    }

    private static TextBlock Value(int initialValue, Slider slider)
    {
        var value = new TextBlock
        {
            Text = initialValue.ToString(CultureInfo.CurrentCulture),
            MinWidth = 30,
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0)
        };
        slider.ValueChanged += (_, _) =>
            value.Text = ((int)slider.Value).ToString(CultureInfo.CurrentCulture);
        return value;
    }

    private static TextBlock Label(string id, TextAlignment alignment) => new()
    {
        Text = LocExtension.Get(EmulationVideoProcessingCatalog.ParameterResourceKeys[id]),
        TextAlignment = alignment,
        TextWrapping = TextWrapping.Wrap,
        HorizontalAlignment = alignment == TextAlignment.Center
            ? HorizontalAlignment.Center : HorizontalAlignment.Stretch,
        Margin = new Thickness(0, 0, 0, 6)
    };

    private static CheckBox Toggle(string id, bool selected)
    {
        var toggle = new CheckBox
        {
            Content = LocExtension.Get(EmulationVideoProcessingCatalog.ParameterResourceKeys[id]),
            IsChecked = selected,
            VerticalContentAlignment = VerticalAlignment.Center
        };
        AutomationProperties.SetAutomationId(toggle, id);
        return toggle;
    }
}
