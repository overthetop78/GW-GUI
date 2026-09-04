using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Constants.Localization;
using GWGUI.App.Localization.Extensions;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Dictionaries;

namespace GWGUI.App.Views.Controls.Emulation.Options;

internal static class EmulationPlasmaSettingsBlock
{
    internal static FrameworkElement Create(EmulationPlasmaVideoConfiguration value,
        Action<Func<EmulationPlasmaVideoConfiguration,
            EmulationPlasmaVideoConfiguration>> changed)
    {
        var groups = new Grid();
        groups.ColumnDefinitions.Add(new ColumnDefinition());
        groups.ColumnDefinitions.Add(new ColumnDefinition());
        groups.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        groups.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        AddGroup(groups, EmulationResourceKeys.VideoPlasmaGroupPanelAndCells, 0, 0,
            Slider(EmulationVideoProcessingCatalog.PlasmaCellStructure,
                value.CellStructure, number => changed(current => current with { CellStructure = number })),
            Slider(EmulationVideoProcessingCatalog.PlasmaBlackDepth,
                value.BlackDepth, number => changed(current => current with { BlackDepth = number })));
        AddGroup(groups, EmulationResourceKeys.VideoPlasmaGroupPhosphors, 0, 1,
            Slider(EmulationVideoProcessingCatalog.PlasmaPhosphorIntensity,
                value.PhosphorIntensity,
                number => changed(current => current with { PhosphorIntensity = number })),
            Slider(EmulationVideoProcessingCatalog.PlasmaGammaResponse,
                value.GammaResponse, number => changed(current => current with { GammaResponse = number })));
        AddGroup(groups, EmulationResourceKeys.VideoPlasmaGroupLight, 1, 0,
            Slider(EmulationVideoProcessingCatalog.PlasmaAutomaticBrightnessLimiter,
                value.AutomaticBrightnessLimiter,
                number => changed(current => current with { AutomaticBrightnessLimiter = number })),
            Slider(EmulationVideoProcessingCatalog.PlasmaDiffusion,
                value.Diffusion, number => changed(current => current with { Diffusion = number })));
        AddGroup(groups, EmulationResourceKeys.VideoPlasmaGroupTemporal, 1, 1,
            Slider(EmulationVideoProcessingCatalog.PlasmaTemporalDithering,
                value.TemporalDithering,
                number => changed(current => current with { TemporalDithering = number })),
            Slider(EmulationVideoProcessingCatalog.PlasmaPersistence,
                value.PersistenceIntensity,
                number => changed(current => current with { PersistenceIntensity = number })));
        return groups;
    }

    private static void AddGroup(Grid groups, string titleResourceKey, int row, int column,
        params FrameworkElement[] fields)
    {
        var content = new StackPanel();
        content.Children.Add(new TextBlock
        {
            Text = LocExtension.Get(titleResourceKey),
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 8)
        });
        foreach (var field in fields) content.Children.Add(field);
        var card = new Border
        {
            Child = content,
            Margin = new Thickness(column == 0 ? 0 : 6, row == 0 ? 0 : 6,
                column == 0 ? 6 : 0, row == 0 ? 6 : 0)
        };
        AutomationProperties.SetAutomationId(card, titleResourceKey);
        card.SetResourceReference(FrameworkElement.StyleProperty,
            ControlVisualConstants.CardStyleResource);
        Grid.SetRow(card, row);
        Grid.SetColumn(card, column);
        groups.Children.Add(card);
    }

    private static FrameworkElement Slider(string id, int value, Action<int> changed)
    {
        var row = new Grid();
        row.ColumnDefinitions.Add(new ColumnDefinition());
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var slider = new Slider
        {
            Minimum = 0,
            Maximum = 100,
            Value = Math.Clamp(value, 0, 100),
            TickFrequency = 1,
            IsSnapToTickEnabled = true
        };
        AutomationProperties.SetAutomationId(slider, id);
        var number = new TextBlock
        {
            Text = value.ToString(CultureInfo.CurrentCulture),
            MinWidth = 36,
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 0, 0)
        };
        slider.ValueChanged += (_, _) =>
        {
            var current = (int)slider.Value;
            number.Text = current.ToString(CultureInfo.CurrentCulture);
            changed(current);
        };
        row.Children.Add(slider);
        Grid.SetColumn(number, 1);
        row.Children.Add(number);
        var field = new StackPanel { Margin = new Thickness(0, 3, 0, 9) };
        field.Children.Add(new TextBlock
        {
            Text = LocExtension.Get(EmulationVideoProcessingCatalog.ParameterResourceKeys[id]),
            Margin = new Thickness(0, 0, 0, 4),
            TextWrapping = TextWrapping.Wrap
        });
        field.Children.Add(row);
        return field;
    }
}
