using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Functions.Views.Emulation.Storage;
using GWGUI.App.Localization.Extensions;
using GWGUI.Emulation;
using System.Windows;
using System.Windows.Controls;

namespace GWGUI.App.Views.Dialogs.Emulation;

public sealed class CoreOptionsDialog : Window
{
    private readonly IReadOnlyList<EmulationOption> _options;
    private readonly string? _moduleId;
    private readonly Dictionary<string, FrameworkElement> _controls = new(StringComparer.Ordinal);

    public IReadOnlyDictionary<string, string> Values => _options.Where(option => _controls.ContainsKey(option.Key)).ToDictionary(option => option.Key, option => ReadValue(_controls[option.Key]), StringComparer.Ordinal);

    public CoreOptionsDialog(string emulatorName, string emulatorVersion,
        IReadOnlyList<EmulationOption> options, string? moduleId = null)
    {
        _options = options.Where(option => option.IsVisible).ToArray();
        _moduleId = moduleId;
        Title = LocExtension.Get("Emulation.Core.NameOptions");
        Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
        ShowInTaskbar = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Width = 760;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.NoResize;

        var subtitle = string.IsNullOrWhiteSpace(emulatorVersion)
            ? emulatorName : $"{emulatorName} · {emulatorVersion}";
        var body = new StackPanel();
        foreach (var group in _options.GroupBy(option => option.Category ?? string.Empty, StringComparer.Ordinal))
        {
            var fields = group.Select(CreateField).ToArray();
            var content = StorageDialogUi.CompactFields(fields);
            body.Children.Add(StorageDialogUi.IconCard(IconGlyphs.Settings, string.IsNullOrWhiteSpace(group.Key) ? LocExtension.Get("Emulation.Core.NameOptions") : group.Key, content));
        }

        Content = StorageDialogUi.DialogLayout(StorageDialogUi.DialogHeader(IconGlyphs.Settings, Title, subtitle), body, StorageDialogUi.Footer(this, LocExtension.Get("Common.Save")));
    }

    private (string Label, FrameworkElement Control) CreateField(EmulationOption option)
    {
        FrameworkElement control;
        if (option.Values.Count == 0)
        {
            control = new TextBox { Text = option.CurrentValue };
        }
        else
        {
            var choices = option.Values.Select(value => new CoreOptionChoice(value.Value, Resolve(value.Label))).ToArray();
            control = new ComboBox
            {
                ItemsSource = choices,
                SelectedItem = choices.FirstOrDefault(value => value.Value == option.CurrentValue)
                    ?? choices.FirstOrDefault(value => value.Value == option.DefaultValue)
                    ?? choices.FirstOrDefault()
            };
        }
        _controls[option.Key] = control;
        if (!string.IsNullOrWhiteSpace(option.Description))
            control.ToolTip = Resolve(option.Description);
        return (System.Text.RegularExpressions.Regex.Replace(Resolve(option.Name), @"\s*\(restart required\)", string.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase), control);
    }

    private string Resolve(string text) => text.StartsWith("Emulation.", StringComparison.Ordinal) ? LocExtension.GetForModule(_moduleId, text) : text;

    private static string ReadValue(FrameworkElement control) => control switch
    {
        ComboBox { SelectedItem: CoreOptionChoice choice } => choice.Value,
        TextBox text => text.Text,
        _ => string.Empty
    };

    private sealed record CoreOptionChoice(string Value, string Label)
    {
        public override string ToString() => Label;
    }
}
