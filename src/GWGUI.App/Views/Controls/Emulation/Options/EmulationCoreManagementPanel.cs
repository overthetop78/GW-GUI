using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Constants.Emulation;

namespace GWGUI.App.Views.Controls.Emulation.Options;

internal sealed class EmulationCoreManagementPanel : UserControl
{
    internal ComboBox Emulators { get; } = new() { MinWidth = 300 };
    internal Button Install { get; } = new() { MinWidth = 110, Visibility = Visibility.Collapsed };
    internal TextBlock Installed { get; } = new()
    {
        FontWeight = FontWeights.SemiBold,
        VerticalAlignment = VerticalAlignment.Center,
        Visibility = Visibility.Collapsed
    };
    internal Button Cancel { get; } = new() { MinWidth = 100, Visibility = Visibility.Collapsed };
    internal ProgressBar Progress { get; } = new()
    {
        Height = 6,
        Minimum = EmulationCoreManagementConstants.InitialProgress,
        Maximum = EmulationCoreManagementConstants.CompletedProgress,
        Visibility = Visibility.Collapsed
    };
    internal TextBlock Status { get; } = new() { TextWrapping = TextWrapping.Wrap };

    internal EmulationCoreManagementPanel(Func<string, object[], string> localize)
    {
        Install.Content = localize("Emulation.Core.Install", []);
        Installed.Text = localize("Emulation.Core.InstalledBadge", []);
        Cancel.Content = localize(EmulationCoreManagementConstants.CancelResource, []);
        AutomationProperties.SetName(Emulators,
            localize(EmulationCoreManagementConstants.EmulatorResource, []));
        AutomationProperties.SetName(Install, localize("Emulation.Core.Install", []));
        AutomationProperties.SetName(Status, localize(EmulationCoreManagementConstants.EmulatorResource, []));
        AutomationProperties.SetLiveSetting(Status, AutomationLiveSetting.Assertive);

        var content = new Grid { Margin = new Thickness(16, 12, 16, 12) };
        content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        content.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(360) });
        content.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        content.ColumnDefinitions.Add(new ColumnDefinition());

        content.Children.Add(new TextBlock
        {
            Text = localize(EmulationCoreManagementConstants.EmulatorResource, []),
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 12, 0)
        });
        Grid.SetColumn(Emulators, 1);
        content.Children.Add(Emulators);
        Install.Margin = new Thickness(12, 0, 0, 0);
        Grid.SetColumn(Install, 2);
        content.Children.Add(Install);
        Installed.Margin = new Thickness(12, 0, 0, 0);
        Installed.Foreground = new SolidColorBrush(ControlVisualConstants.CompatibleForegroundColor);
        Grid.SetColumn(Installed, 2);
        content.Children.Add(Installed);
        Cancel.Margin = new Thickness(12, 0, 0, 0);
        Grid.SetColumn(Cancel, 3);
        content.Children.Add(Cancel);

        Status.Margin = new Thickness(0, 10, 0, 6);
        Status.SetResourceReference(ForegroundProperty, ControlVisualConstants.MutedTextBrushResource);
        Grid.SetRow(Status, 1);
        Grid.SetColumnSpan(Status, 4);
        content.Children.Add(Status);
        Grid.SetRow(Progress, 2);
        Grid.SetColumnSpan(Progress, 4);
        content.Children.Add(Progress);

        var card = new Border { Child = content };
        card.SetResourceReference(FrameworkElement.StyleProperty, "Card");
        Content = card;
    }

    internal void ShowInstallation(bool installed)
    {
        Install.Visibility = installed ? Visibility.Collapsed : Visibility.Visible;
        Installed.Visibility = installed ? Visibility.Visible : Visibility.Collapsed;
    }

    internal void SetStatus(string text, bool isError = false)
    {
        Status.Text = text;
        Status.Foreground = isError
            ? new SolidColorBrush(EmulationCoreManagementConstants.ErrorText)
            : (Brush)FindResource(ControlVisualConstants.MutedTextBrushResource);
    }
}
