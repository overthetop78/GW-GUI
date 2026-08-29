using System.Windows.Controls;
using GWGUI.App.Contracts.Emulation.Settings;

namespace GWGUI.App.Contracts.Views.Emulation.Settings;

internal sealed record EmulationCpuSettingsContent(
    EmulationSettingsControlField CpuModel,
    TextBlock CpuSummary,
    EmulationSettingsControlField? Precision,
    EmulationSettingsControlField? Fpu,
    EmulationSettingsControlField OriginalSpeed,
    EmulationSettingsControlField? CpuSpeed);
