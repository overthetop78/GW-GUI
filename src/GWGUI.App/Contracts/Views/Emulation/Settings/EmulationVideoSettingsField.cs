using System.Windows;

namespace GWGUI.App.Contracts.Views.Emulation.Settings;

internal sealed record EmulationVideoSettingsField(
    string Label,
    FrameworkElement Control,
    int ColumnSpan = 1,
    bool IsTrailingCheckBox = false);
