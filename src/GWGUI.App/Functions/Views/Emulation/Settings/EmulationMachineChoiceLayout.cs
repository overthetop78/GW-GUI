using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Contracts.Emulation.Machine;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;


namespace GWGUI.App.Functions.Views.Emulation.Settings;

internal static class EmulationMachineChoiceLayout
{
    internal static DataTemplate CreateTemplate()
    {
        var text = new FrameworkElementFactory(typeof(TextBlock), "Text");
        text.SetBinding(TextBlock.TextProperty, new Binding(nameof(EmulationMachineChoice.DisplayName)));

        var image = new FrameworkElementFactory(typeof(Image));
        image.SetBinding(Image.SourceProperty, new Binding(nameof(EmulationMachineChoice.Image)));
        image.SetValue(FrameworkElement.MaxHeightProperty, EmulationMachineChoiceVisualConstants.MaximumImageHeight);
        image.SetBinding(FrameworkElement.HeightProperty,
            new Binding(nameof(FrameworkElement.ActualHeight)) { ElementName = text.Name });
        image.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Left);
        image.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        image.SetValue(Image.StretchProperty, Stretch.Uniform);
        image.SetValue(FrameworkElement.MarginProperty,
            new Thickness(0, 0, EmulationMachineChoiceVisualConstants.ImageNameSpacing, 0));
        text.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        var row = new FrameworkElementFactory(typeof(DockPanel));
        row.AppendChild(image);
        row.AppendChild(text);

        var configured = new DataTrigger
        {
            Binding = new Binding(nameof(EmulationMachineChoice.HasSavedConfiguration)),
            Value = true
        };
        configured.Setters.Add(new Setter(TextBlock.ForegroundProperty,
            new SolidColorBrush(EmulationMachineChoiceVisualConstants.ConfiguredForeground), "Text"));
        configured.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.Bold, "Text"));

        var template = new DataTemplate(typeof(EmulationMachineChoice)) { VisualTree = row };
        template.Triggers.Add(configured);
        return template;
    }

    internal static Style CreateListItemContainerStyle()
    {
        var style = new Style(typeof(ListBoxItem),
            (Style)Application.Current.FindResource(typeof(ListBoxItem)));
        style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
        style.Setters.Add(new Setter(FrameworkElement.MarginProperty, new Thickness(0, 0, 0, 6)));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(12, 10, 12, 10)));
        var configured = new DataTrigger
        {
            Binding = new Binding(nameof(EmulationMachineChoice.HasSavedConfiguration)),
            Value = true
        };
        AddConfiguredSetters(configured.Setters);
        style.Triggers.Add(configured);
        return style;
    }

    private static void AddConfiguredSetters(SetterBaseCollection setters)
    {
        setters.Add(new Setter(Control.BackgroundProperty,
            new SolidColorBrush(EmulationMachineChoiceVisualConstants.ConfiguredBackground)));
        setters.Add(new Setter(Control.ForegroundProperty,
            new SolidColorBrush(EmulationMachineChoiceVisualConstants.ConfiguredForeground)));
        setters.Add(new Setter(Control.BorderBrushProperty,
            new SolidColorBrush(EmulationMachineChoiceVisualConstants.ConfiguredBorder)));
        setters.Add(new Setter(Control.FontWeightProperty, FontWeights.Bold));
    }
}
