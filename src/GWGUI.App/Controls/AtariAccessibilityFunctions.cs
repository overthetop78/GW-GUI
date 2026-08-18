using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;

namespace GWGUI.App.Controls;

internal static class AtariAccessibilityFunctions
{
    internal static void Configure(FrameworkElement element, string name, string? helpText = null,
        int? tabIndex = null)
    {
        AutomationProperties.SetName(element, name);
        if (!string.IsNullOrWhiteSpace(helpText)) AutomationProperties.SetHelpText(element, helpText);
        if (tabIndex is not null) KeyboardNavigation.SetTabIndex(element, tabIndex.Value);
    }

    internal static Grid LabeledRow(string label, UIElement editor)
    {
        var row = new Grid { Margin = new Thickness(0, 4, 0, 4) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
        row.ColumnDefinitions.Add(new ColumnDefinition());
        row.Children.Add(new TextBlock
        {
            Text = label,
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0)
        });
        Grid.SetColumn(editor, 1);
        row.Children.Add(editor);
        if (editor is FrameworkElement frameworkElement) Configure(frameworkElement, label);
        return row;
    }

    internal static void ConfigureFlowDirection(FrameworkElement element) =>
        element.FlowDirection = CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft
            ? FlowDirection.RightToLeft
            : FlowDirection.LeftToRight;

}
