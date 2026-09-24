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

internal sealed class AvailableModuleRow
{
    internal AvailableModuleRow(string id, string displayName, string catalogUrl,
        string detailsLabel, bool isInstalled, bool isDownloaded, bool canInstall, string stateLabel)
    {
        Id = id;
        DisplayName = displayName;
        CatalogUrl = catalogUrl;
        DetailsLabel = detailsLabel;
        IsInstalled = isInstalled;
        IsDownloaded = isDownloaded;
        CanInstall = canInstall;
        StateLabel = stateLabel;
        var state = isInstalled || isDownloaded ? UpdateVisualState.Current
            : canInstall ? UpdateVisualState.Available : UpdateVisualState.Error;
        (StateForeground, StateBackground) = UpdateStateBrushes.For(state);
        StateIcon = StateIconFor(state);
    }

    public string Id { get; }
    public string DisplayName { get; }
    public string CatalogUrl { get; }
    public string DetailsLabel { get; }
    public bool IsInstalled { get; }
    public bool IsDownloaded { get; }
    public bool ShowsStateBadge => IsInstalled || IsDownloaded;
    public bool CanInstall { get; }
    public string StateLabel { get; }
    public System.Windows.Media.Brush StateForeground { get; }
    public System.Windows.Media.Brush StateBackground { get; }
    public string StateIcon { get; }

    private static string StateIconFor(UpdateVisualState state) => state switch
    {
        UpdateVisualState.Current => IconGlyphs.Current,
        UpdateVisualState.Available => IconGlyphs.Sync,
        UpdateVisualState.Busy => IconGlyphs.Sync,
        UpdateVisualState.Error => IconGlyphs.Error,
        _ => IconGlyphs.Controller
    };
}
