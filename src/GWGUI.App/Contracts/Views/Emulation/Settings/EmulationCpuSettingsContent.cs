using System.Windows;
using System.Windows.Controls;

namespace GWGUI.App.Contracts.Views.Emulation.Settings;

internal sealed record EmulationCpuSettingsContent(
    FrameworkElement CpuModel,
    TextBlock CpuSummary,
    FrameworkElement? Precision,
    FrameworkElement? Fpu,
    TextBlock OriginalSpeed,
    FrameworkElement? CpuSpeed);
