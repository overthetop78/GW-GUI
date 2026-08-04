using System.Windows;
using GWGUI.Domain.Commands;
using GWGUI.Domain.Hardware;
using GWGUI.Infrastructure.Processes;

namespace GWGUI.App;

public partial class GwToolWindow : Window
{
    private readonly string _executable;
    private readonly string _verb;
    private readonly IGreaseweazleRunner _runner = new GreaseweazleRunner();
    private CancellationTokenSource? _cancellation;

    public GwToolWindow(string executable, string verb)
    {
        InitializeComponent();
        _executable = executable;
        _verb = verb;
        Heading.Text = Title = TitleFor(verb);
        CommandText.Text = new GwCommand(executable, verb, []).ToDisplayString();
    }

    private async void Execute_Click(object sender, RoutedEventArgs e)
    {
        if (_runner.IsRunning) { _cancellation?.Cancel(); return; }
        _cancellation = new CancellationTokenSource();
        ExecuteButton.Content = "Arrêter";
        RawOutput.Clear();
        Summary.Text = "Commande en cours…";
        var progress = new Progress<GwOutputLine>(line => { RawOutput.AppendText(line.Text + Environment.NewLine); RawOutput.ScrollToEnd(); });
        try
        {
            var command = new GwCommand(_executable, _verb, []);
            var result = await _runner.RunAsync(command, progress, _cancellation.Token);
            if (_verb == "info")
            {
                var info = GwInfoParser.Parse(string.Join(Environment.NewLine, result.Output.Select(x => x.Text)));
                Summary.Text = $"{info.Model ?? "Contrôleur"} · {info.FirmwareVersion ?? "Firmware inconnu"} · {info.Port ?? "Port inconnu"}" + (info.HasNetworkWarning ? "\nLes informations locales sont valides; seule la vérification réseau a échoué." : "");
            }
            else Summary.Text = result.IsSuccess ? "Commande terminée avec succès." : result.WasCancelled ? "Commande interrompue." : $"La commande s’est terminée avec le code {result.ExitCode}.";
        }
        catch (Exception exception) { Summary.Text = exception.Message; }
        finally { ExecuteButton.Content = "Exécuter"; _cancellation.Dispose(); _cancellation = null; }
    }

    private static string TitleFor(string verb) => verb switch
    {
        "info" => "Informations du contrôleur", "bandwidth" => "Bande passante USB", "rpm" => "Vitesse du lecteur", "seek" => "Déplacer la tête",
        "pin" => "Broches matérielles", "reset" => "Réinitialiser le contrôleur", "delays" => "Temporisations", "update" => "Firmware", _ => verb
    };
}
