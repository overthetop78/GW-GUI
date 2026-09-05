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

internal static class EmulationProjectionSettingsBlock
{
    internal static FrameworkElement Create(EmulationProjectionVideoConfiguration value,
        Action<Func<EmulationProjectionVideoConfiguration, EmulationProjectionVideoConfiguration>> changed)
    {
        var groups = new Grid();
        groups.ColumnDefinitions.Add(new ColumnDefinition());
        groups.ColumnDefinitions.Add(new ColumnDefinition());
        AddGroup(groups, EmulationResourceKeys.VideoProjectionGroupOptics, 0, 0, 1,
            Slider(EmulationVideoProcessingCatalog.ProjectionOpticalBlur, value.OpticalBlur, 100,
                number => changed(current => current with { OpticalBlur = number })),
            Slider(EmulationVideoProcessingCatalog.ProjectionDiffusion, value.Diffusion, 100,
                number => changed(current => current with { Diffusion = number })),
            Slider(EmulationVideoProcessingCatalog.ProjectionConvergence, value.Convergence, 100,
                number => changed(current => current with { Convergence = number })),
            Slider(EmulationVideoProcessingCatalog.ProjectionLightOutput, value.LightOutput, 100,
                number => changed(current => current with { LightOutput = number })));
        AddGroup(groups, EmulationResourceKeys.VideoProjectionGroupScreen, 0, 1, 1,
            Slider(EmulationVideoProcessingCatalog.ProjectionScreenTexture, value.ScreenTexture, 100,
                number => changed(current => current with { ScreenTexture = number })),
            Slider(EmulationVideoProcessingCatalog.ProjectionAmbientLight, value.AmbientLight, 100,
                number => changed(current => current with { AmbientLight = number })),
            Slider(EmulationVideoProcessingCatalog.ProjectionVignette, value.Vignette, 100,
                number => changed(current => current with { Vignette = number })));
        return groups;
    }

    private static void AddGroup(Grid groups, string titleResourceKey, int row, int column,
        int columnSpan, params FrameworkElement[] fields)
    {
        var content = new StackPanel();
        content.Children.Add(new TextBlock
        {
            Text = LocExtension.Get(titleResourceKey), FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 8)
        });
        foreach (var field in fields) content.Children.Add(field);
        var card = new Border
        {
            Child = content,
            Margin = new Thickness(column == 0 ? 0 : 6, row == 0 ? 0 : 6,
                column + columnSpan >= 2 ? 0 : 6, row == 0 ? 6 : 0)
        };
        AutomationProperties.SetAutomationId(card, titleResourceKey);
        card.SetResourceReference(FrameworkElement.StyleProperty,
            ControlVisualConstants.CardStyleResource);
        Grid.SetRow(card, row);
        Grid.SetColumn(card, column);
        Grid.SetColumnSpan(card, columnSpan);
        groups.Children.Add(card);
    }

    private static FrameworkElement Slider(string id, int value, int maximum,
        Action<int> changed)
    {
        var row = new Grid();
        row.ColumnDefinitions.Add(new ColumnDefinition());
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var slider = new Slider
        {
            Minimum = 0, Maximum = maximum, Value = Math.Clamp(value, 0, maximum),
            TickFrequency = maximum == 100 ? 1 : 5, IsSnapToTickEnabled = true
        };
        AutomationProperties.SetAutomationId(slider, id);
        var number = new TextBlock
        {
            Text = value.ToString(CultureInfo.CurrentCulture), MinWidth = 42,
            TextAlignment = TextAlignment.Right, VerticalAlignment = VerticalAlignment.Center,
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
        return Field(id, row);
    }

    private static FrameworkElement Field(string id, FrameworkElement control)
    {
        var field = new StackPanel { Margin = new Thickness(0, 3, 0, 9) };
        field.Children.Add(new TextBlock
        {
            Text = LocExtension.Get(EmulationVideoProcessingCatalog.ParameterResourceKeys[id]),
            Margin = new Thickness(0, 0, 0, 4), TextWrapping = TextWrapping.Wrap
        });
        field.Children.Add(control);
        return field;
    }

}
