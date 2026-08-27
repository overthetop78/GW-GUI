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
        var container = new FrameworkElementFactory(typeof(Border), "Container");
        var text = new FrameworkElementFactory(typeof(TextBlock), "Text");
        text.SetBinding(TextBlock.TextProperty, new Binding(nameof(EmulationMachineChoice.DisplayName)));
        container.AppendChild(text);

        var configured = new DataTrigger
        {
            Binding = new Binding(nameof(EmulationMachineChoice.HasSavedConfiguration)),
            Value = true
        };
        configured.Setters.Add(new Setter(Border.BackgroundProperty,
            new SolidColorBrush(EmulationMachineChoiceVisualConstants.ConfiguredBackground), "Container"));
        configured.Setters.Add(new Setter(TextBlock.ForegroundProperty,
            new SolidColorBrush(EmulationMachineChoiceVisualConstants.ConfiguredForeground), "Text"));
        configured.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.Bold, "Text"));

        var template = new DataTemplate(typeof(EmulationMachineChoice)) { VisualTree = container };
        template.Triggers.Add(configured);
        return template;
    }
}
