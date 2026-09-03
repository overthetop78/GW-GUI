using GWGUI.App.Constants.Localization;
using GWGUI.App.Localization.Extensions;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Dictionaries;
using GWGUI.Emulation.Enums;
using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace GWGUI.App.Views.Controls.Emulation.Options;

internal static class EmulationImageRestorationSettingsBlock
{
    private static readonly int[] DeditheringValues = [0, 33, 67, 100];

    internal static FrameworkElement Create(EmulationImageRestorationConfiguration restoration,
        Action<EmulationImageRestorationConfiguration> changed)
    {
        var current = restoration;
        var block = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
        block.Children.Add(new TextBlock
        {
            Text = LocExtension.Get(EmulationResourceKeys.VideoRestorationSettings),
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 12)
        });

        var continuous = new UniformGrid { Columns = 3 };
        continuous.Children.Add(VerticalIntensity(EmulationVideoProcessingCatalog.Denoising,
            restoration.Denoising, value => Publish(current with { Denoising = value })));
        continuous.Children.Add(VerticalIntensity(EmulationVideoProcessingCatalog.Debanding,
            restoration.Debanding, value => Publish(current with { Debanding = value })));
        continuous.Children.Add(VerticalIntensity(EmulationVideoProcessingCatalog.DetailRecovery,
            restoration.DetailRecovery, value => Publish(current with { DetailRecovery = value }),
            EmulationResourceKeys.VideoParameterFineDetails));
        block.Children.Add(continuous);

        var choices = new Grid { Margin = new Thickness(0, 14, 0, 0) };
        choices.ColumnDefinitions.Add(new ColumnDefinition());
        choices.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var dedithering = Dedithering(restoration.Dedithering,
            value => Publish(current with { Dedithering = value }));
        dedithering.Margin = new Thickness(0, 0, 18, 0);
        choices.Children.Add(dedithering);
        var deinterlacing = Deinterlacing(restoration.Deinterlacing,
            value => Publish(current with { Deinterlacing = value }));
        Grid.SetColumn(deinterlacing, 1);
        choices.Children.Add(deinterlacing);
        block.Children.Add(choices);
        return block;

        void Publish(EmulationImageRestorationConfiguration value)
        {
            current = value;
            changed(value);
        }
    }

    private static FrameworkElement VerticalIntensity(string id, int value, Action<int> changed,
        string? labelResourceKey = null)
    {
        var column = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(6, 0, 6, 0)
        };
        column.Children.Add(Label(labelResourceKey
            ?? EmulationVideoProcessingCatalog.ParameterResourceKeys[id]));
        var slider = new Slider
        {
            Orientation = Orientation.Vertical,
            Minimum = EmulationVideoProcessingLimits.IntensityMinimum,
            Maximum = EmulationVideoProcessingLimits.IntensityMaximum,
            Value = value,
            TickFrequency = 1,
            IsSnapToTickEnabled = true,
            Height = 150,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        AutomationProperties.SetAutomationId(slider, id);
        var displayedValue = new TextBlock
        {
            Text = value.ToString(CultureInfo.CurrentCulture),
            MinWidth = 36,
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 6, 0, 0)
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

    private static FrameworkElement Dedithering(int value, Action<int> changed)
    {
        var column = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            MinWidth = 0
        };
        AutomationProperties.SetAutomationId(column, "Video.Dedithering.Block");
        column.Children.Add(Label(EmulationResourceKeys.VideoParameterDedithering,
            HorizontalAlignment.Left));
        var slider = new Slider
        {
            Minimum = 0,
            Maximum = DeditheringValues.Length - 1,
            Value = ClosestDeditheringStep(value),
            TickFrequency = 1,
            TickPlacement = TickPlacement.BottomRight,
            IsSnapToTickEnabled = true,
            Margin = new Thickness(4, 3, 4, 0)
        };
        AutomationProperties.SetAutomationId(slider, EmulationVideoProcessingCatalog.Dedithering);

        var tickBar = new TickBar
        {
            Minimum = 0,
            Maximum = DeditheringValues.Length - 1,
            TickFrequency = 1,
            Placement = TickBarPlacement.Bottom,
            Height = 6,
            Margin = new Thickness(8, -5, 8, 0),
            Fill = SystemColors.ControlDarkBrush
        };
        AutomationProperties.SetAutomationId(tickBar, "Video.Dedithering.LevelTicks");

        var levels = new Grid { ClipToBounds = true };
        for (var index = 0; index < DeditheringValues.Length; index++)
            levels.ColumnDefinitions.Add(new ColumnDefinition());
        var resourceKeys = new[]
        {
            EmulationResourceKeys.VideoRestorationLevelNone,
            EmulationResourceKeys.VideoRestorationLevelLight,
            EmulationResourceKeys.VideoRestorationLevelMedium,
            EmulationResourceKeys.VideoRestorationLevelStrong
        };
        for (var index = 0; index < resourceKeys.Length; index++)
        {
            var label = new TextBlock
            {
                Text = LocExtension.Get(resourceKeys[index]),
                FontSize = 11,
                TextAlignment = TextAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            AutomationProperties.SetAutomationId(label,
                $"Video.Dedithering.Level.{index}");
            Grid.SetColumn(label, index);
            levels.Children.Add(label);
        }
        slider.ValueChanged += (_, _) => changed(DeditheringValues[(int)slider.Value]);
        column.Children.Add(slider);
        column.Children.Add(tickBar);
        column.Children.Add(levels);
        return column;
    }

    private static FrameworkElement Deinterlacing(EmulationDeinterlacingMode selected,
        Action<EmulationDeinterlacingMode> changed)
    {
        var column = new StackPanel { Width = 220 };
        column.Children.Add(Label(EmulationResourceKeys.VideoParameterDeinterlacing,
            HorizontalAlignment.Left));
        var choices = EmulationVideoProcessingCatalog.DeinterlacingResourceKeys
            .Select(choice => new Choice<EmulationDeinterlacingMode>(choice.Key,
                LocExtension.Get(choice.Value))).ToArray();
        var selector = new ComboBox
        {
            ItemsSource = choices,
            DisplayMemberPath = nameof(Choice<EmulationDeinterlacingMode>.DisplayName),
            SelectedItem = choices.First(choice => choice.Value == selected),
            Width = 220
        };
        AutomationProperties.SetAutomationId(selector, EmulationVideoProcessingCatalog.Deinterlacing);
        selector.SelectionChanged += (_, _) =>
        {
            if (selector.SelectedItem is Choice<EmulationDeinterlacingMode> choice)
                changed(choice.Value);
        };
        column.Children.Add(selector);
        return column;
    }

    private static TextBlock Label(string resourceKey,
        HorizontalAlignment alignment = HorizontalAlignment.Center) => new()
    {
        Text = LocExtension.Get(resourceKey),
        HorizontalAlignment = alignment,
        TextAlignment = TextAlignment.Center,
        Margin = new Thickness(0, 0, 0, 6)
    };

    private static int ClosestDeditheringStep(int value) =>
        Math.Clamp((int)Math.Round(value / 100d * (DeditheringValues.Length - 1)),
            0, DeditheringValues.Length - 1);

    private sealed record Choice<T>(T Value, string DisplayName) where T : struct, Enum;
}