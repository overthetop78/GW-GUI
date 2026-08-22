using GWGUI.Domain.Settings;
using GWGUI.App.Views.Controls.Shell;
using System.IO;
using System.Windows;
using System.Windows.Controls;



namespace GWGUI.App.Services.Terminal;

public sealed class TerminalPanelController(
    TerminalSection terminal,
    RowDefinition terminalRow,
    GridSplitter splitter,
    AppSettings settings)
{
    public bool IsVisible => terminal.Visibility == Visibility.Visible;
    public double ActualHeight => terminalRow.ActualHeight;

    public string GetCompleteText() =>
        terminal.CommandTextBox.Text + Environment.NewLine + Environment.NewLine + terminal.OutputTextBox.Text;

    public void CopyToClipboard()
    {
        Clipboard.SetText(GetCompleteText());
    }

    public Task ExportAsync(string path) => File.WriteAllTextAsync(path, GetCompleteText());

    public void Toggle() => SetVisibility(!IsVisible);

    public void SetVisibility(bool visible)
    {
        if (!visible && IsVisible && terminalRow.ActualHeight >= 100)
            settings.ConsoleHeight = terminalRow.ActualHeight;

        terminal.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        splitter.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        terminalRow.Height = visible
            ? new GridLength(Math.Max(100, settings.ConsoleHeight))
            : new GridLength(0);
    }
}
