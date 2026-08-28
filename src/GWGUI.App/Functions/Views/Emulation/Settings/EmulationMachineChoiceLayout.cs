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

        var configured = new DataTrigger
        {
            Binding = new Binding(nameof(EmulationMachineChoice.HasSavedConfiguration)),
            Value = true
        };
        configured.Setters.Add(new Setter(TextBlock.ForegroundProperty,
            new SolidColorBrush(EmulationMachineChoiceVisualConstants.ConfiguredForeground), "Text"));
        configured.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.Bold, "Text"));

        var template = new DataTemplate(typeof(EmulationMachineChoice)) { VisualTree = text };
        template.Triggers.Add(configured);
        return template;
    }

    internal static Style CreateItemContainerStyle()
    {
        var style = new Style(typeof(ComboBoxItem),
            (Style)Application.Current.FindResource(typeof(ComboBoxItem)));
        style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
        var configured = new DataTrigger
        {
            Binding = new Binding(nameof(EmulationMachineChoice.HasSavedConfiguration)),
            Value = true
        };
        AddConfiguredSetters(configured.Setters);
        style.Triggers.Add(configured);
        return style;
    }

    internal static Style CreateComboBoxStyle()
    {
        var style = new Style(typeof(ComboBox),
            (Style)Application.Current.FindResource(typeof(ComboBox)));
        var configured = new DataTrigger
        {
            Binding = new Binding("SelectedItem.HasSavedConfiguration")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.Self)
            },
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
