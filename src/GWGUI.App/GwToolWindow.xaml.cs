using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using GWGUI.Domain.Commands;
using GWGUI.Domain.Hardware;
using GWGUI.Domain.Maintenance;
using GWGUI.Infrastructure.Processes;
using GWGUI.App.Localization;
using GWGUI.App.Services;

namespace GWGUI.App;

public partial class GwToolWindow : Window
{
    private readonly string _executable;
    private readonly string _verb;
    private readonly string? _device;
    private readonly string? _drive;
    private readonly IGreaseweazleRunner _runner;
    private readonly IGwCommandBuilder _commandBuilder;
    private readonly ConsoleLogSession? _consoleLog;
    private CancellationTokenSource? _cancellation;
    private readonly Dictionary<string, TextBox> _fields = [];
    private readonly Dictionary<string, CheckBox> _checks = [];

    public GwToolWindow(string executable, string verb, string? device = null, string? drive = null, IGreaseweazleRunner? runner = null, IGwCommandBuilder? commandBuilder = null, ConsoleLogSession? consoleLog = null)
    {
        InitializeComponent();
        _executable = executable;
        _verb = verb;
        _device = device;
        _drive = drive;
        _runner = runner ?? new GreaseweazleRunner();
        _commandBuilder = commandBuilder ?? new GwCommandBuilder();
        _consoleLog = consoleLog;
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
            case "align":
                AddField("tracks", L("Tool.Field.AlignTracks"), "c=40:h=0-1"); AddField("revs", L("Read.Revolutions"), "3"); AddField("reads", L("Tool.Field.AlignReads"), "10");
                AddOptionalField("format", L("Tool.Field.AlignFormat"), "ibm.720"); AddOptionalField("diskdefs", L("Advanced.DiskDefs"), "diskdefs.cfg");
                AddCheck("raw", L("Tool.Field.AlignRaw")); AddOptionalField("fake-index", L("Advanced.FakeIndex"), "300rpm"); AddCheck("hard-sectors", L("Advanced.HardSectors"));
                AddOptionalField("adjust-speed", L("Advanced.AdjustSpeed"), "300rpm"); AddOptionalField("pll", L("Advanced.Pll"), "period=5:phase=60");
                AddOptionalField("densel", L("Advanced.DensityPin"), "H"); AddCheck("gen-tg43", L("Advanced.Tg43")); AddCheck("reverse", L("Advanced.Reverse")); break;
            case "update": AddCheck("bootloader", L("Tool.Field.Bootloader")); break;
        }
    }

    private void AddField(string key, string label, string value)
    {
        var panel = new StackPanel { Margin = new Thickness(0, 0, 14, 8) }; panel.Children.Add(new TextBlock { Text = label });
        var text = new TextBox { Text = value, Width = 150 }; AutomationProperties.SetName(text, label); text.TextChanged += (_, _) => UpdateCommand(); panel.Children.Add(text); ParametersPanel.Children.Add(panel); _fields[key] = text;
    }

    private void AddOptionalField(string key, string label, string value)
    {
        var panel = new StackPanel { Margin = new Thickness(0, 0, 14, 8) }; var check = new CheckBox { Content = label }; check.Checked += (_, _) => UpdateCommand(); check.Unchecked += (_, _) => UpdateCommand(); panel.Children.Add(check);
        var text = new TextBox { Text = value, Width = 145 }; AutomationProperties.SetName(text, label); text.TextChanged += (_, _) => UpdateCommand(); panel.Children.Add(text); ParametersPanel.Children.Add(panel); _checks[key] = check; _fields[key] = text;
    }

    private void AddCheck(string key, string label)
    {
        var check = new CheckBox { Content = label, Margin = new Thickness(0, 8, 16, 8) }; check.Checked += (_, _) => UpdateCommand(); check.Unchecked += (_, _) => UpdateCommand(); ParametersPanel.Children.Add(check); _checks[key] = check;
    }

    private GwCommand BuildCommand()
    {
        return _commandBuilder.BuildTool(new(_executable, _verb, _fields.ToDictionary(x => x.Key, x => x.Value.Text), _checks.Where(x => x.Value.IsChecked == true).Select(x => x.Key).ToHashSet(), _device, _drive));
    }

    private bool Checked(string key) => _checks.GetValueOrDefault(key)?.IsChecked == true;
    private void UpdateCommand()
    {
        if (CommandText is null) return;
        try { CommandText.Text = BuildCommand().ToDisplayString(); ExecuteButton.IsEnabled = true; Summary.Text = L("Tool.Ready"); }
        catch (ArgumentException) { CommandText.Text = L("Tool.InvalidParameters"); ExecuteButton.IsEnabled = false; Summary.Text = L("Tool.InvalidParametersHelp"); }
    }

    private async void Execute_Click(object sender, RoutedEventArgs e)
    {
        if (_runner.IsRunning)
        {
            if (MessageBox.Show(this, LocExtension.Get("Operation.StopConfirm"), LocExtension.Get("Operation.StopTitle"), MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) _cancellation?.Cancel();
            return;
        }
        _cancellation = new CancellationTokenSource();
        ExecuteButton.Content = LocExtension.Get("Common.Stop");
        RawOutput.Clear();
        Summary.Text = L("Tool.Running");
        var progress = new Progress<GwOutputLine>(line => { RawOutput.AppendText(line.Text + Environment.NewLine); RawOutput.ScrollToEnd(); if (_consoleLog is not null) _ = _consoleLog.AppendAsync(line.Text); });
        try
        {
            if (_verb == "update" && Checked("bootloader") && MessageBox.Show(this, L("Tool.BootloaderWarning"), L("Tool.BootloaderTitle"), MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            var command = BuildCommand();
            if (_consoleLog is not null) await _consoleLog.BeginAsync(_verb, command.ToDisplayString());
            var result = await _runner.RunAsync(command, progress, _cancellation.Token);
            if (_verb == "info")
            {
                var info = GwInfoParser.Parse(string.Join(Environment.NewLine, result.Output.Select(x => x.Text)));
                Summary.Text = $"{info.Model ?? L("Tool.ControllerFallback")} · {info.FirmwareVersion ?? L("Tool.FirmwareUnknown")} · {info.Port ?? L("Tool.PortUnknown")}" + (info.HasNetworkWarning ? "\n" + L("Tool.NetworkWarning") : "");
            }
            else Summary.Text = result.IsSuccess ? L("Operation.Succeeded") : result.WasCancelled ? L("Operation.Cancelled") : LocExtension.Get("Operation.ExitCode", result.ExitCode);
            if (_consoleLog is not null) await _consoleLog.AppendAsync(Summary.Text);
        }
        catch (Exception exception)
        {
            var path = ErrorLog.Write(exception, $"Running GW tool '{_verb}'");
            var detail = path is null ? L("Common.Unknown") : LocExtension.Get("Error.LogSaved", path);
            Summary.Text = LocExtension.Get("Error.Unexpected", detail);
            if (_consoleLog is not null) await _consoleLog.AppendAsync(Summary.Text);
        }
        finally { ExecuteButton.Content = LocExtension.Get("Common.Execute"); _cancellation.Dispose(); _cancellation = null; }
    }

    private static string L(string key) => LocExtension.Get(key);
    private static string TitleFor(string verb) => verb switch
    {
        "info" => L("Tool.Title.Info"), "bandwidth" => L("Tool.Title.Bandwidth"), "rpm" => L("Tool.Title.Rpm"), "seek" => L("Tool.Title.Seek"),
        "pin" => L("Tool.Title.Pin"), "reset" => L("Tool.Title.Reset"), "delays" => L("Tool.Title.Delays"), "update" => L("Tool.Title.Update"), "align" => L("Tool.Title.Align"), _ => verb
    };
}
