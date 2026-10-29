using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Automation;
using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Contracts.Emulation.Firmware;
using GWGUI.App.Functions.Views.Emulation.Settings;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Views.Dialogs.Emulation;
using GWGUI.Emulation;

namespace GWGUI.App.Controllers.Emulation.Firmware;

internal sealed class EmulationFirmwareManagementController : IAsyncDisposable
{
    private readonly IEmulationModule _module;
    private readonly IEmulationFirmwareManager _manager;
    private readonly Func<IEmulationConfiguration> _getConfiguration;
    private readonly Dictionary<string, (EmulationSettingsField Field, TextBlock Name)> _views = new(StringComparer.Ordinal);
    private IReadOnlyList<EmulationFirmwareCandidate> _entries = [];
    private int _generation;
    private readonly CancellationTokenSource _lifetime = new();
    private readonly HashSet<Task> _pending = [];
    private FirmwareSelectionDialog? _dialog;
    private bool _disposed;

    internal EmulationFirmwareManagementController(IEmulationModule module, IEmulationFirmwareManager manager, Func<IEmulationConfiguration> getConfiguration)
    {
        _module = module;
        _manager = manager;
        _getConfiguration = getConfiguration;
    }

    internal void ClearViews()
    {
        _generation++;
        _views.Clear();
    }

