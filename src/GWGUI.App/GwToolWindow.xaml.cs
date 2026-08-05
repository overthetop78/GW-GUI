using System.Windows;
using System.Windows.Controls;
using GWGUI.Domain.Commands;
using GWGUI.Domain.Hardware;
using GWGUI.Infrastructure.Processes;
using GWGUI.App.Localization;

namespace GWGUI.App;

public partial class GwToolWindow : Window
{
    private readonly string _executable;
    private readonly string _verb;
    private readonly string? _device;
    private readonly string? _drive;
    private readonly IGreaseweazleRunner _runner = new GreaseweazleRunner();
    private CancellationTokenSource? _cancellation;
    private readonly Dictionary<string, TextBox> _fields = [];
    private readonly Dictionary<string, CheckBox> _checks = [];

    public GwToolWindow(string executable, string verb, string? device = null, string? drive = null)
    {
        InitializeComponent();
        _executable = executable;
        _verb = verb;
        _device = device;
        _drive = drive;
        Heading.Text = Title = TitleFor(verb);
        CreateParameters(); UpdateCommand();
    }

    private void CreateParameters()
    {
        switch (_verb)
        {
            case "rpm": AddField("nr", L("Tool.Field.Measurements"), "1"); break;
            case "seek": AddField("cylinder", L("Tool.Field.Cylinder"), "0"); AddCheck("force", L("Tool.Field.Extreme")); AddCheck("motor-on", L("Tool.Field.MotorOn")); break;
            case "pin": AddField("pin", L("Tool.Field.Pin"), "26"); AddCheck("set", L("Tool.Field.SetPin")); AddCheck("high", L("Tool.Field.High")); break;
            case "delays":
                AddOptionalField("select", L("Tool.Field.Select"), "10"); AddOptionalField("step", L("Tool.Field.Step"), "3000"); AddOptionalField("settle", L("Tool.Field.Settle"), "15"); AddOptionalField("motor", L("Tool.Field.Motor"), "750"); AddOptionalField("watchdog", L("Tool.Field.Watchdog"), "10000"); AddOptionalField("pre-write", L("Tool.Field.PreWrite"), "15"); AddOptionalField("post-write", L("Tool.Field.PostWrite"), "15"); AddOptionalField("index-mask", L("Tool.Field.IndexMask"), "15"); break;
            case "update": AddCheck("bootloader", L("Tool.Field.Bootloader")); break;
        }
    }

    private void AddField(string key, string label, string value)
    {
        var panel = new StackPanel { Margin = new Thickness(0, 0, 14, 8) }; panel.Children.Add(new TextBlock { Text = label });
        var text = new TextBox { Text = value, Width = 150 }; text.TextChanged += (_, _) => UpdateCommand(); panel.Children.Add(text); ParametersPanel.Children.Add(panel); _fields[key] = text;
    }

    private void AddOptionalField(string key, string label, string value)
    {
        var panel = new StackPanel { Margin = new Thickness(0, 0, 14, 8) }; var check = new CheckBox { Content = label }; check.Checked += (_, _) => UpdateCommand(); check.Unchecked += (_, _) => UpdateCommand(); panel.Children.Add(check);
        var text = new TextBox { Text = value, Width = 145 }; text.TextChanged += (_, _) => UpdateCommand(); panel.Children.Add(text); ParametersPanel.Children.Add(panel); _checks[key] = check; _fields[key] = text;
    }

    private void AddCheck(string key, string label)
    {
        var check = new CheckBox { Content = label, Margin = new Thickness(0, 8, 16, 8) }; check.Checked += (_, _) => UpdateCommand(); check.Unchecked += (_, _) => UpdateCommand(); ParametersPanel.Children.Add(check); _checks[key] = check;
    }

    private GwCommand BuildCommand()
    {
        var args = new List<string>();
        switch (_verb)
        {
            case "rpm": args.AddRange(["--nr", _fields["nr"].Text]); break;
            case "seek": args.Add(_fields["cylinder"].Text); if (Checked("force")) args.Add("--force"); if (Checked("motor-on")) args.Add("--motor-on"); break;
            case "pin": args.Add(Checked("set") ? "set" : "get"); args.Add(_fields["pin"].Text); if (Checked("set")) args.Add(Checked("high") ? "H" : "L"); break;
            case "delays": foreach (var key in _fields.Keys) if (Checked(key)) args.AddRange(["--" + key, _fields[key].Text]); break;
            case "update": if (Checked("bootloader")) args.Add("--bootloader"); break;
        }
        if (!string.IsNullOrWhiteSpace(_device)) args.AddRange(["--device", _device]);
        if (!string.IsNullOrWhiteSpace(_drive) && _verb is "rpm" or "seek") args.AddRange(["--drive", _drive]);
        return new GwCommand(_executable, _verb, args);
    }

    private bool Checked(string key) => _checks.GetValueOrDefault(key)?.IsChecked == true;
    private void UpdateCommand() { if (CommandText is not null) CommandText.Text = BuildCommand().ToDisplayString(); }

    private async void Execute_Click(object sender, RoutedEventArgs e)
    {
        if (_runner.IsRunning) { _cancellation?.Cancel(); return; }
        _cancellation = new CancellationTokenSource();
        ExecuteButton.Content = LocExtension.Get("Common.Stop");
        RawOutput.Clear();
        Summary.Text = L("Tool.Running");
        var progress = new Progress<GwOutputLine>(line => { RawOutput.AppendText(line.Text + Environment.NewLine); RawOutput.ScrollToEnd(); });
        try
        {
            if (_verb == "update" && Checked("bootloader") && MessageBox.Show(this, L("Tool.BootloaderWarning"), "Bootloader", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            var command = BuildCommand();
            var result = await _runner.RunAsync(command, progress, _cancellation.Token);
            if (_verb == "info")
            {
                var info = GwInfoParser.Parse(string.Join(Environment.NewLine, result.Output.Select(x => x.Text)));
                Summary.Text = $"{info.Model ?? L("Tool.ControllerFallback")} · {info.FirmwareVersion ?? L("Tool.FirmwareUnknown")} · {info.Port ?? L("Tool.PortUnknown")}" + (info.HasNetworkWarning ? "\n" + L("Tool.NetworkWarning") : "");
            }
            else Summary.Text = result.IsSuccess ? L("Operation.Succeeded") : result.WasCancelled ? L("Operation.Cancelled") : LocExtension.Get("Operation.ExitCode", result.ExitCode);
        }
        catch (Exception exception) { Summary.Text = exception.Message; }
        finally { ExecuteButton.Content = LocExtension.Get("Common.Execute"); _cancellation.Dispose(); _cancellation = null; }
    }

    private static string L(string key) => LocExtension.Get(key);
    private static string TitleFor(string verb) => verb switch
    {
        "info" => L("Tool.Title.Info"), "bandwidth" => L("Tool.Title.Bandwidth"), "rpm" => L("Tool.Title.Rpm"), "seek" => L("Tool.Title.Seek"),
        "pin" => L("Tool.Title.Pin"), "reset" => L("Tool.Title.Reset"), "delays" => L("Tool.Title.Delays"), "update" => L("Tool.Title.Update"), _ => verb
    };
}
