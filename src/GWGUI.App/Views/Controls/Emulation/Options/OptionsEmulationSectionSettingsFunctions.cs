using GWGUI.Domain.Settings.Emulation;
using GWGUI.App.Services.Storage;
using System.Windows.Controls;
using GWGUI.Emulation;
using Microsoft.Win32;

namespace GWGUI.App.Views.Controls.Emulation.Options;

public sealed partial class OptionsEmulationSection
{
    private async Task BrowseFolderAsync(TextBox target)
    {
        var dialog = new OpenFolderDialog { InitialDirectory = target.Text };
        if (dialog.ShowDialog() != true) return;
        target.Text = dialog.FolderName;
        await SaveFoldersAsync();
    }

    private async Task SaveFoldersAsync()
    {
        if (_settings is null) return;
        _settings.EmulationStorageFolder = _storageFolder.Text;
        _settings.EmulationCaptureFolder = _captureFolder.Text;
        _settings.EmulationStateFolder = _stateFolder.Text;
        StoragePaths.ConfigureEmulationStorageDirectory(_settings.EmulationStorageFolder);
        StoragePaths.ConfigureEmulationCaptureDirectory(_settings.EmulationCaptureFolder);
        StoragePaths.ConfigureEmulationStateDirectory(_settings.EmulationStateFolder);
        if (_persistSettings is not null) await _persistSettings();
    }

    private async Task SaveShortcutsAsync()
    {
        if (_settings is null) return;
        _settings.EmulationShortcuts = _shortcuts.Rows.ToDictionary(item => item.Id,
            item => item.Binding, StringComparer.Ordinal);
        if (_persistSettings is not null) await _persistSettings();
    }

    private static IReadOnlyList<InputBindingDefinition> GlobalShortcutDefinitions() =>
    [
        Shortcut(EmulationShortcutDefaults.ReleaseMouse, "Emulation.Shortcut.ReleaseMouse"),
        Shortcut(EmulationShortcutDefaults.PauseResume, "Emulation.Shortcut.PauseResume"),
        Shortcut(EmulationShortcutDefaults.ToggleFullscreen, "Emulation.Shortcut.Fullscreen"),
        Shortcut(EmulationShortcutDefaults.Power, "Emulation.Shortcut.Power"),
        Shortcut(EmulationShortcutDefaults.SoftReset, "Emulation.Shortcut.SoftReset"),
        Shortcut(EmulationShortcutDefaults.HardReset, "Emulation.Shortcut.HardReset"),
        Shortcut(EmulationShortcutDefaults.QuickSave, "Emulation.Shortcut.QuickSave"),
        Shortcut(EmulationShortcutDefaults.QuickLoad, "Emulation.Shortcut.QuickLoad"),
        Shortcut(EmulationShortcutDefaults.Screenshot, "Emulation.Shortcut.Screenshot"),
        Shortcut(EmulationShortcutDefaults.ToggleMute, "Emulation.Shortcut.Mute"),
        Shortcut(EmulationShortcutDefaults.FastForward, "Emulation.Shortcut.FastForward"),
        Shortcut(EmulationShortcutDefaults.InsertMedia, "Emulation.Media.Insert"),
        Shortcut(EmulationShortcutDefaults.EjectMedia, "Emulation.Media.Eject")
    ];

    private static InputBindingDefinition Shortcut(string id, string resourceKey) =>
        new(id, resourceKey, EmulationShortcutDefaults.Values[id]);
}
