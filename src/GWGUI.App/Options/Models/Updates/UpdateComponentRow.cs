using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using GWGUI.App.Services.Updates;
using GWGUI.App.Services.Emulation;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Views.Controls.Options;
using GWGUI.Updates.Contracts;
using GWGUI.App.Contracts.Updates;
using GWGUI.App.Constants.Controls.Visual;

namespace GWGUI.App.Options.Models.Updates;

internal sealed class UpdateComponentRow
{
    internal UpdateComponentRow(string id, UpdateComponentKind kind, string displayName, string installedLabel,
        string statusLabel, IReadOnlyList<string> versions, string? selectedVersion, bool canSelectVersion,
        UpdateVisualState state)
    {
        Id = id;
        Kind = kind;
        DisplayName = displayName;
        InstalledLabel = installedLabel;
        StatusLabel = statusLabel;
        Versions = versions;
        SelectedVersion = selectedVersion;
        CanSelectVersion = canSelectVersion;
        (StateForeground, StateBackground) = UpdateStateBrushes.For(state);
        StateIcon = state switch
        {
            UpdateVisualState.Current => IconGlyphs.Current,
            UpdateVisualState.Available => IconGlyphs.Sync,
            UpdateVisualState.Busy => IconGlyphs.Sync,
            UpdateVisualState.Error => IconGlyphs.Error,
            _ => IconGlyphs.Controller
        };
    }

    public string Id { get; }
    public UpdateComponentKind Kind { get; }
    public string DisplayName { get; }
    public string InstalledLabel { get; }
    public string StatusLabel { get; }
    public IReadOnlyList<string> Versions { get; }
    public string? SelectedVersion { get; set; }
    public bool CanSelectVersion { get; }
    public System.Windows.Media.Brush StateForeground { get; }
    public System.Windows.Media.Brush StateBackground { get; }
    public string StateIcon { get; }
}