    /// <summary>Le chemin reste dans la configuration ; seul le nom est présenté dans la page.</summary>
    internal Grid CreateField(EmulationSettingsField field, Func<Task> browse)
    {
        var name = new TextBlock
        {
            Text = DisplayName(field), ToolTip = field.Value, TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 6, 12, 6)
        };
        var button = new Button { Content = LocExtension.Get(ControlVisualConstants.BrowseResource), MinWidth = 100 };
        button.Click += async (_, _) => await browse();
        AutomationProperties.SetAutomationId(button, "BrowseFirmware:" + field.Id);
        var row = new Grid { Tag = (Func<string>)(() => field.Value ?? string.Empty) };
        row.ColumnDefinitions.Add(new ColumnDefinition());
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.Children.Add(name);
        Grid.SetColumn(button, 1);
        row.Children.Add(button);
        _views[field.Id] = (field, name);
        return row;
    }

    internal async Task RefreshAsync()
    {
        if (_disposed || _views.Count == 0) return;
        var configuration = _getConfiguration();
        var generation = _generation;
        var entries = await ScanAsync(configuration, CancellationToken.None);
        if (generation != _generation || configuration.Id != _getConfiguration().Id || configuration.MachineId != _getConfiguration().MachineId) return;
        _entries = entries;
        UpdateNames();
    }

    internal string GetFirmwareDirectory() => _manager.GetFirmwareDirectory(_getConfiguration().MachineId);

    internal async Task<IEmulationConfiguration?> SelectAsync(Window? owner, EmulationSettingsField field, Action<Exception> showError)
    {
        var configuration = _getConfiguration();
        if (_disposed || !IsAvailable(field.Id)) return null;
        var content = new EmulationFirmwareSettingsContent(
            LocExtension.GetForModule(_module, field.LabelResourceKey), field.Value ?? string.Empty,
            async token =>
            {
                var entries = await ScanAsync(configuration, token);
                token.ThrowIfCancellationRequested();
                _entries = entries;
                UpdateNames();
                return new[] { new EmulationFirmwareCandidate("none:" + field.Id, string.Empty,
                    LocExtension.Get("Emulation.Firmware.NotSelected"), null, EmulationFirmwareCompatibility.Unknown, field.Id) }
                    .Concat(CandidatesForField(entries, field.Id)).ToArray();
            }, OpenFolderAsync, candidate => CanUse(field.Id, candidate), showError, candidate =>
            {
                var destination = _module.Describe(configuration.MachineId, configuration).Blocks
                    .SelectMany(block => block.Fields).FirstOrDefault(item => item.Id == candidate.DestinationFieldId);
                return destination is null ? string.Empty : LocExtension.GetForModule(_module, destination.LabelResourceKey);
            });
        FirmwareSelectionDialog? dialog = null;
        try
        {
            dialog = new FirmwareSelectionDialog(content, owner);
            _dialog = dialog;
            if (dialog.ShowDialog() != true || dialog.SelectedFirmware is not { } selected) return null;
            if (configuration.Id != _getConfiguration().Id || configuration.MachineId != _getConfiguration().MachineId || !CanUse(field.Id, selected)) return null;
            // Les fichiers non identifiés suivent la validation du champ, comme l'ancien choix de fichier.
            if (selected.Compatibility == EmulationFirmwareCompatibility.Unknown)
                return _module.ApplySettings(configuration, new Dictionary<string, string?> { [field.Id] = selected.Path });
            return await RunAsync(_ => Task.FromResult(_manager.UseFirmware(configuration, selected)), CancellationToken.None);
        }
        finally
        {
            if (dialog is not null) await dialog.DisposeAsync();
            if (ReferenceEquals(_dialog, dialog)) _dialog = null;
        }
    }

    /// <summary>Un fichier apparaît une seule fois ; une ROM reconnue pour un autre rôle reste visible, mais inutilisable.</summary>
    internal static IReadOnlyList<EmulationFirmwareCandidate> CandidatesForField(IReadOnlyList<EmulationFirmwareCandidate> entries, string fieldId) => entries
        .GroupBy(candidate => candidate.Path, StringComparer.OrdinalIgnoreCase)
        .Select(group => group.OrderBy(candidate => candidate.DestinationFieldId == fieldId ? 0
                : candidate.DestinationFieldId is null && candidate.Compatibility == EmulationFirmwareCompatibility.Unknown ? 1 : 2)
            .ThenBy(candidate => EmulationSettingsLayout.FirmwareCompatibilityOrder(candidate.Compatibility)).First())
        .Select(candidate => candidate.DestinationFieldId == fieldId ? candidate
            : candidate.DestinationFieldId is null && candidate.Compatibility == EmulationFirmwareCompatibility.Unknown
                ? candidate with { DestinationFieldId = fieldId }
                : candidate with { Compatibility = EmulationFirmwareCompatibility.Incompatible })
        .OrderBy(candidate => EmulationSettingsLayout.FirmwareCompatibilityOrder(candidate.Compatibility))
        .ThenBy(candidate => candidate.DisplayName, StringComparer.CurrentCultureIgnoreCase).ToArray();

    private Task<IReadOnlyList<EmulationFirmwareCandidate>> ScanAsync(IEmulationConfiguration configuration, CancellationToken token) =>
        RunAsync(async cancellation => await _manager.ScanFirmwareAsync(configuration.MachineId, configuration, cancellation), token);

    private async Task<T> RunAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken token)
    {
        if (_disposed) throw new OperationCanceledException(new CancellationToken(true));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token, token);
        var task = Task.Run(() => action(linked.Token), linked.Token);
        _pending.Add(task);
        try { return await task; }
        finally { _pending.Remove(task); }
    }

    private bool CanUse(string fieldId, EmulationFirmwareCandidate candidate) =>
        !_disposed && candidate.DestinationFieldId == fieldId && candidate.Compatibility != EmulationFirmwareCompatibility.Incompatible && IsAvailable(fieldId);

    private bool IsAvailable(string fieldId)
    {
        var configuration = _getConfiguration();
        return _module.Describe(configuration.MachineId, configuration).Blocks.Where(block => block.IsVisible)
            .SelectMany(block => block.Fields).Any(field => field.Id == fieldId && field.IsVisible && field.IsEnabled);
    }

    private string DisplayName(EmulationSettingsField field)
    {
        if (string.IsNullOrWhiteSpace(field.Value)) return LocExtension.Get("Emulation.Firmware.NotSelected");
        var candidate = _entries.FirstOrDefault(entry => entry.DestinationFieldId == field.Id && string.Equals(entry.Path, field.Value, StringComparison.OrdinalIgnoreCase));
        return candidate?.DisplayName ?? Path.GetFileName(field.Value);
    }

    private void UpdateNames()
    {
        foreach (var (field, name) in _views.Values) name.Text = DisplayName(field);
    }

    private Task OpenFolderAsync()
    {
        Process? request = null;
        try
        {
            var directory = GetFirmwareDirectory();
            Directory.CreateDirectory(directory);
            request = Process.Start(new ProcessStartInfo(directory) { UseShellExecute = true });
        }
        finally
        {
            // Le bureau possède Explorer ; seul le handle de notre requête nous appartient.
            request?.Dispose();
        }
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        try
        {
            _lifetime.Cancel();
            if (_dialog is not null) await _dialog.DisposeAsync();
        }
        finally
        {
            try { await Task.WhenAll(_pending.ToArray()); }
            catch (OperationCanceledException) { }
            finally
            {
                ClearViews();
                _entries = [];
                _dialog = null;
                _lifetime.Dispose();
            }
        }
    }
}
