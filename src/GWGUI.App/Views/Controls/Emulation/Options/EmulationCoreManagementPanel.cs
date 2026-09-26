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
    internal Button Search { get; } = new() { MinWidth = 150 };
    internal ComboBox Versions { get; } = new() { MinWidth = 320, Visibility = Visibility.Collapsed };
    internal Button Download { get; } = new() { MinWidth = 160, Visibility = Visibility.Collapsed };
    internal TextBlock Installed { get; } = new()
    {
        FontWeight = FontWeights.SemiBold,
        VerticalAlignment = VerticalAlignment.Center
    };
    internal Button Cancel { get; } = new() { MinWidth = 100, Visibility = Visibility.Collapsed };
    internal ProgressBar Progress { get; } = new()
    {
        Height = 6,
        Minimum = EmulationCoreManagementConstants.InitialProgress,
        Maximum = EmulationCoreManagementConstants.CompletedProgress,
        Visibility = Visibility.Collapsed
    };
    internal TextBlock Description { get; } = new() { TextWrapping = TextWrapping.Wrap };
    internal TextBlock Status { get; } = new() { TextWrapping = TextWrapping.Wrap };

    internal EmulationCoreManagementPanel(Func<string, object[], string> localize)
    {
        Search.Content = localize(EmulationCoreManagementConstants.SearchResource, []);
        Download.Content = localize(EmulationCoreManagementConstants.DownloadResource, []);
        Cancel.Content = localize(EmulationCoreManagementConstants.CancelResource, []);
        AutomationProperties.SetName(Emulators,
            localize(EmulationCoreManagementConstants.EmulatorResource, []));
        AutomationProperties.SetName(Search, localize(EmulationCoreManagementConstants.SearchResource, []));
        AutomationProperties.SetName(Versions,
            localize(EmulationCoreManagementConstants.VersionsFoundResource, [0]));
        AutomationProperties.SetName(Download,
            localize(EmulationCoreManagementConstants.DownloadResource, []));
        AutomationProperties.SetName(Status, localize(EmulationCoreManagementConstants.EmulatorResource, []));
        AutomationProperties.SetLiveSetting(Status, AutomationLiveSetting.Assertive);

        var content = new Grid { Margin = new Thickness(16, 12, 16, 12) };
        content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
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
        Installed.Margin = new Thickness(12, 0, 0, 0);
        Installed.Foreground = new SolidColorBrush(ControlVisualConstants.CompatibleForegroundColor);
        Grid.SetColumn(Installed, 2);
        content.Children.Add(Installed);
        Cancel.Margin = new Thickness(12, 0, 0, 0);
        Grid.SetColumn(Cancel, 3);
        content.Children.Add(Cancel);

        Description.Margin = new Thickness(0, 10, 0, 2);
        Description.SetResourceReference(ForegroundProperty, ControlVisualConstants.MutedTextBrushResource);
        Grid.SetRow(Description, 1);
        Grid.SetColumnSpan(Description, 4);
        content.Children.Add(Description);

        var releases = new Grid { Margin = new Thickness(0, 10, 0, 2) };
        releases.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        releases.ColumnDefinitions.Add(new ColumnDefinition());
        releases.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        Search.Margin = new Thickness(0, 0, 12, 0);
        releases.Children.Add(Search);
        Grid.SetColumn(Versions, 1);
        releases.Children.Add(Versions);
        Download.Margin = new Thickness(12, 0, 0, 0);
        Grid.SetColumn(Download, 2);
        releases.Children.Add(Download);
        Grid.SetRow(releases, 2);
        Grid.SetColumnSpan(releases, 4);
        content.Children.Add(releases);

        Status.Margin = new Thickness(0, 4, 0, 6);
        Status.SetResourceReference(ForegroundProperty, ControlVisualConstants.MutedTextBrushResource);
        Grid.SetRow(Status, 3);
        Grid.SetColumnSpan(Status, 4);
        content.Children.Add(Status);
        Grid.SetRow(Progress, 4);
        Grid.SetColumnSpan(Progress, 4);
        content.Children.Add(Progress);

        var card = new Border { Child = content };
        card.SetResourceReference(FrameworkElement.StyleProperty, "Card");
        Content = card;
    }

    internal void SetDescription(string text) => Description.Text = text;

    internal void SetInstalledVersion(string text)
    {
        Installed.Text = text;
    }

    internal void ShowReleases(bool visible)
    {
        Versions.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        Download.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        if (!visible) Versions.ItemsSource = null;
    }

    internal void SetStatus(string text, bool isError = false)
    {
        Status.Text = text;
        Status.Foreground = isError
            ? new SolidColorBrush(EmulationCoreManagementConstants.ErrorText)
            : (Brush)FindResource(ControlVisualConstants.MutedTextBrushResource);
    }
}
